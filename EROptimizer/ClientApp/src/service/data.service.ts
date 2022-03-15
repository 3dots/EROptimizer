import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, shareReplay, map } from 'rxjs';

import { IArmorDataDto } from './dto/IArmorDataDto';
import { IArmorSetDto } from './dto/IArmorSetDto';
import { ArmorPieceTypeEnum, IArmorPieceDto } from './dto/IArmorPieceDto';
import { ArmorCombo } from '../app/optimizer/model/ArmorCombo';
import { OptimizerConfigDto } from '../app/optimizer/model/OptimizerConfigDto';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  private url: string = "/data/";
  private _armorDataObservable!: Observable<IArmorDataDto>;
  private _armorData!: IArmorDataDto;

  private _localStorageKey = "EROptimizerKey";

  config: OptimizerConfigDto;

  constructor(private http: HttpClient) {

    let json = localStorage.getItem(this._localStorageKey);
    if (json) {
      try {
        this.config = new OptimizerConfigDto(JSON.parse(json));
      } catch {
        this.config = new OptimizerConfigDto();
      }
    } else {
      this.config = new OptimizerConfigDto();
    }

    //console.log(this.config.disabledList);
  }

  get armorData(): Observable<IArmorDataDto> {
    if (this._armorDataObservable == null) {
      this._armorDataObservable = this.http.get<IArmorDataDto>(this.url).pipe(map(x => {

        x.head.forEach(this.enableItem.bind(this));
        x.chest.forEach(this.enableItem.bind(this));
        x.gauntlets.forEach(this.enableItem.bind(this));
        x.legs.forEach(this.enableItem.bind(this));

        x.armorSets.forEach((s: IArmorSetDto) => {

          //0 index piece is the "None".
          let head = x.head.find(p => p.armorSetIds.includes(s.armorSetId)) ?? x.head[0];
          let chest = x.chest.find(p => p.armorSetIds.includes(s.armorSetId)) ?? x.chest[0];
          let gauntlets = x.gauntlets.find(p => p.armorSetIds.includes(s.armorSetId)) ?? x.gauntlets[0];
          let legs = x.legs.find(p => p.armorSetIds.includes(s.armorSetId)) ?? x.legs[0];

          s.combo = new ArmorCombo(head, chest, gauntlets, legs, null);

          //console.log(`${s.name} ${head.name} ${chest.name} ${gauntlets.name} ${legs.name}`);

        });

        this._armorData = x;

        return x;
      })).pipe(shareReplay(1));
      return this._armorDataObservable;
    } else {
      return this._armorDataObservable;
    }
  }

  storeToLocalStorage() {

    this.config.disabledList = [
      ...this._armorData.head.filter(x => !x.isEnabled).map(x => x.name == "None" ? "NoneHead" : x.name),
      ...this._armorData.chest.filter(x => !x.isEnabled).map(x => x.name == "None" ? "NoneChest" : x.name),
      ...this._armorData.gauntlets.filter(x => !x.isEnabled).map(x => x.name == "None" ? "NoneGauntlets" : x.name),
      ...this._armorData.legs.filter(x => !x.isEnabled).map(x => x.name == "None" ? "NoneLegs" : x.name),
    ];

    localStorage.setItem(this._localStorageKey, JSON.stringify(this.config));

    this.config.disabledList = []; //dont store in memory.
  }

  enableItem(p: IArmorPieceDto) {

    let pieceName = p.name;

    if (p.name == "None") {

      if (p.type == ArmorPieceTypeEnum.Head) pieceName = "NoneHead";
      else if (p.type == ArmorPieceTypeEnum.Chest) pieceName = "NoneChest";
      else if (p.type == ArmorPieceTypeEnum.Gauntlets) pieceName = "NoneGauntlets";
      else pieceName = "NoneLegs";
    }

    if (!this.config.disabledList.includes(pieceName)) {
      p.isEnabled = true;
    }
  }

  resetConfig(): OptimizerConfigDto {

    this.config = new OptimizerConfigDto();

    this._armorData.head.forEach(this.enableItem.bind(this));
    this._armorData.chest.forEach(this.enableItem.bind(this));
    this._armorData.gauntlets.forEach(this.enableItem.bind(this));
    this._armorData.legs.forEach(this.enableItem.bind(this));

    return this.config;
  }
}
