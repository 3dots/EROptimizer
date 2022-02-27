using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    public class Scraper
    {
        private const string BaseUrl = "https://eldenring.wiki.fextralife.com";
        private const int MaxSimultaniousRequests = 4;
        private const int ScrapingPauseDuration = 500;

        private readonly IProgressConsole _console;
        private readonly string _filesPath;
        private readonly bool _createHtmlFilesInSource;
        private readonly bool _useStaticHtmlFiles;

        object armorSetIdLock = new object();
        int ArmorSetIdCounter { get; set; } = 1;

        public List<ArmorSet> ArmorSets { get; private set; } = new List<ArmorSet>();
        public List<ArmorPiece> ArmorPieces { get; private set; } = new List<ArmorPiece>();

        public List<string> ParsingContinueErrors { get; private set; } = new List<string>();

        public Scraper(IProgressConsole pc)
        {
            _console = pc;
        }

        public Scraper(IProgressConsole pc, string filesPath, bool createHtmlFilesInSource, bool useStaticFiles) : this(pc)
        {
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

            List<ArmorSet> setsCopy = ArmorSets.ToList();
            foreach (IEnumerable<ArmorSet> batch in setsCopy.Batch(MaxSimultaniousRequests))
            {
                Task<ArmorSet>[] tasks = batch.Select(set => ProcessSet(set)).ToArray();
                Task.WaitAll(tasks);

                foreach (Task<ArmorSet> task in tasks)
                {
                    if (task.Result != null)
                    {
                        ArmorSets.Remove(task.Result); //wiki data is bad, remove that set. 
                    }
                }

                if (!_useStaticHtmlFiles) Thread.Sleep(ScrapingPauseDuration);
            }
            ArmorPieces.AddRange(ArmorSets.SelectMany(x => x.ArmorPieces));

            await EnsureUniqueness();

            //Get data
            var types = new List<ArmorPieceTypeEnum>()
                { ArmorPieceTypeEnum.Head, ArmorPieceTypeEnum.Chest, ArmorPieceTypeEnum.Gauntlets, ArmorPieceTypeEnum.Legs };
            foreach (IEnumerable<ArmorPieceTypeEnum> batch in types.Batch(MaxSimultaniousRequests))
            {
                Task<List<ArmorSet>>[] tasks = batch.Select(type => GetData(type)).ToArray();
                Task.WaitAll(tasks);

                foreach (Task<List<ArmorSet>> t in tasks.Where(x => x.Result.Count > 0))
                {
                    ArmorSets.AddRange(t.Result);
                    ArmorPieces.AddRange(t.Result.SelectMany(x => x.ArmorPieces));
                }
                if (!_useStaticHtmlFiles) Thread.Sleep(ScrapingPauseDuration);
            }

            await EnsureUniqueness();

            foreach (string m in ParsingContinueErrors) await _console.WriteLine(m);

            await EnsureCorrectness();

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
                string setName = a.InnerText?.Replace("&nbsp;", " ")?.Trim();
                string setResourceName = a.Attributes["href"]?.Value;
                if (string.IsNullOrEmpty(setName)) throw new ScrapeParsingException(resourceName, "Empty Set name.");
                if (string.IsNullOrEmpty(setResourceName)) throw new ScrapeParsingException(resourceName, "Empty Set link.");

                await _console.WriteLine($"{setResourceName} {setName}");
                ArmorSets.Add(new ArmorSet() { ArmorSetId = ArmorSetIdCounter++, Name = setName, ResourceName = setResourceName });
            }
        }

        private async Task<ArmorSet> ProcessSet(ArmorSet set)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await GetHtml(set.ResourceName));

            string headerText = $"{set.Name} Armor Pieces in Elden Ring";
            IList<HtmlNode> h3s = htmlDoc.QuerySelectorAll("#wiki-content-block > h3");
            HtmlNode h3ArmorPieces = h3s.FirstOrDefault(x => x.InnerText == headerText);
            if (h3ArmorPieces == null)
            {
                ScrapeExceptionContinue(new ScrapeParsingException(set.ResourceName, $"h3 with \"{headerText}\" not found."));
                return set;
            }

            HtmlNode ul = h3ArmorPieces.NextSiblingElement();
            if (ul.Name != "ul") ul = ul.NextSiblingElement();
            if (ul.Name != "ul") throw new ScrapeParsingException(set.ResourceName, $"Could not find <ul> with armor pieces.");
            IList<HtmlNode> armorPieceAnchors = ul.QuerySelectorAll("a");

            foreach (HtmlNode a in armorPieceAnchors)
            {
                string name = a.InnerText?.Replace("&nbsp;", " ")?.Trim();
                if (string.IsNullOrEmpty(name)) throw new ScrapeParsingException(set.ResourceName, "Empty Piece name.");

                if (name == "Vulgar Militia Gauntlets")
                {
                    ScrapeExceptionContinue(new ScrapeParsingException("Manually removing Vulgar Militia Gauntlets"));
                    continue;
                } 
                else if (name == "Map Link")
                {
                    ScrapeExceptionContinue(new ScrapeParsingException(set.ResourceName, "Ignoring Map Link"));
                    continue;
                }

                set.ArmorPieces.Add(new ArmorPiece() { ArmorSetId = set.ArmorSetId, Name = name });
            }

            return null;
        }

        private async Task<List<ArmorSet>> GetData(ArmorPieceTypeEnum armorPieceType)
        {
            var standaloneArmorPieceSets = new List<ArmorSet>();

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

            ArmorPiece piece = null;

            foreach (HtmlNode tr in trs)
            {
                int i = 0;
                double value;
                ArmorPiece prevPiece = piece;
                piece = null;
                               
                foreach (HtmlNode td in tr.GetChildElements())
                {
                    if (i == 0)
                    {
                        HtmlNode node = td.QuerySelector("a");
                        if (node == null) node = td.QuerySelector("p");
                        string armorPieceName = node?.InnerText.Replace("&nbsp;", " ")?.Trim();

                        if (string.IsNullOrEmpty(armorPieceName)) node = node.NextSiblingElement(); //Image but no link
                        armorPieceName = node?.InnerText.Replace("&nbsp;", " ")?.Trim();

                        if (string.IsNullOrEmpty(armorPieceName))
                        {
                            throw new ScrapeParsingException(resourceName, "Armor piece name was blank in the table?");
                        }

                        if (armorPieceName == "Vulgar Militia Gauntlets")
                        {
                            ScrapeExceptionContinue(new ScrapeParsingException("Ignoring Vulgar Militia Gauntlets"));
                            break;
                        }

                        piece = ArmorPieces.FirstOrDefault(x => x.Name == armorPieceName);
                        if (piece == null)
                        {
                            await _console.WriteLine($"Standalone armor piece found: {armorPieceName}");

                            int newSetId;
                            lock (armorSetIdLock)
                            {
                                newSetId = ArmorSetIdCounter++;
                            }

                            piece = new ArmorPiece() { ArmorSetId = newSetId, Name = armorPieceName };
                            standaloneArmorPieceSets.Add(new ArmorSet() { ArmorSetId = newSetId, Name = armorPieceName, ArmorPieces = new List<ArmorPiece>() { piece } });
                        }

                        if (piece.IsProcessed) throw new ScrapeParsingException(resourceName, $"Attempted to process Armor Piece {armorPieceName} multiple times.");
                        piece.IsProcessed = true;
                        piece.Type = armorPieceType;
                    }
                    else if (i <= 13)
                    {
                        bool p;
                        string text = td.InnerText.Replace("&nbsp;", " ").Trim();
                        if (string.IsNullOrEmpty(text))
                        {
                            p = true;
                            value = 0;
                        }
                        else
                        {
                            p = double.TryParse(text, out value);
                        }

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
                        else if (i == 12) ParseTableCell(p, () => { piece.Vitality = value; }, nameof(piece.Vitality), piece.Name, resourceName);
                        else if (i == 13) ParseTableCell(p, () => { piece.Weight = value; }, nameof(piece.Weight), piece.Name, resourceName);
                    }

                    i++;
                }
            }

            return standaloneArmorPieceSets;
        }

        private void ParseTableCell(bool parseSucceded, Action assignValue, string propertyName, string armorPieceName, string resourceName)
        {
            if (parseSucceded)
                assignValue.Invoke();
            else
                throw new ScrapeParsingException(resourceName, $"Couldn't parse {propertyName} for Armor Piece: {armorPieceName}");
        }

        private async Task EnsureUniqueness()
        {
            if (ArmorSets.Select(x => x.Name).Distinct().Count() != ArmorSets.Count)
            {
                await _console.WriteLine("There are multiple armor sets with the same name:");
                foreach (ArmorSet s in ArmorSets.GroupBy(x => x.Name).Where(g => g.Count() > 1).Select(g => g.First()))
                {
                    await _console.WriteLine($"{s.Name}");
                }
                throw new ScrapeParsingException("Exiting.");
            }

            if (ArmorPieces.Select(x => x.Name).Distinct().Count() != ArmorPieces.Count)
            {
                await _console.WriteLine("There are multiple armor pieces with the same name:");
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
        }

        private async Task EnsureCorrectness()
        {
            foreach(ArmorSet set in ArmorSets.Where(x => x.ArmorPieces.Any(p => p.Type == null)))
            {
                List<ArmorPiece> setPiecesListCopy = set.ArmorPieces.ToList();
                foreach (ArmorPiece piece in setPiecesListCopy.Where(p => p.Type == null))
                {
                    await _console.WriteLine($"Did not process {set.Name}: {piece.Name}");
                    set.ArmorPieces.Remove(piece);
                    ArmorPieces.Remove(piece);
                }
            }

            foreach (ArmorSet set in ArmorSets.Where(x => x.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1).Any()))
            {
                foreach(IGrouping<ArmorPieceTypeEnum?, ArmorPiece> group in set.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1))
                {
                    foreach(ArmorPiece piece in group)
                    {
                        await _console.WriteLine($"Multiple pieces of the same type. {set.Name}, {group.Key}, {piece.Name}");
                    }
                }
            }
        }

        private void ScrapeExceptionContinue(ScrapeParsingException acceptableException)
        {
            ParsingContinueErrors.Add(acceptableException.Message);
        }
    }

    class ScrapeParsingException : Exception
    {
        public ScrapeParsingException(string message) : base(message) { }

        public ScrapeParsingException(string resourceName, string message) : base($"{resourceName}: {message}") { }
    }
}
