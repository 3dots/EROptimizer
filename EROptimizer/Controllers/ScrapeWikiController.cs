using EROptimizer.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EROptimizer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScrapeWikiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ScrapeWikiController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //[HttpGet]
        //public async Task<IEnumerable<ArmorChangedDto>> ScrapeWiki()
        //{


        //    return new List<ArmorChangedDto>() { new ArmorChangedDto() { ArmorSetName = "Test" } };
        //}

        //[HttpGet("/ws")]
        //public async Task ScrapeWiki()
        //{
        //    HttpContext httpContext = _httpContextAccessor.HttpContext;

        //    if (httpContext.WebSockets.IsWebSocketRequest)
        //    {
        //        using WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
        //        await Echo(httpContext, webSocket);
        //    }
        //    else
        //    {
        //        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }
        //}

        //private async Task Echo(HttpContext context, WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    while (!result.CloseStatus.HasValue)
        //    {
        //        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

        //        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    }
        //    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        //}
    }
}
