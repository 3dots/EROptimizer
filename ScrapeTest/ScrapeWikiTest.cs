using Microsoft.Extensions.Configuration;
using ScrapeWiki;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ScrapeTest
{
    public class ScrapeWikiTest
    {
        private readonly ITestOutputHelper output;

        Scraper _scraper;
        bool _scraperResult;

        public ScrapeWikiTest(ITestOutputHelper output)
        {
            this.output = output;

            IConfiguration config = AppSettings.InitConfiguration();
            _scraper = new Scraper(new TestProgressConsole(this.output), config["FilesPath"], null, true, true);

            _scraperResult = Task.Run(async () => await _scraper.Scrape()).Result;
        }

        [Fact]
        public void TestSuccess()
        {
            Assert.True(_scraperResult);
        }

        [Fact]
        public void Counts()
        {
            Assert.Equal(560, _scraper.ArmorPieces.Count);

            Assert.Equal(239, _scraper.ArmorSets.Count);
        }
    }

    class TestProgressConsole : IProgressConsole
    {
        private readonly ITestOutputHelper output;

        public TestProgressConsole(ITestOutputHelper output)
        {
            this.output = output;
        }

        Task IProgressConsole.WriteLine(string s)
        {
            return Task.Run(() => { output.WriteLine(s); });
        }
    }
}