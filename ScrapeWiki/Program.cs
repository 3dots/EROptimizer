using System;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var scraper = new Scraper(new ProgressConsole());

            await scraper.Scrape();

            Console.WriteLine("Success, press any key to exit.");
            Console.ReadKey();
        }
    }
}
