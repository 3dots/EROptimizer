using EROptimizer.Dto;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using ScrapeWiki;
using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EROptimizer.Hubs
{
    public class ScrapeWikiHub : Hub, IProgressConsole
    {
        private readonly string _staticDataFileName = "ArmorData.json";
        private readonly string _staticDataTempFileName = "ArmorDataTemp.json";

        private readonly IConfiguration _configuration;
        public ScrapeWikiHub(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task StartScrape(string password)
        {
            if (_configuration["AdminPassword"] != password)
            {
                await Clients.Caller.SendAsync("Denied");
                return;
            }

            Scraper scraper;
            if (_configuration["ScrapeWiki:UseStaticHtmlFiles"] == true.ToString())
                scraper = new Scraper(this, _configuration["ScrapeWiki:FilesPath"], false, true);
            else
                scraper = new Scraper(this);

            bool success = await scraper.Scrape();
            if (success) await Evaluate(scraper);
        }

        public async Task WriteLine(string s)
        {
            await Clients.Caller.SendAsync(nameof(WriteLine), s);
        }

        public async Task DataSave()
        {
            string path = _configuration["StaticDataPath"];
            string staticDataFileName = _staticDataFileName;
            string staticDataTempFileName = _staticDataTempFileName;

            try
            {
                File.Delete(Path.Combine(path, staticDataFileName));
                File.Move(Path.Combine(path, staticDataTempFileName), Path.Combine(path, staticDataFileName));
                await WriteLine($"Replaced {staticDataFileName} with new data from {staticDataTempFileName}.");
                await ScrapeEnd();
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        async Task ScrapeEnd()
        {
            await Clients.Caller.SendAsync(nameof(ScrapeEnd));
        }

        async Task Evaluate(Scraper s)
        {
            var newData = new ArmorDataDto();

            int index = 0;

            var emptySetIdList = new List<int>() { 0 };

            ArmorPieceDto emptyHead = new ArmorPieceDto() { ArmorSetIds = emptySetIdList, ArmorPieceId = index++, Name = "None", Type = ArmorPieceTypeEnum.Head };
            ArmorPieceDto emptyChest = new ArmorPieceDto() { ArmorSetIds = emptySetIdList, ArmorPieceId = index++, Name = "None", Type = ArmorPieceTypeEnum.Chest };
            ArmorPieceDto emptyGauntlets = new ArmorPieceDto() { ArmorSetIds = emptySetIdList, ArmorPieceId = index++, Name = "None", Type = ArmorPieceTypeEnum.Gauntlets };
            ArmorPieceDto emptyLegs = new ArmorPieceDto() { ArmorSetIds = emptySetIdList, ArmorPieceId = index++, Name = "None", Type = ArmorPieceTypeEnum.Legs };

            newData.Head.Add(emptyHead);
            newData.Head.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Head).Select(x => new ArmorPieceDto(x, index++)));

            newData.Chest.Add(emptyChest);
            newData.Chest.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Chest).Select(x => new ArmorPieceDto(x, index++)));

            newData.Gauntlets.Add(emptyGauntlets);
            newData.Gauntlets.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Gauntlets).Select(x => new ArmorPieceDto(x, index++)));

            newData.Legs.Add(emptyLegs);
            newData.Legs.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Legs).Select(x => new ArmorPieceDto(x, index++)));

            newData.ArmorSets.Add(new ArmorSetDto() { ArmorSetId = 0, Name = "None" });
            newData.ArmorSets.AddRange(s.ArmorSets.Select(x => (ArmorSetDto)x));

            string path = _configuration["StaticDataPath"];
            string staticDataTempFileName = _staticDataTempFileName;

            try
            {
                await WriteLine($"Writing temporary file.");
                await WriteStaticDataFile(newData, staticDataTempFileName);
            }
            catch (Exception e)
            {
                await HandleException(e);
                return;
            }                     

            string staticDataFileName = _staticDataFileName;
            try
            {
                await WriteLine($"Reading {staticDataFileName} at {path}");

                using FileStream fileStream = File.OpenRead(Path.Combine(path, staticDataFileName));
                ArmorDataDto existingData = await JsonSerializer.DeserializeAsync<ArmorDataDto>(fileStream);
                await fileStream.DisposeAsync();

                await Clients.Caller.SendAsync("DataRetrieved", CalculateDiff(existingData, newData));
            }
            catch (FileNotFoundException)
            {
                await WriteLine($"{staticDataFileName} not found.");

                try
                {
                    await WriteStaticDataFile(newData, staticDataFileName);
                    await ScrapeEnd();
                }
                catch (Exception e)
                {
                    await HandleException(e);
                }
            }
            catch (Exception e)
            {
                await HandleException(e);
            }
        }

        async Task HandleException(Exception e)
        {
            await WriteLine($"Fail. Exception:");
            await WriteLine(e.ToString());
        }

        async Task WriteStaticDataFile(ArmorDataDto data, string fileName)
        {
            data.ArmorSets.ForEach(x => x.ArmorPieces.Clear()); //dont double store the data in the .json

            string path = _configuration["StaticDataPath"];

            await WriteLine($"Writing {fileName} at {path}");

            await File.WriteAllTextAsync(Path.Combine(path, fileName), JsonSerializer.Serialize(data));
            await WriteLine($"Success.");
        }

        private ArmorDataChangesDto CalculateDiff(ArmorDataDto existingData, ArmorDataDto newData)
        {
            RestoreSetPieceLists(existingData);
            RestoreSetPieceLists(newData);

            var changes = new ArmorDataChangesDto();

            changes.Messages.AddRange(existingData.ArmorSets.Where(x => !newData.ArmorSets.Any(n => n.Name == x.Name))
                .Select(x => $"New data is missing armor set: {x.Name}"));

            foreach (ArmorSetDto newSet in newData.ArmorSets)
            {
                ArmorSetDto existingSet = existingData.ArmorSets.FirstOrDefault(x => x.Name == newSet.Name);
                if (existingSet == null)
                {
                    changes.Messages.Add(@$"New armor set: {newSet.Name} - 
                                            Head: {newSet.ArmorPieces.FirstOrDefault(p => p.Type == ArmorPieceTypeEnum.Head)?.Name} - 
                                            Chest: {newSet.ArmorPieces.FirstOrDefault(p => p.Type == ArmorPieceTypeEnum.Chest)?.Name} -
                                            Gauntlets: {newSet.ArmorPieces.FirstOrDefault(p => p.Type == ArmorPieceTypeEnum.Gauntlets)?.Name} -
                                            Legs: {newSet.ArmorPieces.FirstOrDefault(p => p.Type == ArmorPieceTypeEnum.Legs)?.Name}");
                }
                else
                {
                    CheckArmorPieceType(changes, existingSet, newSet, ArmorPieceTypeEnum.Head);
                    CheckArmorPieceType(changes, existingSet, newSet, ArmorPieceTypeEnum.Chest);
                    CheckArmorPieceType(changes, existingSet, newSet, ArmorPieceTypeEnum.Gauntlets);
                    CheckArmorPieceType(changes, existingSet, newSet, ArmorPieceTypeEnum.Legs);
                }
            }

            if (changes.Messages.Count == 0 && changes.ArmorPieceChanges.Count == 0)
                changes.Messages.Add("No data changes.");

            return changes;
        }

        private void RestoreSetPieceLists(ArmorDataDto dto)
        {
            dto.ArmorSets.ForEach(x =>
            {
                x.ArmorPieces.AddRange(dto.Head.Where(p => p.ArmorSetIds.Contains(x.ArmorSetId)));
                x.ArmorPieces.AddRange(dto.Chest.Where(p => p.ArmorSetIds.Contains(x.ArmorSetId)));
                x.ArmorPieces.AddRange(dto.Gauntlets.Where(p => p.ArmorSetIds.Contains(x.ArmorSetId)));
                x.ArmorPieces.AddRange(dto.Legs.Where(p => p.ArmorSetIds.Contains(x.ArmorSetId)));
            });
        }

        private void CheckArmorPieceType(ArmorDataChangesDto dto, ArmorSetDto existingSet, ArmorSetDto newSet, ArmorPieceTypeEnum type)
        {
            ArmorPieceDto existingPiece = existingSet.ArmorPieces.FirstOrDefault(x => x.Type == type);
            ArmorPieceDto newPiece = newSet.ArmorPieces.FirstOrDefault(x => x.Type == type);
            if (existingPiece == null && newPiece == null)
            {
                return;
            }
            else if (existingPiece == null && newPiece != null)
            {
                dto.Messages.Add($"Armor set: {existingSet.Name} has gained a {type} piece: {newPiece.Name}");
            }
            else if (existingPiece != null && newPiece == null)
            {
                dto.Messages.Add($"New data armor set: {existingSet.Name} is missing {type} piece: {existingPiece.Name}");
            }
            else
            {
                if (existingPiece.Name != newPiece.Name) 
                    dto.Messages.Add($"Armor set: {existingSet.Name} {type} piece has changed name from: {existingPiece.Name} to: {newPiece.Name}");
                else
                {
                    var pieceChanges = new ArmorPieceChangesDto() { SetName = existingSet.Name, PieceName = existingPiece.Name };

                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Physical, nameof(newPiece.Physical));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.PhysicalStrike, nameof(newPiece.PhysicalStrike));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.PhysicalSlash, nameof(newPiece.PhysicalSlash));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.PhysicalPierce, nameof(newPiece.PhysicalPierce));

                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Magic, nameof(newPiece.Magic));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Fire, nameof(newPiece.Fire));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Lightning, nameof(newPiece.Lightning));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Holy, nameof(newPiece.Holy));

                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Immunity, nameof(newPiece.Immunity));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Robustness, nameof(newPiece.Robustness));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Focus, nameof(newPiece.Focus));
                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Vitality, nameof(newPiece.Vitality));

                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Poise, nameof(newPiece.Poise));

                    CheckProperty(pieceChanges, existingPiece, newPiece, x => x.Weight, nameof(newPiece.Weight));

                    if (pieceChanges.Changes.Count > 0) dto.ArmorPieceChanges.Add(pieceChanges);
                }
            }
        }

        private void CheckProperty(ArmorPieceChangesDto dto, ArmorPieceDto existingPiece, ArmorPieceDto newPiece, 
                                    Func<ArmorPieceDto, double> compare, string propertyName)
        {
            if (compare(newPiece) != compare(existingPiece))
                dto.Changes.Add($"{propertyName}: {compare(existingPiece)} -> {compare(newPiece)}");
        }
    }
}
