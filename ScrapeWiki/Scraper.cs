﻿using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    public class Scraper
    {
        private const string BaseUrl = "https://eldenring.wiki.fextralife.com";
        private const int MaxSimultaniousRequests = 4;

        private readonly IProgressConsole _console;
        private readonly string _filesPath;
        private readonly bool _createHtmlFilesInSource;
        private readonly bool _useStaticHtmlFiles;

        object armorPieceIdLock = new object();
        int ArmorPieceIdCounter { get; set; } = 1;

        object armorSetIdLock = new object();
        int ArmorSetIdCounter { get; set; } = 1;

        public List<ArmorSet> ArmorSets { get; private set; } = new List<ArmorSet>();
        public List<ArmorPiece> ArmorPieces { get; private set; } = new List<ArmorPiece>();

        public Scraper(IProgressConsole pc)
        {

        }

        public Scraper(IProgressConsole pc, string filesPath, bool createHtmlFilesInSource, bool useStaticFiles)
        {
            _console = pc;
            _createHtmlFilesInSource = createHtmlFilesInSource;
            _useStaticHtmlFiles = useStaticFiles;
            _filesPath = filesPath;
        }

        public Scraper(IProgressConsole pc, string filesPath, string createHtmlFilesInSource, string useStaticFiles)
            : this(pc, filesPath, bool.Parse(createHtmlFilesInSource), bool.Parse(useStaticFiles))
        {

        }

        public async Task<bool> Scrape()
        {
            try
            {
                await BeginScrape();
                return true;
            }
            catch (Exception e)
            {
                await _console.WriteLine("Scape failed. Exception:");
                await _console.WriteLine(e.ToString());
                return false;
            }
        }

        private async Task<string> GetHtml(string resourceName) //Expects slash "/Armor"
        {
            string resourceFile = $"{resourceName.Replace("/", "")}.html";
            string url = $"{BaseUrl}{resourceName}";

            await _console.WriteLine($"Scraping {url}");

            string htmlString;
            if (_useStaticHtmlFiles)
            {
                htmlString = await File.ReadAllTextAsync(Path.Combine(_filesPath, resourceFile));
            }
            else
            {
                var c = new HttpClient();
                htmlString = await c.GetStringAsync(url);

                if (_createHtmlFilesInSource) await File.WriteAllTextAsync(Path.Combine(_filesPath, resourceFile), htmlString);
            }

            await _console.WriteLine($"Retrieved {url}");
            return htmlString;
        }

        private async Task BeginScrape()
        {
            await _console.WriteLine($"Begin Scrape.");
            if (_createHtmlFilesInSource) await _console.WriteLine("Creating HTML files in source.");
            if (_useStaticHtmlFiles) await _console.WriteLine($"Using static HTML files at {_filesPath}");

            //Getting armor set names
            await GetSets();

            // Adding armor pieces
            //await ProcessSet(ArmorSets.First());
            foreach (IEnumerable<ArmorSet> batch in ArmorSets.Batch(MaxSimultaniousRequests))
            {
                Task.WaitAll(batch.Select(set => ProcessSet(set)).ToArray());
            }
            ArmorPieces.AddRange(ArmorSets.SelectMany(x => x.ArmorPieces));

            //Ensure unquiness of all armor pieces
            if (ArmorPieces.Select(x => x.Name).Distinct().Count() != ArmorPieces.Count)
            {
                await _console.WriteLine("There are duplicate armor pieces:");
                foreach (IGrouping<string, ArmorPiece> group in ArmorPieces.GroupBy(x => x.Name).Where(g => g.Count() > 1))
                {
                    foreach (ArmorPiece a in group)
                    {
                        ArmorSet set = ArmorSets.FirstOrDefault(x => x.ArmorSetId == a.ArmorSetId);
                        await _console.WriteLine($"{set?.Name}: {a.Name}");
                    }
                }
                throw new ScrapeParsingException("Exiting.");
            }

            //Get data
            var types = new List<ArmorPieceTypeEnum>()
                { ArmorPieceTypeEnum.Head, ArmorPieceTypeEnum.Chest, ArmorPieceTypeEnum.Gauntlets, ArmorPieceTypeEnum.Legs };
            foreach (IEnumerable<ArmorPieceTypeEnum> batch in types.Batch(MaxSimultaniousRequests))
            {
                Task.WaitAll(batch.Select(type => GetData(type)).ToArray());
            }

            ArmorPieces = ArmorPieces.OrderBy(x => x.ArmorSetId).ToList();

            await _console.WriteLine("Successfully scraped data.");
        }

        private async Task GetSets()
        {
            string resourceName = "/Armor";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await GetHtml(resourceName));

            string headerText = "Elden Ring Armor Set Gallery";
            IList<HtmlNode> h2s = htmlDoc.QuerySelectorAll("#wiki-content-block > h2");
            HtmlNode h2ArmorSetGallery = h2s.FirstOrDefault(x => x.InnerText == headerText);
            if (h2ArmorSetGallery == null) throw new ScrapeParsingException(resourceName, $"h2 with \"{headerText}\" not found.");

            IList<HtmlNode> armorSetAnchors = h2ArmorSetGallery.NextSiblingElement().GetChildElements().First().QuerySelectorAll("a");

            ArmorSetIdCounter = 1;
            foreach (HtmlNode a in armorSetAnchors)
            {
                string setName = a.InnerText?.Replace("&nbsp;", "")?.Trim();
                string setResourceName = a.Attributes["href"]?.Value;
                if (string.IsNullOrEmpty(setName)) throw new ScrapeParsingException(resourceName, "Empty Set name.");
                if (string.IsNullOrEmpty(setResourceName)) throw new ScrapeParsingException(resourceName, "Empty Set link.");

                await _console.WriteLine($"{setResourceName} {setName}");
                ArmorSets.Add(new ArmorSet() { ArmorSetId = ArmorSetIdCounter++, Name = setName, ResourceName = setResourceName });
            }
        }

        private async Task ProcessSet(ArmorSet set)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await GetHtml(set.ResourceName));

            string headerText = $"{set.Name} Armor Pieces in Elden Ring";
            IList<HtmlNode> h3s = htmlDoc.QuerySelectorAll("#wiki-content-block > h3");
            HtmlNode h3ArmorPieces = h3s.FirstOrDefault(x => x.InnerText == headerText);
            if (h3ArmorPieces == null) throw new ScrapeParsingException(set.ResourceName, $"h3 with \"{headerText}\" not found.");

            HtmlNode ul = h3ArmorPieces.NextSiblingElement();
            IList<HtmlNode> armorPieceAnchors = ul.QuerySelectorAll("a");

            foreach (HtmlNode a in armorPieceAnchors)
            {
                string name = a.InnerText?.Replace("&nbsp;", "")?.Trim();
                if (string.IsNullOrEmpty(name)) throw new ScrapeParsingException(set.ResourceName, "Empty Piece name.");

                int id;
                lock (armorPieceIdLock)
                {
                    id = ArmorPieceIdCounter++;
                }

                set.ArmorPieces.Add(new ArmorPiece() { ArmorPieceId = id, ArmorSetId = set.ArmorSetId, Name = name });
            }
        }

        private async Task GetData(ArmorPieceTypeEnum armorPieceType)
        {
            string resourceName = null;
            switch (armorPieceType)
            {
                case ArmorPieceTypeEnum.Head:
                    resourceName = "/Helms";
                    break;
                case ArmorPieceTypeEnum.Chest:
                    resourceName = "/Chest+Armor";
                    break;
                case ArmorPieceTypeEnum.Gauntlets:
                    resourceName = "/Gauntlets";
                    break;
                case ArmorPieceTypeEnum.Legs:
                    resourceName = "/Leg+Armor";
                    break;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await GetHtml(resourceName));

            IList<HtmlNode> trs = htmlDoc.QuerySelectorAll("#wiki-content-block > div > table > tbody > tr");
            if (!(trs?.Count > 0)) throw new ScrapeParsingException(resourceName, $"Couldn't find table rows.");

            foreach (HtmlNode tr in trs)
            {
                int i = 0;
                double value;
                ArmorPiece piece = null;

                foreach (HtmlNode td in tr.GetChildElements())
                {
                    if (i == 0)
                    {
                        string armorPieceName = td.QuerySelector("a")?.InnerText.Replace("&nbsp;", "")?.Trim();
                        if (string.IsNullOrEmpty(armorPieceName)) throw new ScrapeParsingException(resourceName, "Armor piece name was blank in the table?");
                        piece = ArmorPieces.FirstOrDefault(x => x.Name == armorPieceName);
                        if (piece == null)
                        {
                            await _console.WriteLine($"Standalone armor piece found: {armorPieceName}");

                            int newSetId;
                            lock (armorSetIdLock)
                            {
                                newSetId = ArmorSetIdCounter++;
                            }

                            int newPieceId;
                            lock (armorPieceIdLock)
                            {
                                newPieceId = ArmorPieceIdCounter++;
                            }

                            piece = new ArmorPiece() { ArmorPieceId = newPieceId, ArmorSetId = newSetId, Name = armorPieceName };
                            ArmorPieces.Add(piece);
                            ArmorSets.Add(new ArmorSet() { ArmorSetId = newSetId, Name = armorPieceName, ArmorPieces = new List<ArmorPiece>() { piece } });
                        }

                        if (piece.IsProcessed) throw new ScrapeParsingException(resourceName, $"Attempted to process Armor Piece {armorPieceName} multiple times.");
                        piece.IsProcessed = true;
                        piece.Type = armorPieceType;
                    }
                    else if (i <= 13)
                    {
                        bool p = double.TryParse(td.InnerText.Replace("&nbsp;", ""), out value);
                        if (i == 1) ParseTableCell(p, () => { piece.Physical = value; }, nameof(piece.Physical), piece.Name, resourceName);
                        else if (i == 2) ParseTableCell(p, () => { piece.PhysicalStrike = value; }, nameof(piece.PhysicalStrike), piece.Name, resourceName);
                        else if (i == 3) ParseTableCell(p, () => { piece.PhysicalSlash = value; }, nameof(piece.PhysicalSlash), piece.Name, resourceName);
                        else if (i == 4) ParseTableCell(p, () => { piece.PhysicalPierce = value; }, nameof(piece.PhysicalPierce), piece.Name, resourceName);
                        else if (i == 5) ParseTableCell(p, () => { piece.Magic = value; }, nameof(piece.Magic), piece.Name, resourceName);
                        else if (i == 6) ParseTableCell(p, () => { piece.Fire = value; }, nameof(piece.Fire), piece.Name, resourceName);
                        else if (i == 7) ParseTableCell(p, () => { piece.Lightning = value; }, nameof(piece.Lightning), piece.Name, resourceName);
                        else if (i == 8) ParseTableCell(p, () => { piece.Holy = value; }, nameof(piece.Holy), piece.Name, resourceName);
                        else if (i == 9) ParseTableCell(p, () => { piece.Immunity = value; }, nameof(piece.Immunity), piece.Name, resourceName);
                        else if (i == 10) ParseTableCell(p, () => { piece.Robustness = value; }, nameof(piece.Robustness), piece.Name, resourceName);
                        else if (i == 11) ParseTableCell(p, () => { piece.Focus = value; }, nameof(piece.Focus), piece.Name, resourceName);
                        else if (i == 12) ParseTableCell(p, () => { piece.Death = value; }, nameof(piece.Death), piece.Name, resourceName);
                        else if (i == 13) ParseTableCell(p, () => { piece.Weight = value; }, nameof(piece.Weight), piece.Name, resourceName);
                    }

                    i++;
                }
            }
        }

        private void ParseTableCell(bool parseSucceded, Action assignValue, string propertyName, string armorPieceName, string resourceName)
        {
            if (parseSucceded)
                assignValue.Invoke();
            else
                throw new ScrapeParsingException(resourceName, $"Couldn't parse {propertyName} for Armor Piece: {armorPieceName}");
        }
    }

    class ScrapeParsingException : Exception
    {
        public ScrapeParsingException(string message) : base(message)
        {

        }

        public ScrapeParsingException(string resourceName, string message) : base($"{resourceName}: {message}")
        {

        }
    }

}