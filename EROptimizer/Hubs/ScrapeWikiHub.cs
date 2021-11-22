using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Hubs
{
    public class ScrapeWikiHub : Hub
    {
        public async Task StartScrape()
        {
            await Clients.All.SendAsync("ReceiveMessage", "Success!");
        }
    }
}
