
export class ERBuild {

  name: string | null = null;

  vigor: number = 0;
  mind: number = 0;
  endurance: number = 0;
  strength: number = 0;
  dexterity: number = 0;
  intelligence: number = 0;
  faith: number = 0;
  arcane: number = 0;

  startingLevel: number = 0;

  isBest: boolean = false;

  public constructor(init?: Partial<ERBuild>) {
    Object.assign(this, init);
  }
}
