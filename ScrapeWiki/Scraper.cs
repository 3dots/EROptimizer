using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
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
        private readonly string _baseUrl = "https://eldenring.wiki.fextralife.com";

        private readonly IProgressConsole _console;
        private readonly string _filesPath;
        private readonly bool _createHtmlFilesInSource;
        private readonly bool _useStaticHtmlFiles;

        public Scraper(IProgressConsole pc)
        {

        }

        internal Scraper(IProgressConsole pc, string filesPath, bool createHtmlFilesInSource, bool useStaticFiles)
        {
            _console = pc;
            _createHtmlFilesInSource = createHtmlFilesInSource;
            _useStaticHtmlFiles = useStaticFiles;
            _filesPath = filesPath;
        }

        internal Scraper(IProgressConsole pc, string filesPath, string createHtmlFilesInSource, string useStaticFiles) 
            : this(pc, filesPath, bool.Parse(createHtmlFilesInSource), bool.Parse(useStaticFiles))
        {
            
        }

        public async Task Scrape()
        {            
            //try
            //{
                await BeginScrape();
            //}
            //catch (Exception e)
            //{
            //    PConsole.WriteLine("Scape failed. Exception:");
            //    PConsole.WriteLine(e.ToString());
            //}
        }

        private async Task BeginScrape()
        {
            string url = $"{_baseUrl}/Armor";
            _console.WriteLine($"Scraping {url}");
            if (_createHtmlFilesInSource) _console.WriteLine("Creating HTML files in source.");
            if (_useStaticHtmlFiles) _console.WriteLine($"Using static HTML files at {_filesPath}");

            string section = "Armor.html";

            string armorHtml;
            if (_useStaticHtmlFiles)
            {
                armorHtml = await File.ReadAllTextAsync(Path.Combine(_filesPath, section));
            }
            else
            {
                var c = new HttpClient();
                armorHtml = await c.GetStringAsync(url);

                if (_createHtmlFilesInSource) await File.WriteAllTextAsync(Path.Combine(_filesPath, section), armorHtml);
            }

            _console.WriteLine($"Retrieved {url}");

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(armorHtml);

            IList<HtmlNode> h2s = htmlDoc.QuerySelectorAll("#wiki-content-block > h2");
            HtmlNode h2ArmorSetGallery = h2s.FirstOrDefault(x => x.InnerText == "Elden Ring Armor Set Gallery");
            if (h2ArmorSetGallery == null) throw new ScrapeParsingException(section, "h2 with \"Elden Ring Armor Set Gallery\" not found.");

            IList<HtmlNode> armorSetLinks = h2ArmorSetGallery.NextSiblingElement().GetChildElements().First().QuerySelectorAll("a");
            foreach(HtmlNode a in armorSetLinks)
            {
                _console.WriteLine(a.Attributes["href"]?.Value);
                _console.WriteLine(a.InnerText.Replace("&nbsp;", ""));
            }
        }
    }

    class ScrapeParsingException : Exception 
    {
        public ScrapeParsingException(string section, string message) : base($"{section}: {message}")
        {
            
        }
    }

}
