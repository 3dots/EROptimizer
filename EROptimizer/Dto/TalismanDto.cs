using ScrapeWiki.Model;
using System.Collections.Generic;
using System.Linq;

namespace EROptimizer.Dto
{
    public class TalismanDto
    {
        public string Name { get; set; }
        public double Weight { get; set; }

        public TalismanDto() { }

        public TalismanDto(Talisman t)
        {
            Name = t.Name;
            Weight = t.Weight;
        }        
    }
}
