import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { VisitResponse } from '../models/visit.model';
// models


@Injectable({
  providedIn: 'root',
})
export class VisitService {

  private visitUrl = 'http://localhost:5034/api/Visit';
  constructor(private http: HttpClient) { }

  getVisitById(id: number) {
    try {
      return this.http.get<VisitResponse>(`${this.visitUrl}/${id}`);
    }
    catch (error) {
      console.error('Không lấy được thông tin lần khám:', error);
      throw error;
    }
  }
}
