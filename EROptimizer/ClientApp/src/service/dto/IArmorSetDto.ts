import { ArmorCombo } from "../../app/optimizer/model/ArmorCombo";
import { IArmorPieceDto } from "./IArmorPieceDto";

export interface IArmorSetDto {
  armorSetId: number;
  name: string;

  combo: ArmorCombo;
}
