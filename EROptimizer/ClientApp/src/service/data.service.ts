import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, shareReplay, map } from 'rxjs';

import { IArmorDataDto } from './dto/IArmorDataDto';
import { IArmorSetDto } from './dto/ArmorSetDto';
import { IArmorPieceDto } from './dto/IArmorPieceDto';
import { ArmorCombo } from '../app/optimizer/model/ArmorCombo';
import { OptimizerConfigDto } from '../app/optimizer/model/OptimizerConfigDto';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  private url: string = "/data/";
  private _armorData!: Observable<IArmorDataDto>;

  config: OptimizerConfigDto = new OptimizerConfigDto();

  constructor(private http: HttpClient) {

  }

  get armorData(): Observable<IArmorDataDto> {
    if (this._armorData == null) {
      this._armorData = this.http.get<IArmorDataDto>(this.url).pipe(map(x => {

        //todo wire to cookie storage
        x.head.forEach((p: IArmorPieceDto) => { p.isEnabled = true; });
        x.chest.forEach((p: IArmorPieceDto) => { p.isEnabled = true; });
        x.gauntlets.forEach((p: IArmorPieceDto) => { p.isEnabled = true; });
        x.legs.forEach((p: IArmorPieceDto) => { p.isEnabled = true; });

        x.armorSets.forEach((s: IArmorSetDto) => {

          //0 index piece is the "None".
          let head = x.head.find(p => p.armorSetId == s.armorSetId) ?? x.head[0];
          let chest = x.chest.find(p => p.armorSetId == s.armorSetId) ?? x.chest[0];
          let gauntlets = x.gauntlets.find(p => p.armorSetId == s.armorSetId) ?? x.gauntlets[0];    
          let legs = x.legs.find(p => p.armorSetId == s.armorSetId) ?? x.legs[0];

          s.combo = new ArmorCombo(head, chest, gauntlets, legs, null);

          //console.log(`${s.name} ${head.name} ${chest.name} ${gauntlets.name} ${legs.name}`);

        });

        return x;
      })).pipe(shareReplay(1));
      return this._armorData;
    } else {
      return this._armorData;
    }
  }
}
