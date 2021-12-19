import { IArmorPieceDto } from "../../../service/dto/IArmorPieceDto";
import { OptimizerConfigDto } from "./OptimizerConfigDto";

export class ArmorCombo {

  public constructor(
    public head: IArmorPieceDto,
    public chest: IArmorPieceDto,
    public gauntlets: IArmorPieceDto,
    public legs: IArmorPieceDto,
    config: OptimizerConfigDto | null) {

    this.physical = head.physical + chest.physical + gauntlets.physical + legs.physical;
    this.physicalStrike = head.physicalStrike + chest.physicalStrike + gauntlets.physicalStrike + legs.physicalStrike;
    this.physicalSlash = head.physicalSlash + chest.physicalSlash + gauntlets.physicalSlash + legs.physicalSlash;
    this.physicalPierce = head.physicalPierce + chest.physicalPierce + gauntlets.physicalPierce + legs.physicalPierce;

    this.avgPhysical = (this.physical + this.physicalStrike + this.physicalSlash + this.physicalPierce) / 4;

    this.magic = head.magic + chest.magic + gauntlets.magic + legs.magic;
    this.fire = head.fire + chest.fire + gauntlets.fire + legs.fire;
    this.lightning = head.lightning + chest.lightning + gauntlets.lightning + legs.lightning;
    this.holy = head.holy + chest.holy + gauntlets.holy + legs.holy;

    this.immunity = head.immunity + chest.immunity + gauntlets.immunity + legs.immunity;
    this.robustness = head.robustness + chest.robustness + gauntlets.robustness + legs.robustness;
    this.focus = head.focus + chest.focus + gauntlets.focus + legs.focus;
    this.death = head.death + chest.death + gauntlets.death + legs.death;

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
        config.priDeath * this.death;
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
  death: number;

  weight: number;

  score: number;
}
