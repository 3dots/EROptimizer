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
            bool dontDownload = false;
            bool.TryParse(ConfigurationManager.AppSettings.Get("DontDownload"), out dontDownload);

            var scraper = new Scraper(new ProgressConsole(),
                                        ConfigurationManager.AppSettings.Get("FilesPath"),
                                        ConfigurationManager.AppSettings.Get("ChromeDriverFolderPath"),
                                        dontDownload);

            Stopwatch stopwatch = Stopwatch.StartNew();
            await scraper.Scrape();
            stopwatch.Stop();

            Console.WriteLine($"Success, press any key to exit. {stopwatch.Elapsed.TotalMinutes:n2} minutes");
            Console.ReadKey();
        }
    }
}
