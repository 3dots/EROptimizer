using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using ScrapeWiki.Model;
using System;
using System.Collections.Concurrent;
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
        private const int ScrapingPauseDuration = 1000;

        private readonly IProgressConsole _console;
        private readonly string _filesPath;
        private readonly bool _createHtmlFilesInSource;
        private bool _useStaticHtmlFiles; //readonly

        object threadSafetyLock = new object();

        int ArmorSetIdCounter { get; set; } = 1;
        public List<ArmorSet> ArmorSets { get; private set; } = new List<ArmorSet>();
        public List<ArmorPiece> ArmorPieces { get; private set; } = new List<ArmorPiece>();

        public ConcurrentBag<string> ParsingContinueErrors { get; private set; } = new ConcurrentBag<string>();

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
                try
                {
                    htmlString = await File.ReadAllTextAsync(Path.Combine(_filesPath, resourceFile));
                }
                catch (FileNotFoundException)
                {
                    var c = new HttpClient();
                    htmlString = await c.GetStringAsync(url);

                    if (_createHtmlFilesInSource) await File.WriteAllTextAsync(Path.Combine(_filesPath, resourceFile), htmlString);

                    Thread.Sleep(ScrapingPauseDuration/MaxSimultaniousRequests);
                }
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
                Task.WaitAll(batch.Select(set => ProcessSet(set)).ToArray());
                if (!_useStaticHtmlFiles) Thread.Sleep(ScrapingPauseDuration);
            }

            await EnsureUniqueness();

            //_useStaticHtmlFiles = false;

            //Get data
            var types = new List<ArmorPieceTypeEnum>()
                { ArmorPieceTypeEnum.Head, ArmorPieceTypeEnum.Chest, ArmorPieceTypeEnum.Gauntlets, ArmorPieceTypeEnum.Legs };
            foreach (IEnumerable<ArmorPieceTypeEnum> batch in types.Batch(MaxSimultaniousRequests))
            {
                Task.WaitAll(batch.Select(type => GetData(type)).ToArray());
                if (!_useStaticHtmlFiles) Thread.Sleep(ScrapingPauseDuration);
            }

            await EnsureUniqueness();

            //foreach (string m in ParsingContinueErrors) await _console.WriteLine(m);

            await EnsureCorrectness();

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
                if (string.IsNullOrEmpty(setName))
                {
                    //ScrapeExceptionContinue(new ScrapeParsingException(resourceName, "Empty Set name."));
                    continue;
                }
                if (string.IsNullOrEmpty(setResourceName)) throw new ScrapeParsingException(resourceName, "Empty Set link.");

                if (new string[] { "Blackflame Set", "Erdtree Capital Set" }.Contains(setName))
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(resourceName, $"Ignoring {setName}"));
                    continue;
                }

                await _console.WriteLine($"{setResourceName} {setName}");

                //Dont need to lock here, since single thread
                ArmorSets.Add(new ArmorSet() { ArmorSetId = ArmorSetIdCounter++, Name = setName, ResourceName = setResourceName });
            }
        }

        private async Task ProcessSet(ArmorSet set)
        {
            string html = null;
            try
            {
                html = await GetHtml(set.ResourceName);
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await ScrapeExceptionContinue(new ScrapeParsingException(set.ResourceName, "404"));
                Thread.Sleep(ScrapingPauseDuration/MaxSimultaniousRequests);

                lock (threadSafetyLock)
                {
                    ArmorSets.Remove(set);
                }
                return;
            }
            catch
            {
                throw;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            string headerText = $"{set.Name} Armor Pieces in Elden Ring";
            IList<HtmlNode> h3s = htmlDoc.QuerySelectorAll("#wiki-content-block > h3");
            HtmlNode h3ArmorPieces = h3s.FirstOrDefault(x => new string[] {
                $"{set.Name} Armor Pieces in Elden Ring",
                $"{set.Name}Armor Pieces in Elden Ring",
                $"{set.Name} Pieces in Elden Ring",
                $"{set.Name} Armor Pieces"}.Contains(x.InnerText, StringComparer.InvariantCultureIgnoreCase));
            if (h3ArmorPieces == null)
            {
                throw new ScrapeParsingException(set.ResourceName, $"h3 with \"{headerText}\" not found.");
                //ScrapeExceptionContinue();
                //return set;
            }

            HtmlNode ul = h3ArmorPieces.NextSiblingElement();
            if (ul.Name != "ul") ul = ul.NextSiblingElement();
            if (ul.Name != "ul") throw new ScrapeParsingException(set.ResourceName, $"Could not find <ul> with armor pieces.");
            IList<HtmlNode> armorPieceAnchors = ul.QuerySelectorAll("a");

            foreach (HtmlNode a in armorPieceAnchors)
            {
                string name = a.InnerText?.Replace("&nbsp;", " ")?.Trim();
                if (string.IsNullOrEmpty(name)) throw new ScrapeParsingException(set.ResourceName, "Empty Piece name.");

                if (name == "Map Link")
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(set.ResourceName, "Ignoring Map Link"));
                    continue;
                }
                else if (name == "runes")
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(set.ResourceName, "Ignoring runes link"));
                    continue;
                }

                lock (threadSafetyLock)
                {
                    ArmorPiece existingArmorPiece = ArmorPieces.FirstOrDefault(a => a.Name == name);
                    if (existingArmorPiece == null)
                    {
                        ArmorPiece newPiece = new ArmorPiece() { Name = name };
                        newPiece.ArmorSetIds.Add(set.ArmorSetId);

                        ArmorPieces.Add(newPiece);
                        set.ArmorPieces.Add(newPiece);
                    }
                    else
                    {
                        existingArmorPiece.ArmorSetIds.Add(set.ArmorSetId);
                        set.ArmorPieces.Add(existingArmorPiece);
                    }
                }
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

            ArmorPiece piece = null;

            foreach (HtmlNode tr in trs)
            {
                int i = 0;
                double value;
                ArmorPiece prevPiece = piece;
                piece = null;
                bool triedFetchFromPieceLink = false;

                foreach (HtmlNode td in tr.GetChildElements())
                {
                    if (i == 0)
                    {
                        HtmlNode node = td.QuerySelector("a");
                        if (node == null) node = td.QuerySelector("p");
                        string armorPieceName = node?.InnerText.Replace("&nbsp;", " ")?.Trim();

                        if (string.IsNullOrEmpty(armorPieceName))
                        {
                            node = node.NextSiblingElement(); //Image but no link
                            armorPieceName = node?.InnerText.Replace("&nbsp;", " ")?.Trim();
                        }

                        if (string.IsNullOrEmpty(armorPieceName))
                        {
                            throw new ScrapeParsingException(resourceName, "Armor piece name was blank in the table?");
                        }

                        bool isStandAlonePiece = false;
                        lock (threadSafetyLock)
                        {
                            piece = ArmorPieces.FirstOrDefault(x => x.Name == armorPieceName);
                            if (piece == null)
                            {
                                int lastIndexOfAltered = armorPieceName.LastIndexOf("(Altered)");
                                if (lastIndexOfAltered > 0)
                                {
                                    string nonAlteredName = armorPieceName.Substring(0, lastIndexOfAltered).Trim();
                                    ArmorPiece nonAlteredPiece = ArmorPieces.FirstOrDefault(x => x.Name == nonAlteredName);
                                    if (nonAlteredPiece != null)
                                    {
                                        if (nonAlteredPiece.ArmorSetIds.Count > 1)
                                            throw new ScrapeParsingException(resourceName, $"Non Altered piece belongs to multiple sets???");

                                        int setId = nonAlteredPiece.ArmorSetIds.First();
                                        ArmorSet armorSet = ArmorSets.First(x => x.ArmorSetId == setId);

                                        piece = new ArmorPiece() { Name = armorPieceName };
                                        piece.ArmorSetIds.Add(setId);

                                        ArmorPieces.Add(piece);
                                        armorSet.ArmorPieces.Add(piece);
                                    }
                                    else
                                    {
                                        isStandAlonePiece = true;
                                        piece = AddStandalonePiece(armorPieceName);
                                    }
                                }
                                else
                                {
                                    isStandAlonePiece = true;
                                    piece = AddStandalonePiece(armorPieceName);
                                }
                            }
                        }
                        if (isStandAlonePiece) await _console.WriteLine($"Standalone armor piece found: {armorPieceName}");

                        if (piece.IsProcessed) throw new ScrapeParsingException(resourceName, $"Attempted to process Armor Piece {armorPieceName} multiple times.");
                        piece.IsProcessed = true;
                        piece.Type = armorPieceType;

                        if (node.Name == "a" && !string.IsNullOrEmpty(node.Attributes["href"]?.Value))
                        {
                            piece.ResourceName = node.Attributes["href"].Value;
                        }
                    }
                    else if (i <= 14)
                    {
                        bool p;
                        string text = td.InnerText.Replace("&nbsp;", " ").Trim();
                        if (string.IsNullOrEmpty(text))
                        {
                            if (!triedFetchFromPieceLink)
                            {
                                triedFetchFromPieceLink = true;

                                bool gotDataFromPiecePageInstead = await TryGetDataFromArmorPiecePage(piece);
                                if (!_useStaticHtmlFiles) Thread.Sleep(ScrapingPauseDuration/MaxSimultaniousRequests);

                                if (gotDataFromPiecePageInstead) break;
                                else
                                {
                                    await ScrapeExceptionContinue(new ScrapeParsingException(resourceName, $"{piece.Name}: blank value in table and failed to get from individual page."));

                                    p = true;
                                    value = 0;
                                }
                            }
                            else
                            {
                                p = true;
                                value = 0;
                            }
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
                        else if (i == 13) ParseTableCell(p, () => { piece.Poise = value; }, nameof(piece.Poise), piece.Name, resourceName);
                        else if (i == 14) ParseTableCell(p, () => { piece.Weight = value; }, nameof(piece.Weight), piece.Name, resourceName);
                        else break;
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

        private ArmorPiece AddStandalonePiece(string armorPieceName)
        {
            int newSetId;
            newSetId = ArmorSetIdCounter++;

            ArmorPiece piece = new ArmorPiece() { Name = armorPieceName };
            piece.ArmorSetIds.Add(newSetId);

            ArmorPieces.Add(piece);
            ArmorSets.Add(new ArmorSet() { ArmorSetId = newSetId, Name = armorPieceName, ArmorPieces = new List<ArmorPiece>() { piece } });

            return piece;
        }

        private async Task<bool> TryGetDataFromArmorPiecePage(ArmorPiece piece)
        {
            if (string.IsNullOrEmpty(piece.ResourceName)) return false;

            string html = null;
            try
            {
                html = await GetHtml(piece.ResourceName);
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "404"));
                Thread.Sleep(ScrapingPauseDuration/MaxSimultaniousRequests);
                return false;
            }
            catch
            {
                throw;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlNode tr = htmlDoc.QuerySelector("table.wiki_table > tbody > tr:nth-child(3)");
            if (tr == null)
            {
                await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "Induvidual Piece fetch, couldn't find table tr"));
                return false;
            }

            IList<HtmlNode> tds = tr.QuerySelectorAll("td");
            if (tds == null || tds.Count != 2)
            {
                await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "Induvidual Piece fetch, not two tds"));
                return false;
            }

            IList<HtmlNode> anchors = tds[0].QuerySelectorAll("a");
            foreach (HtmlNode anchor in anchors)
            {
                bool? parsed = null;

                parsed = await TryParseIndividualData(anchor, "Phy", piece, (p, v) => { p.Physical = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "VS Strike", piece, (p, v) => { p.PhysicalStrike = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "VS Slash", piece, (p, v) => { p.PhysicalSlash = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "VS Pierce", piece, (p, v) => { p.PhysicalPierce = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Phy", piece, (p, v) => { p.Physical = v; });
                if (parsed == true) continue; else if (parsed == false) return false;

                parsed = await TryParseIndividualData(anchor, "Magic", piece, (p, v) => { p.Magic = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Fire", piece, (p, v) => { p.Fire = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Ligt", piece, (p, v) => { p.Lightning = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Holy", piece, (p, v) => { p.Holy = v; });
                if (parsed == true) continue; else if (parsed == false) return false;

                if (parsed == null)
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse any data??"));
                    return false;
                }
            }

            anchors = tds[1].QuerySelectorAll("a");
            foreach (HtmlNode anchor in anchors)
            {
                bool? parsed = null;

                parsed = await TryParseIndividualData(anchor, "Immunity", piece, (p, v) => { p.Immunity = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Robustness", piece, (p, v) => { p.Robustness = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Focus", piece, (p, v) => { p.Focus = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Vitality", piece, (p, v) => { p.Vitality = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Poise", piece, (p, v) => { p.Poise = v; });
                if (parsed == true) continue; else if (parsed == false) return false;

                if (parsed == null)
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse any data??"));
                    return false;
                }
            }

            return true;
        }

        private async Task<bool?> TryParseIndividualData(HtmlNode anchor, string dataTypeText, ArmorPiece piece, Action<ArmorPiece, double> assignment)
        {
            if (anchor.InnerText == dataTypeText)
            {
                HtmlNode data;
                if (anchor.ParentNode.Name == "span") //Magic/Fire/Light/Holy
                {
                    data = anchor.ParentNode.NextSibling;
                }
                else
                {
                    data = anchor.NextSibling;
                }

                if (data == null || data.Name != "#text")
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "Couldn't find data."));
                    return false;
                }

                bool parsed;
                double value;
                parsed = double.TryParse(data.InnerText.Replace("\n", "").Replace("&nbsp;", " "), out value);
                if (parsed)
                {
                    assignment.Invoke(piece, value);
                    return true;
                }
                else
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse {dataTypeText}"));
                    return false;
                }
            }
            else
            {
                return null;
            }
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
                        foreach (int armorSetId in a.ArmorSetIds)
                        {
                            ArmorSet set = ArmorSets.Find(x => x.ArmorSetId == armorSetId);
                            await _console.WriteLine($"{set?.Name}: {a.Name}");
                        }
                    }
                }
                throw new ScrapeParsingException("Exiting.");
            }
        }

        private async Task EnsureCorrectness()
        {
            List<ArmorSet> armorSetsListCopy = ArmorSets.ToList();
            foreach (ArmorSet set in armorSetsListCopy.Where(x => x.ArmorPieces.Any(p => p.Type == null)))
            {
                List<ArmorPiece> setPiecesListCopy = set.ArmorPieces.ToList();
                foreach (ArmorPiece piece in setPiecesListCopy.Where(p => p.Type == null))
                {
                    await _console.WriteLine($"Did not process {set.Name}: {piece.Name}");
                    set.ArmorPieces.Remove(piece);
                    ArmorPieces.Remove(piece);
                }

                if (set.ArmorPieces.Count == 0)
                {
                    await _console.WriteLine($"Removing set due to no pieces: {set.Name}");
                    ArmorSets.Remove(set);
                }
            }

            await FlattenMultiples();

            foreach (ArmorSet set in ArmorSets.Where(x => x.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1).Any()))
            {
                foreach (IGrouping<ArmorPieceTypeEnum?, ArmorPiece> group in set.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1))
                {
                    foreach (ArmorPiece piece in group)
                    {
                        await _console.WriteLine($"Multiple pieces of the same type. {set.Name}, {group.Key}, {piece.Name}");
                    }
                }
                throw new ScrapeParsingException("Exiting.");
            }
        }

        private async Task FlattenMultiples()
        {
            List<ArmorSet> armorSetsCopy = ArmorSets.ToList();
            foreach (ArmorSet set in armorSetsCopy.Where(x => x.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1).Any()))
            {
                await PrintSet(set);

                List<ArmorPiece> NonAlteredHeads = set.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Head && !x.Name.EndsWith("(Altered)")).ToList();
                List<ArmorPiece> NonAlteredChests = set.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Chest && !x.Name.EndsWith("(Altered)")).ToList();
                List<ArmorPiece> NonAlteredGauntlets = set.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Gauntlets && !x.Name.EndsWith("(Altered)")).ToList();
                List<ArmorPiece> NonAlteredLegs = set.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Legs && !x.Name.EndsWith("(Altered)")).ToList();

                if (NonAlteredHeads.Count() > 1 && NonAlteredChests.Count() > 1 ||
                    NonAlteredHeads.Count() > 1 && NonAlteredGauntlets.Count() > 1 ||
                    NonAlteredHeads.Count() > 1 && NonAlteredLegs.Count() > 1 ||
                    NonAlteredChests.Count() > 1 && NonAlteredGauntlets.Count() > 1 ||
                    NonAlteredChests.Count() > 1 && NonAlteredLegs.Count() > 1 ||
                    NonAlteredGauntlets.Count() > 1 && NonAlteredLegs.Count() > 1)
                {
                    foreach (IGrouping<ArmorPieceTypeEnum?, ArmorPiece> group in set.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1))
                    {
                        foreach (ArmorPiece piece in group)
                        {
                            await _console.WriteLine($"Multiple pieces of the same type. {set.Name}, {group.Key}, {piece.Name}");
                        }
                    }
                    throw new ScrapeParsingException("Multiple groupings multiplicity. How handle?");
                }

                //if (set.ArmorPieces.Any(x => x.ArmorSetIds.Count > 1))
                //{
                //    foreach(ArmorPiece piece in set.ArmorPieces.Where(x => x.ArmorSetIds.Count > 1))
                //    {
                //        foreach (int armorSetId in piece.ArmorSetIds)
                //        {
                //            ArmorSet pieceSet = ArmorSets.First(x => x.ArmorSetId == armorSetId);
                //            foreach (ArmorPiece p in pieceSet.ArmorPieces)
                //            {
                //                await _console.WriteLine($"{pieceSet.Name}: {p.Name}");
                //            }
                //        }
                //    }

                //    throw new ScrapeParsingException("Groupings multiplicity. A piece belonged to multiple sets. How handle?");
                //}
               
                if (NonAlteredHeads.Count == 0) NonAlteredHeads.Add(null);
                if (NonAlteredChests.Count == 0) NonAlteredChests.Add(null);
                if (NonAlteredGauntlets.Count == 0) NonAlteredGauntlets.Add(null);
                if (NonAlteredLegs.Count == 0) NonAlteredLegs.Add(null);

                List<ArmorPiece> originalPiecesList = set.ArmorPieces.ToList();
                set.ArmorPieces.Clear();

                foreach (ArmorPiece piece in originalPiecesList)
                {
                    //relationships will be re-established
                    piece.ArmorSetIds.Remove(set.ArmorSetId);
                }

                bool isFirstCombo = true;
                foreach (ArmorPiece head in NonAlteredHeads)
                {
                    foreach (ArmorPiece chest in NonAlteredChests)
                    {
                        foreach (ArmorPiece gauntlets in NonAlteredGauntlets)
                        {
                            foreach (ArmorPiece legs in NonAlteredLegs)
                            {
                                var combo = new List<ArmorPiece>();

                                ArmorPiece alteredHead = null;
                                ArmorPiece alteredChest = null;
                                ArmorPiece alteredGauntlets = null;
                                ArmorPiece alteredLegs = null;

                                if (head != null)
                                {
                                    combo.Add(head);
                                    alteredHead = originalPiecesList.FirstOrDefault(x => x.Type == ArmorPieceTypeEnum.Head && x.Name == $"{head.Name} (Altered)");
                                }
                                if (chest != null)
                                {
                                    combo.Add(chest);
                                    alteredChest = originalPiecesList.FirstOrDefault(x => x.Type == ArmorPieceTypeEnum.Chest && x.Name == $"{chest.Name} (Altered)");
                                }
                                if (gauntlets != null)
                                {
                                    combo.Add(gauntlets);
                                    alteredGauntlets = originalPiecesList.FirstOrDefault(x => x.Type == ArmorPieceTypeEnum.Gauntlets && x.Name == $"{gauntlets.Name} (Altered)");
                                }
                                if (legs != null)
                                {
                                    combo.Add(legs);
                                    alteredLegs = originalPiecesList.FirstOrDefault(x => x.Type == ArmorPieceTypeEnum.Legs && x.Name == $"{legs.Name} (Altered)");
                                }

                                List<ArmorPiece> alteredCombo = null;
                                if (alteredHead != null || alteredChest != null || alteredGauntlets != null || alteredLegs != null)
                                {
                                    alteredCombo = new List<ArmorPiece>();

                                    if (alteredHead != null) alteredCombo.Add(alteredHead);
                                    else if (head != null) alteredCombo.Add(head);

                                    if (alteredChest != null) alteredCombo.Add(alteredChest);
                                    else if (chest != null) alteredCombo.Add(chest);

                                    if (alteredGauntlets != null) alteredCombo.Add(alteredGauntlets);
                                    else if (gauntlets != null) alteredCombo.Add(gauntlets);

                                    if (alteredLegs != null) alteredCombo.Add(alteredLegs);
                                    else if (legs != null) alteredCombo.Add(legs);
                                }

                                if (isFirstCombo)
                                {
                                    isFirstCombo = false;
                                    set.ArmorPieces = combo;
                                    foreach (ArmorPiece p in combo) p.ArmorSetIds.Add(set.ArmorSetId);
                                    await PrintSet(set);
                                }
                                else
                                {
                                    await CreateNewSetFrom(set, combo, false);
                                }

                                if (alteredCombo != null)
                                {
                                    await CreateNewSetFrom(set, alteredCombo, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task CreateNewSetFrom(ArmorSet set, List<ArmorPiece> pieces, bool isAltered)
        {
            var newSet = new ArmorSet() { ArmorSetId = ArmorSetIdCounter++ };
            newSet.Name = isAltered ? $"{set.Name} (Altered)" : set.Name;
            newSet.ArmorPieces = pieces;
            foreach (ArmorPiece p in pieces)
            {
                p.ArmorSetIds.Add(newSet.ArmorSetId);
            }
            await PrintSet(newSet);
        }

        private async Task PrintSet(ArmorSet set)
        {
            bool debug = false;
            if (debug)
            {
                await _console.WriteLine(new string('-', 30));
                foreach (ArmorPiece p in set.ArmorPieces)
                {
                    await _console.WriteLine($"{set.Name}: {p.Name}");
                }
            }
        }

        private async Task ScrapeExceptionContinue(ScrapeParsingException acceptableException)
        {
            await _console.WriteLine($"WARNING: {acceptableException.Message}");
            ParsingContinueErrors.Add(acceptableException.Message);
        }
    }

    class ScrapeParsingException : Exception
    {
        public ScrapeParsingException(string message) : base(message) { }

        public ScrapeParsingException(string resourceName, string message) : base($"{resourceName}: {message}") { }
    }
}
