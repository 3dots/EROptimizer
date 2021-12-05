import { IArmorPieceDto } from "./IArmorPieceDto";

export interface IArmorSetDto {
  armorSetId: number;
  name: string;

  armorPieces: IArmorPieceDto[];
}
