import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, shareReplay, map } from 'rxjs';

import { IArmorDataDto } from './dto/IArmorDataDto';
import { IArmorSetDto } from './dto/IArmorSetDto';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  private url: string = "/data/";
  private _armorData!: Observable<IArmorDataDto>;

  constructor(private http: HttpClient) {

  }

  get armorData(): Observable<IArmorDataDto> {
    if (this._armorData == null) {
      this._armorData = this.http.get<IArmorDataDto>(this.url).pipe(map(x => {

        //the same object references.
        x.armorSets.forEach((s: IArmorSetDto) => {

          let head = x.head.find(p => p.armorSetId == s.armorSetId);
          if (head) s.armorPieces.push(head);
          let chest = x.chest.find(p => p.armorSetId == s.armorSetId);
          if (chest) s.armorPieces.push(chest);
          let gauntlets = x.gauntlets.find(p => p.armorSetId == s.armorSetId);
          if (gauntlets) s.armorPieces.push(gauntlets);
          let legs = x.legs.find(p => p.armorSetId == s.armorSetId);
          if (legs) s.armorPieces.push(legs);

        });

        return x;
      })).pipe(shareReplay(1));
      return this._armorData;
    } else {
      return this._armorData;
    }
  }
}
