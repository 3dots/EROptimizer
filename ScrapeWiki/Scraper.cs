using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using OpenQA.Selenium.Chrome;
using ScrapeWiki.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    public class Scraper
    {
        #region Fields

        const string BASE_URL = "https://eldenring.wiki.fextralife.com";

        const int SCRAPING_PAUSE_AVG_MS = 15000;
        const int SCRAPING_PAUSE_STD_DEV_MS = 5000;

        readonly Random _random = new Random();

        readonly IProgressConsole _console;
        readonly string _filesPath;
        readonly string _chromeDriverPath;

        readonly bool _dontDownload;
        readonly bool _dontDownloadRelationships;

        #endregion

        #region Properties

        int ArmorSetIdCounter { get; set; } = 1;
        public List<ArmorSet> ArmorSets { get; private set; } = new List<ArmorSet>();
        public List<ArmorPiece> ArmorPieces { get; private set; } = new List<ArmorPiece>();

        public List<double> EquipLoadArray { get; private set; } = new List<double>();
        public List<Talisman> Talismans { get; private set; } = new List<Talisman>();

        bool DontDownload { get; set; }

        #endregion

        #region Constructor

        public Scraper(IProgressConsole pc, string filesPath, string chromeDriverPath, bool dontDownload, bool dontDownloadRelationships)
        {
            _console = pc ?? new DummyProgressConsole();
            _filesPath = filesPath;
            _chromeDriverPath = chromeDriverPath;
            _dontDownload = dontDownload;
            _dontDownloadRelationships = dontDownloadRelationships;
        }

        #endregion

        #region Scrape main

        public async Task<bool> Scrape()
        {
            bool success = false;

            try
            {
                await BeginScrape();
                success = true;
            }
            catch (Exception e)
            {
                await _console.WriteLine("Scape failed. Exception:");
                await _console.WriteLine(e.ToString());
            }

            await CloseChromeDriver();

            return success;
        }

        private async Task BeginScrape()
        {
            await _console.WriteLine($"Begin Scrape.");

            DontDownload = _dontDownload || _dontDownloadRelationships;

            //Getting armor set names
            await GetSets();

            List<ArmorSet> setsCopy = ArmorSets.ToList(); //copy because ProcessSet() removes 404s
            foreach (ArmorSet s in setsCopy)
            {
                await ProcessSet(s);
            }

            await EnsureUniqueness();

            DontDownload = _dontDownload;

            await GetData(ArmorPieceTypeEnum.Head);
            await GetData(ArmorPieceTypeEnum.Chest);
            await GetData(ArmorPieceTypeEnum.Gauntlets);
            await GetData(ArmorPieceTypeEnum.Legs);

            await EnsureUniqueness();

            await EnsureCorrectness();

            await EnsureUniqueness();

            ArmorSets = ArmorSets.OrderBy(x => x.Name).ToList();
            ArmorPieces = ArmorPieces.OrderBy(x => x.Name).ToList();

            bool debug = false;
            if (debug)
            {
                foreach (ArmorSet set in ArmorSets)
                {
                    await PrintSet(true, set);
                }
            }

            await GetEquipLoadArray();
            await ScrapeTalismans();

            PopulateArmorEnduranceBonus();

            await _console.WriteLine("Successfully scraped data.");
        }

        #endregion

        #region Selenium

        private ChromeDriver _chromeDriver;
        private ChromeDriver ChromeDriver
        {
            get
            {
                if (_chromeDriver == null)
                {
                    var options = new ChromeOptions();
                    options.PageLoadStrategy = OpenQA.Selenium.PageLoadStrategy.Eager;
                    _chromeDriver = new ChromeDriver(_chromeDriverPath, options);
                }
                return _chromeDriver;
            }
        }

        private async Task CloseChromeDriver()
        {
            if (_chromeDriver == null) return;
            try
            {
                _chromeDriver.Close();
            }
            catch (Exception e)
            {
                await _console.WriteLine("Closing chrome driver exception:");
                await _console.WriteLine(e.ToString());
            }
        }

        private async Task<string> GetHtml(string resourceName) //Expects slash "/Armor"
        {
            string resourceFile = $"{resourceName.Replace("/", "")}.html";
            string filePath = Path.Combine(_filesPath, resourceFile);

            string htmlString = null;
            if (DontDownload || File.Exists(filePath) && File.GetLastWriteTime(filePath).Date == DateTime.Now.Date)
            {
                htmlString = await File.ReadAllTextAsync(filePath);
                await _console.WriteLine($"Using local copy of {resourceName}");
            }

            if (htmlString == "") throw new HttpRequestException("Empty file", null, HttpStatusCode.NotFound);

            if (htmlString == null)
            {
                string url = $"{BASE_URL}{resourceName}";
                await _console.WriteLine($"Scraping {url}");
                Stopwatch stopwatchOverall = Stopwatch.StartNew();

                Stopwatch stopwatch = Stopwatch.StartNew();

                ChromeDriver.Navigate().GoToUrl(url);
                htmlString = _chromeDriver.PageSource;

                //todo: detect 404

                await File.WriteAllTextAsync(filePath, htmlString);

                stopwatch.Stop();

                double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - _random.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal =
                             SCRAPING_PAUSE_AVG_MS + SCRAPING_PAUSE_STD_DEV_MS * randStdNormal; //random normal(mean,stdDev^2)

                double timeToSleep = randNormal - stopwatch.ElapsedMilliseconds;

                if (timeToSleep > 0) Thread.Sleep((int)timeToSleep);

                stopwatchOverall.Stop();

                await _console.WriteLine($"Retrieved {url} in {stopwatchOverall.Elapsed.TotalSeconds:n2}s");
            }

            return htmlString;
        }

        #endregion

        #region Scrape helpers

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
                else if (setName == "Millicent's Set") //Unobtainable
                {
                    continue;
                }

                if (string.IsNullOrEmpty(setResourceName)) throw new ScrapeParsingException(resourceName, "Empty Set link.");

                //await _console.WriteLine($"{setResourceName} {setName}");

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
                ArmorSets.Remove(set);
                return;
            }
            catch
            {
                throw;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var headingStrings = new List<string> {
                $"{set.Name} Armor Pieces in Elden Ring",
                $"{set.Name}Armor Pieces in Elden Ring",
                $"{set.Name} Pieces in Elden Ring",
                $"{set.Name} Armor Pieces"
            };

            if (set.Name == "Haligtree Soldier Set") headingStrings.Add("Haligtree Set Armor Pieces in Elden Ring");

            string headerText = $"{set.Name} Armor Pieces in Elden Ring";
            IList<HtmlNode> h3s = htmlDoc.QuerySelectorAll("#wiki-content-block > h3");
            HtmlNode h3ArmorPieces = h3s.FirstOrDefault(x => headingStrings.Contains(x.InnerText.Replace("&nbsp;", " ")?.Trim(),
                StringComparer.InvariantCultureIgnoreCase));
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

                if (new string[] { "Map Link", "runes", "Elden Ring Map here" }.Contains(name))
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(set.ResourceName, $"Ignoring {name} Link"));
                    continue;
                }

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
                        else if (IsUnobtainable(armorPieceName))
                        {
                            break;
                        }

                        bool isStandAlonePiece = false;

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
                        if (string.IsNullOrEmpty(text) || text == "-")
                        {
                            if (!triedFetchFromPieceLink)
                            {
                                triedFetchFromPieceLink = true;

                                bool gotDataFromPiecePageInstead = await TryGetDataFromArmorPiecePage(piece);
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
                await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "Individual Piece fetch, couldn't find table tr"));
                return false;
            }

            IList<HtmlNode> tds = tr.QuerySelectorAll("td");
            if (tds == null || tds.Count != 2)
            {
                await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "Individual Piece fetch, not two tds"));
                return false;
            }

            IList<HtmlNode> anchors = tds[0].QuerySelectorAll("a");
            foreach (HtmlNode anchor in anchors)
            {
                bool? parsed = null;

                parsed = await TryParseIndividualData(anchor, "Phy", piece, (p, v) => { p.Physical = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "Physical", piece, (p, v) => { p.Physical = v; });
                if (parsed == true) continue; else if (parsed == false) return false;

                parsed = await TryParseIndividualData(anchor, "VS Strike", piece, (p, v) => { p.PhysicalStrike = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "VS Slash", piece, (p, v) => { p.PhysicalSlash = v; });
                if (parsed == true) continue; else if (parsed == false) return false;
                parsed = await TryParseIndividualData(anchor, "VS Pierce", piece, (p, v) => { p.PhysicalPierce = v; });
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
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse any data?? {anchor.InnerText}"));
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
                parsed = await TryParseIndividualData(anchor, "Death", piece, (p, v) => { p.Vitality = v; });
                if (parsed == true) continue; else if (parsed == false) return false;

                parsed = await TryParseIndividualData(anchor, "Poise", piece, (p, v) => { p.Poise = v; });
                if (parsed == true) continue; else if (parsed == false) return false;

                if (parsed == null)
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse any data?? {anchor.InnerText}"));
                    return false;
                }
            }

            HtmlNode node = htmlDoc.QuerySelector("table.wiki_table > tbody > tr:nth-child(4) > td:nth-child(2) > span");
            if (node == null)
            {
                node = htmlDoc.QuerySelector("table.wiki_table > tbody > tr:nth-child(4) > td:nth-child(2) > a");
                if (node != null) node = node.NextSibling;

                if (node == null || node.Name != "#text")
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, "Individual Piece fetch, couldn't find table weight html."));
                    return false;
                }
            }

            double value;
            if (double.TryParse(node.InnerText.Replace("\n", "").Replace("&nbsp;", " ").Replace(",", "."), out value))
            {
                piece.Weight = value;
            }
            else
            {
                await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse weight"));
                return false;
            }

            return true;
        }

        private async Task<bool?> TryParseIndividualData(HtmlNode anchor, string dataTypeText, ArmorPiece piece, Action<ArmorPiece, double> assignment)
        {
            if (anchor.InnerText.Replace("&nbsp;", " ").Trim() == dataTypeText)
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
                string text = data.InnerText.Replace("\n", "").Replace("&nbsp;", " ").Replace(",", ".");
                parsed = double.TryParse(text, out value);
                if (parsed)
                {
                    assignment.Invoke(piece, value);
                    return true;
                }
                else
                {
                    await ScrapeExceptionContinue(new ScrapeParsingException(piece.ResourceName, $"Failed to parse {dataTypeText}, text: {text}"));
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        private async Task GetEquipLoadArray()
        {
            //await ScrapeEquipLoadArray();
            var path = Path.Combine(AppContext.BaseDirectory, "equip.load.csv");
            string[] lines = await File.ReadAllLinesAsync(path);

            for (int i = 1; i < lines.Length; i++)
            {
                string hexFloat = lines[i].Split(',')[1];
                uint num = uint.Parse(hexFloat, System.Globalization.NumberStyles.AllowHexSpecifier);
                byte[] floatVals = BitConverter.GetBytes(num);
                float f = BitConverter.ToSingle(floatVals, 0);

                double value = (double)f;
                EquipLoadArray.Add(value);
            }
        }

        private async Task ScrapeEquipLoadArray()
        {
            string resourceName = "/Endurance";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await GetHtml(resourceName));

            IList<HtmlNode> tables = htmlDoc.QuerySelectorAll("table.wiki_table");
            if (tables.Count < 3) throw new ScrapeParsingException(resourceName, $"Expected at least 3 tables.");

            HtmlNode table = tables[2];

            HtmlNode eqipLoadTd = table.QuerySelector("thead > tr > td:nth-child(2)");
            if (eqipLoadTd == null || eqipLoadTd.InnerText != "Equip Load")
                throw new ScrapeParsingException(resourceName, $"Expected 'Equip Load' in 3rd table.");

            IList<HtmlNode> trs = table.QuerySelectorAll("tbody > tr");
            int i = 1;
            foreach (HtmlNode tr in trs)
            {
                List<HtmlNode> tds = tr.GetChildElements().ToList();
                if (tds[0].InnerText != (i++).ToString()) throw new ScrapeParsingException(resourceName, $"{i}'th row isn't indexed properly");
                EquipLoadArray.Add(double.Parse(tds[1].InnerText));
            }
        }

        private async Task ScrapeTalismans()
        {
            string resourceName = "/Talismans";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(await GetHtml(resourceName));

            HtmlNode table = htmlDoc.QuerySelector("table.wiki_table");
            if (table == null) throw new ScrapeParsingException(resourceName, $"Didn't find table");

            IList<HtmlNode> trs = table.QuerySelectorAll("tbody > tr");
            foreach (HtmlNode tr in trs)
            {
                List<HtmlNode> tds = tr.GetChildElements().ToList();

                Talisman t = new Talisman();
                t.Name = tds[0].QuerySelector("a").InnerText.Replace("&nbsp;", " ").Trim();
                t.Weight = double.Parse(tds[2].InnerText.Replace("&nbsp;", " ").Trim());

                Talismans.Add(t);
            }

            Talismans = Talisman.Generate(Talismans);
        }

        private void PopulateArmorEnduranceBonus()
        {
            ArmorPiece headWithBonus = ArmorPieces.First(x => x.Name == "Imp Head (Wolf)");
            headWithBonus.EnduranceBonus = 2;

            headWithBonus = ArmorPieces.First(x => x.Name == "Hierodas Glintstone Crown");
            headWithBonus.EnduranceBonus = 2;
        }

        #endregion

        #region Data Integrity

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

            if (ArmorPieces.Where(x => x.Weight == 0).Count() > 0)
            {
                foreach (ArmorPiece piece in ArmorPieces.Where(x => x.Weight == 0))
                {
                    await _console.WriteLine($"Piece with 0 weight: {piece.Name}");
                }
                throw new ScrapeParsingException("Exiting.");
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
            bool debug = false;

            List<ArmorSet> armorSetsCopy = ArmorSets.ToList();
            foreach (ArmorSet set in armorSetsCopy.Where(x => x.ArmorPieces.GroupBy(p => p.Type).Where(g => g.Count() > 1).Any()))
            {
                await PrintSet(debug, set);

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

                    await ScrapeExceptionContinue(new ScrapeParsingException("Multiple groupings multiplicity. How handle?"));
                    //throw ;
                }

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

                int i = 1;
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

                                if (i == 1)
                                {
                                    set.ArmorPieces = combo;
                                    foreach (ArmorPiece p in combo) p.ArmorSetIds.Add(set.ArmorSetId);
                                    await PrintSet(debug, set);
                                }
                                else
                                {
                                    await CreateNewSetFrom(set, combo, false, i, debug);
                                }

                                if (alteredCombo != null)
                                {
                                    await CreateNewSetFrom(set, alteredCombo, true, i, debug);
                                }

                                i++;
                            }
                        }
                    }
                }
            }
        }

        private async Task CreateNewSetFrom(ArmorSet set, List<ArmorPiece> pieces, bool isAltered, int versionIndex, bool debug)
        {
            var newSet = new ArmorSet() { ArmorSetId = ArmorSetIdCounter++ };

            if (versionIndex > 1)
                newSet.Name = isAltered ? $"{set.Name} v{versionIndex} (Altered)" : $"{set.Name} v{versionIndex}";
            else
                newSet.Name = isAltered ? $"{set.Name} (Altered)" : set.Name;

            newSet.ArmorPieces = pieces;
            ArmorSets.Add(newSet);
            foreach (ArmorPiece p in pieces)
            {
                p.ArmorSetIds.Add(newSet.ArmorSetId);
            }
            await PrintSet(debug, newSet);
        }

        private async Task PrintSet(bool debug, ArmorSet set)
        {
            if (debug)
            {
                await _console.WriteLine(new string('-', 30));
                foreach (ArmorPiece p in set.ArmorPieces)
                {
                    await _console.WriteLine($"{set.Name}: {p.Name}");
                }
            }
        }

        private bool IsUnobtainable(string armorPieceName)
        {
            return new string[]
            {
                "Brave's Battlewear",
                "Brave's Battlewear (Altered)",
                "Brave's Bracer",
                "Brave's Leather Helm",
                "Brave's Legwraps",
                "Deathbed Smalls",
                "Grass Hair Ornament",
                "Golden Prosthetic",
                "Millicent's Boots",
                "Millicent's Gloves",
                "Millicent's Robe",
                "Millicent's Tunic",
                "Ragged Armor",
                "Ragged Armor (Altered)",
                "Ragged Gloves",
                "Ragged Hat",
                "Ragged Hat (Altered)",
                "Ragged Loincloth",
            }.Contains(armorPieceName);
        }

        #endregion

        #region Exceptions
        private async Task ScrapeExceptionContinue(ScrapeParsingException acceptableException)
        {
            await _console.WriteLine($"WARNING: {acceptableException.Message}");
        }

        class ScrapeParsingException : Exception
        {
            public ScrapeParsingException(string message) : base(message) { }

            public ScrapeParsingException(string resourceName, string message) : base($"{resourceName}: {message}") { }
        }

        #endregion
    }
}
