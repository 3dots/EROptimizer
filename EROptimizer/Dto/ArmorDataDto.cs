using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Dto
{
    public class ArmorDataDto
    {
        public List<ArmorPieceDto> Head { get; set; } = new List<ArmorPieceDto>();
        public List<ArmorPieceDto> Chest { get; set; } = new List<ArmorPieceDto>();
        public List<ArmorPieceDto> Gauntlets { get; set; } = new List<ArmorPieceDto>();
        public List<ArmorPieceDto> Legs { get; set; } = new List<ArmorPieceDto>();

        public List<ArmorSetDto> ArmorSets { get; set; } = new List<ArmorSetDto>();
    }
}
