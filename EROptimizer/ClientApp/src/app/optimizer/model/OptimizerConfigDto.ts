
export class OptimizerConfigDto {

  strength: number = 10;
  weightFractionGoal: number = 0.6999;

  rightHand1: number = 0;
  rightHand2: number = 0;
  rightHand3: number = 0;
  leftHand1: number = 0;
  leftHand2: number = 0;
  leftHand3: number = 0;

  talisman1: number = 0;
  talisman2: number = 0;
  talisman3: number = 0;
  talisman4: number = 0;

  minAvgPhysical: number = 0;
  minPhysical: number = 0;
  minPhysicalStrike: number = 0;
  minPhysicalSlash: number = 0;
  minPhysicalPierce: number = 0;

  minMagic: number = 0;
  minFire: number = 0;
  minLightning: number = 0;
  minHoly: number = 0;

  minImmunity: number = 0;
  minRobustness: number = 0;
  minFocus: number = 0;
  minVitality: number = 0;

  minPoise: number = 0;

  priPhysical: number = 1;
  priPhysicalStrike: number = 1;
  priPhysicalSlash: number = 1;
  priPhysicalPierce: number = 1;

  priMagic: number = 0;
  priFire: number = 0;
  priLightning: number = 0;
  priHoly: number = 0;

  priImmunity: number = 0;
  priRobustness: number = 0;
  priFocus: number = 0;
  priVitality: number = 0;

  priPoise: number = 0;

  numberOfResults: number = 10;
  numberOfThreads: number = 4;

  maxWeight: number = 45;

  //public get maxWeight() {
  //  return this.strength * 2; //todo get correct formula
  //}

  public get totalAvailableWeight() {
    return this.maxWeight * this.weightFractionGoal
      - this.rightHand1 - this.rightHand2 - this.rightHand3
      - this.leftHand1 - this.leftHand2 - this.leftHand3
      - this.talisman1 - this.talisman2  - this.talisman3 - this.talisman4;
  }

  public constructor(init?: Partial<OptimizerConfigDto>) {
    Object.assign(this, init);
  }
}
