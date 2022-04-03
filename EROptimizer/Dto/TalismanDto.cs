using ScrapeWiki.Model;
using System.Collections.Generic;
using System.Linq;

namespace EROptimizer.Dto
{
    public class TalismanDto
    {
        public int TalismanId { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public double PhysicalBonus { get; set; }
        public double PhysicalStrikeBonus { get; set; }
        public double PhysicalSlashBonus { get; set; }
        public double PhysicalPierceBonus { get; set; }

        public double? PhysicalBonusPVP { get; set; }
        public double? PhysicalStrikeBonusPVP { get; set; }
        public double? PhysicalSlashBonusPVP { get; set; }
        public double? PhysicalPierceBonusPVP { get; set; }

        public double MagicBonus { get; set; }
        public double FireBonus { get; set; }
        public double LightningBonus { get; set; }
        public double HolyBonus { get; set; }

        public double? MagicBonusPVP { get; set; }
        public double? FireBonusPVP { get; set; }
        public double? LightningBonusPVP { get; set; }
        public double? HolyBonusPVP { get; set; }

        public double ImmunityBonus { get; set; }
        public double RobustnessBonus { get; set; }
        public double FocusBonus { get; set; }
        public double VitalityBonus { get; set; }

        public double PoiseBonus { get; set; }

        public double WeightBonus { get; set; }

        public double EnduranceBonus { get; set; }

        public TalismanDto() { }

        public TalismanDto(Talisman t, int id)
        {
            TalismanId = id;

            Name = t.Name;
            Weight = t.Weight;

            PhysicalBonus = t.PhysicalBonus;
            PhysicalStrikeBonus = t.PhysicalStrikeBonus;
            PhysicalSlashBonus = t.PhysicalSlashBonus;
            PhysicalPierceBonus = t.PhysicalPierceBonus;

            PhysicalBonusPVP = t.PhysicalBonusPVP;
            PhysicalStrikeBonusPVP = t.PhysicalStrikeBonusPVP;
            PhysicalSlashBonusPVP = t.PhysicalSlashBonusPVP;
            PhysicalPierceBonusPVP = t.PhysicalPierceBonusPVP;

            MagicBonus = t.MagicBonus;
            FireBonus = t.FireBonus;
            LightningBonus = t.LightningBonus;
            HolyBonus = t.HolyBonus;

            MagicBonusPVP = t.MagicBonusPVP;
            FireBonusPVP = t.FireBonusPVP;
            LightningBonusPVP = t.LightningBonusPVP;
            HolyBonusPVP = t.HolyBonusPVP;

            ImmunityBonus = t.ImmunityBonus;
            RobustnessBonus = t.RobustnessBonus;
            FocusBonus = t.FocusBonus;
            VitalityBonus = t.VitalityBonus;

            PoiseBonus = t.PoiseBonus;

            WeightBonus = t.WeightBonus;

            EnduranceBonus = t.EnduranceBonus;
        }        
    }
}
