import { IArmorPieceDto } from './IArmorPieceDto';
import { IArmorSetDto } from './IArmorSetDto';

export interface IArmorDataDto {
  head: IArmorPieceDto[];
  chest: IArmorPieceDto[];
  gauntlets: IArmorPieceDto[];
  legs: IArmorPieceDto[];

  armorSets: IArmorSetDto[];
}
