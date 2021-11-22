using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Dto
{
    public class ArmorPieceDto
    {
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

        public double Weight { get; set; }

        public static explicit operator ArmorPieceDto(ArmorPiece a)
        {
            return new ArmorPieceDto()
            {
                ArmorSetId = a.ArmorSetId,
                Name = a.Name,

                Physical = a.Physical,
                PhysicalStrike = a.PhysicalStrike,
                PhysicalSlash = a.PhysicalSlash,
                PhysicalPierce = a.PhysicalPierce,

                Magic = a.Magic,
                Fire = a.Fire,
                Lightning = a.Lightning,
                Holy = a.Holy,

                Immunity = a.Immunity,
                Robustness = a.Robustness,
                Focus = a.Focus,
                Death = a.Death,

                Weight = a.Weight,
            };
        }
    }
}
