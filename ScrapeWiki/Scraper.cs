using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    public class Scraper
    {
        private readonly IProgressConsole PConsole;

        public Scraper(IProgressConsole pc)
        {
            PConsole = pc;
        }

        private readonly string ArmorUrl = "https://eldenring.wiki.fextralife.com/Armor";

        public async Task Scrape()
        {            
            try
            {
                await BeginScrape();
            }
            catch (Exception e)
            {
                PConsole.WriteLine("Scape failed. Exception:");
                PConsole.WriteLine(e.ToString());
            }
        }

        private async Task BeginScrape()
        {
            PConsole.WriteLine($"Scraping {ArmorUrl}");
            Thread.Sleep(1000);
            throw new Exception("Death");
        }
    }
}
