
export interface IOptimizerResult {
  headPieceId: number;
  chestPieceId: number;
  gauntletsPieceId: number;
  legsPieceId: number;

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
