import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

import { IArmorDataDto } from './dto/IArmorDataDto';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  url: string = "/data/";

  constructor(private http: HttpClient) { }

  getArmorData(): Observable<IArmorDataDto> {
    return this.http.get<IArmorDataDto>(this.url);
  }
}
