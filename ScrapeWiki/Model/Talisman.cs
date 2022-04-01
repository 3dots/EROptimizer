using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeWiki.Model
{
    public class Talisman
    {
        public string Name { get; set; }
        public double Weight { get; set; }

        public double? PhysicalBonus { get; set; }
        public double? PhysicalStrikeBonus { get; set; }
        public double? PhysicalSlashBonus { get; set; }
        public double? PhysicalPierceBonus { get; set; }

        public double? MagicBonus { get; set; }
        public double? FireBonus { get; set; }
        public double? LightningBonus { get; set; }
        public double? HolyBonus { get; set; }

        public double? ImmunityBonus { get; set; }
        public double? RobustnessBonus { get; set; }
        public double? FocusBonus { get; set; }
        public double? VitalityBonus { get; set; }

        public double? PoiseBonus { get; set; }

        public double? WeightBonus { get; set; }

        public double? EnduranceBonus { get; set; }

        public static List<Talisman> Generate(List<Talisman> list)
        {           
            foreach (Talisman t in list.ToList())
            {
                if (t.Name == "Arsenal Charm")
                {
                    t.WeightBonus = 15;

                    list.Add(new Talisman() {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                        WeightBonus = 17,
                    });
                }
                else if (t.Name == "Great-Jar's Arsenal")
                {
                    t.WeightBonus = 19;
                }
                else if (t.Name == "Erdtree's Favor")
                {
                    t.WeightBonus = 5;

                    list.Add(new Talisman()
                    {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                        WeightBonus = 6.5,
                    });

                    list.Add(new Talisman()
                    {
                        Name = $"{t.Name} +2",
                        Weight = t.Weight,
                        WeightBonus = 8,
                    });
                }
                else if (t.Name == "Radagon's Scarseal")
                {
                    t.EnduranceBonus = 3;
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus = 
                        t.MagicBonus = t.FireBonus = t.LightningBonus = t.HolyBonus = -10;
                }
                else if (t.Name == "Radagon's Soreseal")
                {
                    t.EnduranceBonus = 5;
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus =
                        t.MagicBonus = t.FireBonus = t.LightningBonus = t.HolyBonus = -15;
                }
                else if (t.Name == "Stalwart Horn Charm")
                {
                    t.RobustnessBonus = 90;

                    list.Add(new Talisman()
                    {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                        RobustnessBonus = 140,
                    });
                }
                else if (t.Name == "Immunizing Horn Charm")
                {
                    t.ImmunityBonus = 90;

                    list.Add(new Talisman()
                    {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                        ImmunityBonus = 140,
                    });
                }
                else if (t.Name == "Clarifying Horn Charm")
                {
                    t.FocusBonus = 90;

                    list.Add(new Talisman()
                    {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                        FocusBonus = 140,
                    });
                }
                else if (t.Name == "Prince of Death's Pustule")
                {
                    t.VitalityBonus = 90;

                    list.Add(new Talisman()
                    {
                        Name = "Prince of Death's Cyst",
                        Weight = 0.6,
                        VitalityBonus = 140,
                    });
                }
                else if (t.Name == "Mottled Necklace")
                {
                    //Wiki doesn't have numbers

                    list.Add(new Talisman()
                    {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                    });
                }
                else if (t.Name == "Bull-Goat's Talisman")
                {
                    t.PoiseBonus = 100/3;
                }
                else if (t.Name == "Marika's Scarseal")
                {
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus =
                        t.MagicBonus = t.FireBonus = t.LightningBonus = t.HolyBonus = -10;
                }
                else if (t.Name == "Marika's Soreseal")
                {
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus =
                        t.MagicBonus = t.FireBonus = t.LightningBonus = t.HolyBonus = -15;
                }
                else if (t.Name == "Magic Scorpion Charm" ||
                    t.Name == "Lightning Scorpion Charm" ||
                    t.Name == "Fire Scorpion Charm" ||
                    t.Name == "Sacred Scorpion Charm")
                {
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus = -10;
                }
                else if (t.Name == "Dragoncrest Shield Talisman")
                {
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus = 10;

                    var tPlus = new Talisman()
                    {
                        Name = $"{t.Name} +1",
                        Weight = t.Weight,
                    };
                    tPlus.PhysicalBonus = tPlus.PhysicalStrikeBonus = tPlus.PhysicalSlashBonus = tPlus.PhysicalPierceBonus = 13;
                    list.Add(tPlus);

                    tPlus = new Talisman()
                    {
                        Name = $"{t.Name} +2",
                        Weight = t.Weight,
                    };
                    tPlus.PhysicalBonus = tPlus.PhysicalStrikeBonus = tPlus.PhysicalSlashBonus = tPlus.PhysicalPierceBonus = 17;
                    list.Add(tPlus);
                }
                else if (t.Name == "Dragoncrest Greatshield Talisman")
                {
                    t.PhysicalBonus = t.PhysicalStrikeBonus = t.PhysicalSlashBonus = t.PhysicalPierceBonus = 20;
                }
                //else if ()
            }

            return list;
        }
    }
}
