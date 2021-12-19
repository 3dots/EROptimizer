
export class OptimizerConfigDto {

  strength: number = 10;
  weightFractionGoal: number = 0.5;

  rightHand1: number = 0;
  rightHand2: number = 0;
  rightHand3: number = 0;
  leftHand1: number = 0;
  leftHand2: number = 0;
  leftHand3: number = 0;

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
  minDeath: number = 0;

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
  priDeath: number = 0;

  numberOfResults: number = 10;
  numberOfThreads: number = 4;

  public get maxWeight() {
    return this.strength * 2; //todo get correct formula
  }

  public get totalAvailableWeight() {
    return this.maxWeight * this.weightFractionGoal;
  }

  public constructor(init?: Partial<OptimizerConfigDto>) {
    Object.assign(this, init);
  }
}
