
export interface ITalismanDto {
  talismanId: number;

  name: string;
  weight: number;

  physicalBonus: number;
  physicalStrikeBonus: number;
  physicalSlashBonus: number;
  physicalPierceBonus: number;

  physicalBonusPVP: number | null;
  physicalStrikeBonusPVP: number | null;
  physicalSlashBonusPVP: number | null;
  physicalPierceBonusPVP: number | null;

  magicBonus: number;
  fireBonus: number;
  lightningBonus: number;
  holyBonus: number;

  magicBonusPVP: number | null;
  fireBonusPVP: number | null;
  lightningBonusPVP: number | null;
  holyBonusPVP: number | null;

  immunityBonus: number;
  robustnessBonus: number;
  focusBonus: number;
  vitalityBonus: number;

  poiseBonus: number;

  weightBonus: number;

  enduranceBonus: number;
}
