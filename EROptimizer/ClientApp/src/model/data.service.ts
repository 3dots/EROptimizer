import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

import { ArmorDataDto } from './dto/ArmorDataDto';

@Injectable({
  providedIn: 'root'
})
export class DataService {

  url: string = "/data/";

  constructor(private http: HttpClient) { }

  getArmorData(): Observable<ArmorDataDto> {
    return this.http.get<ArmorDataDto>(this.url);
  }
}
