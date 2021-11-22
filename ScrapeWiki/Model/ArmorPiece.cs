using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeWiki.Model
{
    public class ArmorPiece
    {
        public int ArmorPieceId { get; set; }
        public int ArmorSetId { get; set; }
        public string Name { get; set; }

        public double Physical { get; set; }
        public double PhysicalStrike { get; set; }
        public double PhysicalSlash { get; set; }
        public double PhysicalPierce { get; set; }

        public double Magic { get; set; }
        public double Fire { get; set; }
        public double Lightning { get; set; }
        public double Holy { get; set; }

        public double Immunity { get; set; }
        public double Robustness { get; set; }
        public double Focus { get; set; }
        public double Death { get; set; }

        public double Weigtht { get; set; }
    }
}
