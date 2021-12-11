using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Dto
{
    public class ArmorPieceDto
    {
        public int ArmorPieceId { get; set; }
        public int ArmorSetId { get; set; }
        public string Name { get; set; }
        public ArmorPieceTypeEnum? Type { get; set; }

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

        public ArmorPieceDto() { }

        public ArmorPieceDto(ArmorPiece p, int index)
        {
            ArmorPieceId = index;

            ArmorSetId = p.ArmorSetId;
            Name = p.Name;
            Type = p.Type;

            Physical = p.Physical;
            PhysicalStrike = p.PhysicalStrike;
            PhysicalSlash = p.PhysicalSlash;
            PhysicalPierce = p.PhysicalPierce;

            Magic = p.Magic;
            Fire = p.Fire;
            Lightning = p.Lightning;
            Holy = p.Holy;

            Immunity = p.Immunity;
            Robustness = p.Robustness;
            Focus = p.Focus;
            Death = p.Death;

            Weight = p.Weight;
        }
    }
}
