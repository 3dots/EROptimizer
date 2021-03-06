import { IArmorPieceDto } from './IArmorPieceDto';
import { IArmorSetDto } from './IArmorSetDto';
import { ITalismanDto } from './ITalismanDto';

export class ArmorDataDto implements IArmorDataDto {
  head: IArmorPieceDto[] = [];
  chest: IArmorPieceDto[] = [];
  gauntlets: IArmorPieceDto[] = [];
  legs: IArmorPieceDto[] = [];

  armorSets: IArmorSetDto[] = [];

  equipLoadArray: number[] = [];

  talismans: ITalismanDto[] = [];

  public constructor(init?: Partial<ArmorDataDto>) {
    Object.assign(this, init);
  }
}

export interface IArmorDataDto {
  head: IArmorPieceDto[];
  chest: IArmorPieceDto[];
  gauntlets: IArmorPieceDto[];
  legs: IArmorPieceDto[];

  armorSets: IArmorSetDto[];

  equipLoadArray: number[];

  talismans: ITalismanDto[];
}
