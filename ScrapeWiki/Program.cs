using System;
using System.Configuration;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var scraper = new Scraper(new ProgressConsole(), 
                                        ConfigurationManager.AppSettings.Get("FilesPath"),
                                        ConfigurationManager.AppSettings.Get("CreateHtmlFiles"),
                                        ConfigurationManager.AppSettings.Get("UseStaticHtmlFiles"));

            await scraper.Scrape();

            Console.WriteLine("Success, press any key to exit.");
            Console.ReadKey();
        }
    }
}
