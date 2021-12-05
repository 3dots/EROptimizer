using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Dto
{
    public class ArmorPieceChangesDto
    {
        public string SetName { get; set; }
        public string PieceName { get; set; }
        public List<string> Changes { get; set; } = new List<string>();
    }
}
