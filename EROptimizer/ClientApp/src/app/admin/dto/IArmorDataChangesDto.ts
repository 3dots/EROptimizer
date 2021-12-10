import { IArmorPieceChangesDto } from "./IArmorPieceChangesDto";

export interface IArmorDataChangesDto {
  messages: string[];
  armorPieceChanges: IArmorPieceChangesDto[];
}
