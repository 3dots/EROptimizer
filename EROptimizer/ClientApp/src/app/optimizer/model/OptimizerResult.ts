import { ArmorCombo } from "./ArmorCombo";

export class OptimizerResult implements IOptimizerResult {
  headPieceId: number;
  chestPieceId: number;
  gauntletsPieceId: number;
  legsPieceId: number;

  constructor(combo: ArmorCombo) {
    this.headPieceId = combo.head.armorPieceId;
    this.chestPieceId = combo.chest.armorPieceId;
    this.gauntletsPieceId = combo.gauntlets.armorPieceId;
    this.legsPieceId = combo.legs.armorPieceId;
  }
}

export interface IOptimizerResult {
  headPieceId: number;
  chestPieceId: number;
  gauntletsPieceId: number;
  legsPieceId: number;
}
