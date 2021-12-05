
export enum ArmorPieceTypeEnum {
  Head = 0,
  Chest = 1,
  Gauntlets = 2,
  Legs = 3,
}

export interface IArmorPieceDto {
  armorSetId: number;
  name: string;
  type: ArmorPieceTypeEnum;

  physical: number;
  physicalStrike: number;
  physicalSlash: number;
  physicalPierce: number;

  magic: number;
  fire: number;
  lightning: number;
  holy: number;

  immunity: number;
  robustness: number;
  focus: number;
  death: number;

  weight: number;
}
