using EROptimizerOld.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizerOld.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScrapeWikiController
    {
        [HttpGet]
        public async Task<IEnumerable<ArmorChangedDto>> ScrapeWiki()
        {


            return new List<ArmorChangedDto>() { new ArmorChangedDto() { ArmorSet = "Test" } };
        }
    }
}
