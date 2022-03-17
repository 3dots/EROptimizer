import { Component, OnInit } from '@angular/core';
import { ERBuild } from './model/ERBuild';

@Component({
  selector: 'app-best-class',
  templateUrl: './best-class.component.html',
  styleUrls: ['./best-class.component.css']
})
export class BestClassComponent implements OnInit {

  yourBuild: ERBuild = new ERBuild();
  startingClasses: ERBuild[];

  constructor() {
    this.startingClasses = [];
    this.startingClasses.push(new ERBuild({ name: "Hero", startingLevel: 7, vigor: 14, mind: 9, endurance: 12, strength: 16, dexterity: 9, intelligence: 7, faith: 8, arcane: 11 }));
    this.startingClasses.push(new ERBuild({ name: "Bandit", startingLevel: 5, vigor: 10, mind: 11, endurance: 10, strength: 9, dexterity: 13, intelligence: 9, faith: 8, arcane: 14 }));
    this.startingClasses.push(new ERBuild({ name: "Astrologer", startingLevel: 6, vigor: 9, mind: 15, endurance: 9, strength: 8, dexterity: 12, intelligence: 16, faith: 7, arcane: 9 }));
    this.startingClasses.push(new ERBuild({ name: "Warrior", startingLevel: 8, vigor: 11, mind: 12, endurance: 11, strength: 10, dexterity: 16, intelligence: 10, faith: 8, arcane: 9 }));
    this.startingClasses.push(new ERBuild({ name: "Prisoner", startingLevel: 9, vigor: 11, mind: 12, endurance: 11, strength: 11, dexterity: 14, intelligence: 14, faith: 6, arcane: 9 }));
    this.startingClasses.push(new ERBuild({ name: "Confessor", startingLevel: 10, vigor: 10, mind: 13, endurance: 10, strength: 12, dexterity: 12, intelligence: 9, faith: 14, arcane: 9 }));
    this.startingClasses.push(new ERBuild({ name: "Wretch", startingLevel: 1, vigor: 10, mind: 10, endurance: 10, strength: 10, dexterity: 10, intelligence: 10, faith: 10, arcane: 10 }));
    this.startingClasses.push(new ERBuild({ name: "Vagabond", startingLevel: 9, vigor: 15, mind: 10, endurance: 11, strength: 14, dexterity: 13, intelligence: 9, faith: 9, arcane: 7 }));
    this.startingClasses.push(new ERBuild({ name: "Prophet", startingLevel: 7, vigor: 10, mind: 14, endurance: 8, strength: 11, dexterity: 10, intelligence: 7, faith: 16, arcane: 10 }));
    this.startingClasses.push(new ERBuild({ name: "Samurai", startingLevel: 9, vigor: 12, mind: 11, endurance: 13, strength: 12, dexterity: 15, intelligence: 9, faith: 8, arcane: 8 }));

    this.setMinStats();
  }

  setMinStats() {
    this.yourBuild.vigor = Math.min(...this.startingClasses.map(x => x.vigor));
    this.yourBuild.mind = Math.min(...this.startingClasses.map(x => x.mind));
    this.yourBuild.endurance = Math.min(...this.startingClasses.map(x => x.endurance));
    this.yourBuild.strength = Math.min(...this.startingClasses.map(x => x.strength));
    this.yourBuild.dexterity = Math.min(...this.startingClasses.map(x => x.dexterity));
    this.yourBuild.intelligence = Math.min(...this.startingClasses.map(x => x.intelligence));
    this.yourBuild.faith = Math.min(...this.startingClasses.map(x => x.faith));
    this.yourBuild.arcane = Math.min(...this.startingClasses.map(x => x.arcane));
  }

  ngOnInit(): void {
  }

  soulLevelNeeded(startingClass: ERBuild): number {

    let levelUpsNeeded = 0;
    if (this.yourBuild.vigor > startingClass.vigor) levelUpsNeeded += this.yourBuild.vigor - startingClass.vigor;
    if (this.yourBuild.mind > startingClass.mind) levelUpsNeeded += this.yourBuild.mind - startingClass.mind;
    if (this.yourBuild.endurance > startingClass.endurance) levelUpsNeeded += this.yourBuild.endurance - startingClass.endurance;
    if (this.yourBuild.strength > startingClass.strength) levelUpsNeeded += this.yourBuild.strength - startingClass.strength;
    if (this.yourBuild.dexterity > startingClass.dexterity) levelUpsNeeded += this.yourBuild.dexterity - startingClass.dexterity;
    if (this.yourBuild.intelligence > startingClass.intelligence) levelUpsNeeded += this.yourBuild.intelligence - startingClass.intelligence;
    if (this.yourBuild.faith > startingClass.faith) levelUpsNeeded += this.yourBuild.faith - startingClass.faith;
    if (this.yourBuild.arcane > startingClass.arcane) levelUpsNeeded += this.yourBuild.arcane - startingClass.arcane;

    return startingClass.startingLevel + levelUpsNeeded;
  }

  get bestClass(): ERBuild {

    let bestClass = this.startingClasses[0];
    let smallestLevel = this.soulLevelNeeded(bestClass);

    for (let i = 1; i < this.startingClasses.length; i++) {
      let soulLevelNeeded = this.soulLevelNeeded(this.startingClasses[i]);
      if (soulLevelNeeded < smallestLevel) {
        smallestLevel = soulLevelNeeded;
        bestClass = this.startingClasses[i];
      }
    }

    return bestClass;
  }

  reset() {
    this.setMinStats();
  }
}
