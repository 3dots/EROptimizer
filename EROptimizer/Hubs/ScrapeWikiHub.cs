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

        private ArmorDataDto NewData { get; set; }

        private readonly IConfiguration _configuration;
        public ScrapeWikiHub(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task StartScrape()
        {
            Scraper scraper;
            if (_configuration["ScrapeWiki:UseStaticHtmlFiles"] == true.ToString())
                scraper = new Scraper(this, _configuration["ScrapeWiki:FilesPath"], false, true);
            else
                scraper = new Scraper(this);

            bool success = await scraper.Scrape();
            if (success) await CalculateDiff(scraper);
        }

        public async Task WriteLine(string s)
        {
            await Clients.Caller.SendAsync(nameof(WriteLine), s);
        }

        public async Task DataSave()
        {

        }

        async Task ScrapeEnd()
        {
            await Clients.Caller.SendAsync(nameof(ScrapeEnd));
        }

        async Task CalculateDiff(Scraper s)
        {
            var newData = NewData = new ArmorDataDto();

            ArmorPieceDto emptyHead = new ArmorPieceDto() { ArmorSetId = 0, Name = "None", Type = ArmorPieceTypeEnum.Head };
            ArmorPieceDto emptyChest = new ArmorPieceDto() { ArmorSetId = 0, Name = "None", Type = ArmorPieceTypeEnum.Chest };
            ArmorPieceDto emptyGauntlets = new ArmorPieceDto() { ArmorSetId = 0, Name = "None", Type = ArmorPieceTypeEnum.Gauntlets };
            ArmorPieceDto emptyLegs = new ArmorPieceDto() { ArmorSetId = 0, Name = "None", Type = ArmorPieceTypeEnum.Legs };

            newData.Head.Add(emptyHead);
            newData.Head.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Head).Select(x => (ArmorPieceDto)x));

            newData.Chest.Add(emptyChest);
            newData.Chest.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Chest).Select(x => (ArmorPieceDto)x));

            newData.Gauntlets.Add(emptyGauntlets);
            newData.Gauntlets.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Gauntlets).Select(x => (ArmorPieceDto)x));

            newData.Legs.Add(emptyLegs);
            newData.Legs.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Legs).Select(x => (ArmorPieceDto)x));

            newData.ArmorSets.Add(new ArmorSetDto() { ArmorSetId = 0, Name = "None" });
            newData.ArmorSets.AddRange(s.ArmorSets.Select(x => (ArmorSetDto)x));

            newData.ArmorSets.ForEach(x =>
            {
                x.ArmorPieces.AddRange(newData.Head.Where(p => p.ArmorSetId == x.ArmorSetId));
                x.ArmorPieces.AddRange(newData.Chest.Where(p => p.ArmorSetId == x.ArmorSetId));
                x.ArmorPieces.AddRange(newData.Gauntlets.Where(p => p.ArmorSetId == x.ArmorSetId));
                x.ArmorPieces.AddRange(newData.Legs.Where(p => p.ArmorSetId == x.ArmorSetId));
            });

            string path = _configuration["StaticDataPath"];
            string fileName = _staticDataFileName;

            try
            {
                await WriteLine($"Reading {fileName} at {path}");

                using FileStream fileStream = File.OpenRead(Path.Combine(path, fileName));
                ArmorDataDto existingData = await JsonSerializer.DeserializeAsync<ArmorDataDto>(fileStream);
                await fileStream.DisposeAsync();

                existingData.ArmorSets.ForEach(x =>
                {
                    x.ArmorPieces.AddRange(existingData.Head.Where(p => p.ArmorSetId == x.ArmorSetId));
                    x.ArmorPieces.AddRange(existingData.Chest.Where(p => p.ArmorSetId == x.ArmorSetId));
                    x.ArmorPieces.AddRange(existingData.Gauntlets.Where(p => p.ArmorSetId == x.ArmorSetId));
                    x.ArmorPieces.AddRange(existingData.Legs.Where(p => p.ArmorSetId == x.ArmorSetId));
                });

                var changes = new ArmorDataChangesDto();

                changes.Messages.AddRange(existingData.ArmorSets.Where(x => !newData.ArmorSets.Any(n => n.Name == x.Name))
                    .Select(x => $"New data is missing armor set: {x.Name}"));

                changes.Messages.AddRange(existingData.Head.Where(x => !newData.Head.Any(n => n.Name == x.Name))
                    .Select(x => $"New data is missing head piece: {x.Name} from set: {existingData.ArmorSets.First(s => s.ArmorSetId == x.ArmorSetId).Name }"));
                changes.Messages.AddRange(existingData.Chest.Where(x => !newData.Chest.Any(n => n.Name == x.Name))
                    .Select(x => $"New data is missing chest piece: {x.Name} from set: {existingData.ArmorSets.First(s => s.ArmorSetId == x.ArmorSetId).Name }"));
                changes.Messages.AddRange(existingData.Gauntlets.Where(x => !newData.Gauntlets.Any(n => n.Name == x.Name))
                    .Select(x => $"New data is missing gauntlets piece: {x.Name} from set: {existingData.ArmorSets.First(s => s.ArmorSetId == x.ArmorSetId).Name }"));
                changes.Messages.AddRange(existingData.Legs.Where(x => !newData.Legs.Any(n => n.Name == x.Name))
                    .Select(x => $"New data is missing legs piece: {x.Name} from set: {existingData.ArmorSets.First(s => s.ArmorSetId == x.ArmorSetId).Name }"));

                //changes.Messages.AddRange();

                await Clients.Caller.SendAsync("DataRetrieved", changes);
            }
            catch (FileNotFoundException)
            {
                await WriteLine($"{fileName} not found.");
                await WriteStaticDataFile(newData);
                await ScrapeEnd();
            }
            catch (Exception e)
            {
                await WriteLine($"Fail. Exception:");
                await WriteLine(e.ToString());
            }
        }

        async Task WriteStaticDataFile(ArmorDataDto data)
        {
            data.ArmorSets.ForEach(x => x.ArmorPieces.Clear()); //dont double store the data in the .json

            string path = _configuration["StaticDataPath"];
            string fileName = _staticDataFileName;

            await WriteLine($"Writing {fileName} at {path}");

            try
            {
                await File.WriteAllTextAsync(Path.Combine(path, fileName), JsonSerializer.Serialize(data));
                await WriteLine($"Success.");
            }
            catch (Exception e)
            {
                await WriteLine($"Fail. Exception:");
                await WriteLine(e.ToString());
            }
        }
    }
}
