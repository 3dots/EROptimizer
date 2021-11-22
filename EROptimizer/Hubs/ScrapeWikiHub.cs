using EROptimizer.Dto;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using ScrapeWiki;
using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EROptimizer.Hubs
{
    public class ScrapeWikiHub : Hub, IProgressConsole
    {
        private readonly IConfiguration _configuration;
        public ScrapeWikiHub(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task StartScrape()
        {
            Scraper scraper;
            if (_configuration["ScrapeWiki:UseStaticHtmlFiles"] == true.ToString())
                scraper = new Scraper(this, _configuration["ScrapeWiki:FilesPath"], false, true);
            else
                scraper = new Scraper(this);

            await scraper.Scrape();
            await WriteStaticDataFile(scraper);            
            await ScrapeEnd();
        }

        public async Task WriteLine(string s)
        {
            await Clients.All.SendAsync("WriteLine", s);
        }

        async Task ScrapeEnd()
        {
            await Clients.All.SendAsync("ScrapeEnd");
        }

        async Task WriteStaticDataFile(Scraper s)
        {
            string path = _configuration["StaticDataPath"];
            string fileName = "ArmorData.json";

            await WriteLine($"Creating {fileName} at {path}");

            try
            {
                var data = new ArmorDataDto();

                data.Head.Add(new ArmorPieceDto() { ArmorSetId = 0, Name = "None" });
                data.Head.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Head).Select(x => (ArmorPieceDto)x));

                data.Chest.Add(new ArmorPieceDto() { ArmorSetId = 0, Name = "None" });
                data.Chest.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Chest).Select(x => (ArmorPieceDto)x));
                
                data.Gauntlets.Add(new ArmorPieceDto() { ArmorSetId = 0, Name = "None" });
                data.Gauntlets.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Gauntlets).Select(x => (ArmorPieceDto)x));

                data.Legs.Add(new ArmorPieceDto() { ArmorSetId = 0, Name = "None" });
                data.Legs.AddRange(s.ArmorPieces.Where(x => x.Type == ArmorPieceTypeEnum.Legs).Select(x => (ArmorPieceDto)x));

                await File.WriteAllTextAsync(Path.Combine(path, fileName), JsonSerializer.Serialize(data));
                await WriteLine($"Success.");
            }
            catch (Exception e)
            {
                await WriteLine($"Fail. Exception:");
                await WriteLine(e.ToString());
            }            
        }
    }
}
