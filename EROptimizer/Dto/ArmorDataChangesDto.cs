using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Dto
{
    public class ArmorDataChangesDto
    {
        public List<string> Messages { get; set; } = new List<string>();
        public List<ArmorPieceChangesDto> ArmorPieceChanges { get; set; } = new List<ArmorPieceChangesDto>();
    }
}
