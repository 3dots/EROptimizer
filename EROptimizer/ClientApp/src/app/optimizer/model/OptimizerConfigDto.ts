import { IArmorDataDto } from "../../../service/dto/IArmorDataDto";
import { ITalismanDto } from "../../../service/dto/ITalismanDto";

export class OptimizerConfigDto {

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

  disabledList: string[] = [];

  configType: ConfigTypeEnum = ConfigTypeEnum.StatsAndDropdowns;

  endurance: number = 8;

  totalAvailableWeightCalc: number = 0;
  totalAvailableWeightCalcArmorBonusHack: number = 0;

  talisman1Id: number | null = null;
  talisman2Id: number | null = null;
  talisman3Id: number | null = null;
  talisman4Id: number | null = null;

  optimizeForType: OptimizeForEnum = OptimizeForEnum.PVP;

  filterOverrideHeadName: string | null = null;
  filterOverrideChestName: string | null = null;
  filterOverrideGauntletsName: string | null = null;
  filterOverrideLegsName: string | null = null;
  
  public constructor(init?: Partial<OptimizerConfigDto>) {
    Object.assign(this, init);
  }

  public get totalAvailableWeight() {
    return this.maxWeight * this.weightFractionGoal
      - this.rightHand1 - this.rightHand2 - this.rightHand3
      - this.leftHand1 - this.leftHand2 - this.leftHand3
      - this.talisman1 - this.talisman2 - this.talisman3 - this.talisman4;
  }

  public equipLoad(armorData: IArmorDataDto): number {

    let talisman1: ITalismanDto | null | undefined = null;
    if (this.talisman1Id) talisman1 = armorData.talismans.find(x => x.talismanId == this.talisman1Id);

    let talisman2: ITalismanDto | null | undefined = null;
    if (this.talisman2Id) talisman2 = armorData.talismans.find(x => x.talismanId == this.talisman2Id);

    let talisman3: ITalismanDto | null | undefined = null;
    if (this.talisman3Id) talisman3 = armorData.talismans.find(x => x.talismanId == this.talisman3Id);

    let talisman4: ITalismanDto | null | undefined = null;
    if (this.talisman4Id) talisman4 = armorData.talismans.find(x => x.talismanId == this.talisman4Id);

    let endurance = this.endurance +
      (talisman1?.enduranceBonus ?? 0) + (talisman2?.enduranceBonus ?? 0) + (talisman3?.enduranceBonus ?? 0) + (talisman4?.enduranceBonus ?? 0);

    return armorData.equipLoadArray[endurance - 8] *
      (1 + (talisman1?.weightBonus ?? 0) / 100) *
      (1 + (talisman2?.weightBonus ?? 0) / 100) *
      (1 + (talisman3?.weightBonus ?? 0) / 100) *
      (1 + (talisman4?.weightBonus ?? 0) / 100);
  }

  public totalAvailableWeightStats(armorData: IArmorDataDto, baseEndurance: number): number {
    //console.log("totalAvailableWeightStats()");
    //copy of equipLoad()

    let talisman1: ITalismanDto | null | undefined = null;
    if (this.talisman1Id) talisman1 = armorData.talismans.find(x => x.talismanId == this.talisman1Id);

    let talisman2: ITalismanDto | null | undefined = null;
    if (this.talisman2Id) talisman2 = armorData.talismans.find(x => x.talismanId == this.talisman2Id);

    let talisman3: ITalismanDto | null | undefined = null;
    if (this.talisman3Id) talisman3 = armorData.talismans.find(x => x.talismanId == this.talisman3Id);

    let talisman4: ITalismanDto | null | undefined = null;
    if (this.talisman4Id) talisman4 = armorData.talismans.find(x => x.talismanId == this.talisman4Id);

    
    let endurance = baseEndurance +
      (talisman1?.enduranceBonus ?? 0) + (talisman2?.enduranceBonus ?? 0) + (talisman3?.enduranceBonus ?? 0) + (talisman4?.enduranceBonus ?? 0);
    //console.log(endurance);

    let equipLoadFromStats = armorData.equipLoadArray[endurance - 8]
    //console.log(equipLoadFromStats);

    let equipLoad = equipLoadFromStats *
      (1 + (talisman1?.weightBonus ?? 0) / 100) *
      (1 + (talisman2?.weightBonus ?? 0) / 100) *
      (1 + (talisman3?.weightBonus ?? 0) / 100) *
      (1 + (talisman4?.weightBonus ?? 0) / 100);
    //console.log(equipLoad);

    let weightLeft = equipLoad * this.weightFractionGoal
      - this.rightHand1 - this.rightHand2 - this.rightHand3
      - this.leftHand1 - this.leftHand2 - this.leftHand3;

    //console.log(weightLeft);

    if (talisman1) weightLeft -= talisman1.weight;
    if (talisman2) weightLeft -= talisman2.weight;
    if (talisman3) weightLeft -= talisman3.weight;
    if (talisman4) weightLeft -= talisman4.weight;

    //console.log(weightLeft);

    return weightLeft;
  }

  public SelectedTalismans(allTalismans: ITalismanDto[]): ITalismanDto[] {

    if (this.configType == ConfigTypeEnum.Weights) return [];

    let selectedTalismans: ITalismanDto[] = [];

    let talisman: ITalismanDto | null | undefined = null;
    if (this.talisman1Id) talisman = allTalismans.find(x => x.talismanId == this.talisman1Id);
    if (talisman) selectedTalismans.push(talisman);

    talisman = null;
    if (this.talisman2Id) talisman = allTalismans.find(x => x.talismanId == this.talisman2Id);
    if (talisman) selectedTalismans.push(talisman);

    talisman = null;
    if (this.talisman3Id) talisman = allTalismans.find(x => x.talismanId == this.talisman3Id);
    if (talisman) selectedTalismans.push(talisman);

    talisman = null;
    if (this.talisman4Id) talisman = allTalismans.find(x => x.talismanId == this.talisman4Id);
    if (talisman) selectedTalismans.push(talisman);

    return selectedTalismans;
  }

  public calcTotal(armorData: IArmorDataDto) {
    this.totalAvailableWeightCalc = this.configType == ConfigTypeEnum.Weights ?
      this.totalAvailableWeight :
      this.totalAvailableWeightStats(armorData, this.endurance);

    //Armor Endurance bonus value is alwayes 2 currently. If that changes, this won't work.
    //Basically, I don't want to pass the entire equipLoadArray into each thread worker currently.
    this.totalAvailableWeightCalcArmorBonusHack = this.configType == ConfigTypeEnum.Weights ?
      this.totalAvailableWeight :
      this.totalAvailableWeightStats(armorData, this.endurance + 2);
  }
}

export enum ConfigTypeEnum {
  Weights = 0,
  StatsAndDropdowns = 1,
}

export enum OptimizeForEnum {
  PVP = 0,
  PVE = 1,
}
