
export enum ArmorPieceTypeEnum {
  Head = 0,
  Chest = 1,
  Gauntlets = 2,
  Legs = 3,
}

export interface IArmorPieceDto {
  armorPieceId: number;
  armorSetId: number;
  name: string;
  type: ArmorPieceTypeEnum;

  isEnabled: boolean;

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
