using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var scraper = new Scraper(new ProgressConsole(),
                                        ConfigurationManager.AppSettings.Get("FilesPath"),
                                        ConfigurationManager.AppSettings.Get("ChromeDriverFolderPath"));

            Stopwatch stopwatch = Stopwatch.StartNew();
            await scraper.Scrape();
            stopwatch.Stop();

            Console.WriteLine($"Success, press any key to exit. {stopwatch.Elapsed.TotalSeconds:n2}s");
            Console.ReadKey();
        }
    }
}
