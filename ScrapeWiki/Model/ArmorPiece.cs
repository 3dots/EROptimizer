using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeWiki.Model
{
    public enum ArmorPieceTypeEnum
    {
        Head = 0,
        Chest = 1,
        Gauntlets = 2,
        Legs = 3,
    }

    public class ArmorPiece
    {
        public List<int> ArmorSetIds { get; set; } = new List<int>();
        public string Name { get; set; }
        public string ResourceName { get; set; }
        public ArmorPieceTypeEnum? Type { get; set; }

        public bool IsProcessed { get; set; }

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
        public double Vitality { get; set; }

        public double Poise { get; set; }

        public double Weight { get; set; }
    }
}
