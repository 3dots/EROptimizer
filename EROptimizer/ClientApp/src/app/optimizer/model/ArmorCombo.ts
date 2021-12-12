import { IArmorPieceDto } from "../../../service/dto/IArmorPieceDto";
import { OptimizerConfigDto } from "./OptimizerConfigDto";

export class ArmorCombo {

  public constructor(
    public head: IArmorPieceDto,
    public chest: IArmorPieceDto,
    public gauntlets: IArmorPieceDto,
    public legs: IArmorPieceDto,
    config: OptimizerConfigDto) {

  }

  //physical: number;
  //physicalStrike: number;
  //physicalSlash: number;
  //physicalPierce: number;

  //magic: number;
  //fire: number;
  //lightning: number;
  //holy: number;

  //immunity: number;
  //robustness: number;
  //focus: number;
  //death: number;

  //weight: number;

  //score: number;

  //calculateScore(config: OptimizerConfigDto): number {

  //}
}
