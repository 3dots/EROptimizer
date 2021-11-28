
export interface IArmorPieceDto {
  armorSetId: number;
  name: string;

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
