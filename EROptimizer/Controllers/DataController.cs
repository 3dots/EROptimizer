using EROptimizer.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EROptimizer.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DataController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ArmorDataDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            //return NotFound(); //testing error dialogs

            string path = _configuration["StaticDataPath"];
            string fileName = "ArmorData.json";

            try
            {
                using FileStream fileStream = System.IO.File.Open(Path.Combine(path, fileName), FileMode.Open, FileAccess.Read, FileShare.Delete);
                ArmorDataDto data = await JsonSerializer.DeserializeAsync<ArmorDataDto>(fileStream);
                await fileStream.DisposeAsync();

                return Ok(data);
            }
            catch (FileNotFoundException)
            {
                return NotFound(); //future, try to get from DB
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
