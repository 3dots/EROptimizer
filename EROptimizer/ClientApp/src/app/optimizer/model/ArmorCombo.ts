import { IArmorPieceDto } from "../../../service/dto/IArmorPieceDto";
import { OptimizerConfigDto } from "./OptimizerConfigDto";

export class ArmorCombo {

  public constructor(
    public head: IArmorPieceDto,
    public chest: IArmorPieceDto,
    public gauntlets: IArmorPieceDto,
    public legs: IArmorPieceDto,
    config: OptimizerConfigDto | null) {

    this.physical = 100 * (1 - (1 - head.physical / 100) * (1 - chest.physical / 100) * (1 - gauntlets.physical / 100) * (1 - legs.physical / 100));
    this.physicalStrike = 100 * (1 - (1 - head.physicalStrike / 100) * (1 - chest.physicalStrike / 100) * (1 - gauntlets.physicalStrike / 100) * (1 - legs.physicalStrike / 100));
    this.physicalSlash = 100 * (1 - (1 - head.physicalSlash / 100) * (1 - chest.physicalSlash / 100) * (1 - gauntlets.physicalSlash / 100) * (1 - legs.physicalSlash / 100));
    this.physicalPierce = 100 * (1 - (1 - head.physicalPierce / 100) * (1 - chest.physicalPierce / 100) * (1 - gauntlets.physicalPierce / 100) * (1 - legs.physicalPierce / 100));

    this.avgPhysical = (this.physical + this.physicalStrike + this.physicalSlash + this.physicalPierce) / 4;

    this.magic = 100 * (1 - (1 - head.magic / 100) * (1 - chest.magic / 100) * (1 - gauntlets.magic / 100) * (1 - legs.magic / 100));
    this.fire = 100 * (1 - (1 - head.fire / 100) * (1 - chest.fire / 100) * (1 - gauntlets.fire / 100) * (1 - legs.fire / 100));
    this.lightning = 100 * (1 - (1 - head.lightning / 100) * (1 - chest.lightning / 100) * (1 - gauntlets.lightning / 100) * (1 - legs.lightning / 100));
    this.holy = 100 * (1 - (1 - head.holy / 100) * (1 - chest.holy / 100) * (1 - gauntlets.holy / 100) * (1 - legs.holy / 100));

    this.immunity = head.immunity + chest.immunity + gauntlets.immunity + legs.immunity;
    this.robustness = head.robustness + chest.robustness + gauntlets.robustness + legs.robustness;
    this.focus = head.focus + chest.focus + gauntlets.focus + legs.focus;
    this.vitality = head.vitality + chest.vitality + gauntlets.vitality + legs.vitality;

    this.poise = head.poise + chest.poise + gauntlets.poise + legs.poise;

    this.weight = head.weight + chest.weight + gauntlets.weight + legs.weight;

    if (config != null) {
      this.score = config.priPhysical * this.physical +
        config.priPhysicalStrike * this.physicalStrike +
        config.priPhysicalSlash * this.physicalSlash +
        config.priPhysicalPierce * this.physicalPierce +
        config.priMagic * this.magic +
        config.priFire * this.fire +
        config.priLightning * this.lightning +
        config.priHoly * this.holy +
        config.priImmunity * this.immunity +
        config.priRobustness * this.robustness +
        config.priFocus * this.focus +
        config.priVitality * this.vitality;
    } else {
      this.score = -1;
    }
  }

  physical: number;
  physicalStrike: number;
  physicalSlash: number;
  physicalPierce: number;

  avgPhysical: number;

  magic: number;
  fire: number;
  lightning: number;
  holy: number;

  immunity: number;
  robustness: number;
  focus: number;
  vitality: number;

  poise: number;

  weight: number;

  score: number;
}
