using ScrapeWiki.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EROptimizer.Dto
{
    public class ArmorSetDto
    {
        public int ArmorSetId { get; set; }
        public string Name { get; set; }

        //Never populated/stored in static .json file.
        public List<ArmorPieceDto> ArmorPieces { get; set; } = new List<ArmorPieceDto>();

        public static explicit operator ArmorSetDto(ArmorSet a)
        {
            return new ArmorSetDto()
            {
                ArmorSetId = a.ArmorSetId,
                Name = a.Name,
            };
        }
    }
}
