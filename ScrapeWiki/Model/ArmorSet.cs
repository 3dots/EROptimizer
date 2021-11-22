using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeWiki.Model
{
    public class ArmorSet
    {
        public int ArmorSetId { get; set; }
        public string Name { get; set; }
        public string ResourceName { get; set; }

        public List<ArmorPiece> ArmorPieces { get; set; } = new List<ArmorPiece>();
    }
}
