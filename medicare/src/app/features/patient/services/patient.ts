import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import {
  PatientQueryParams,
  PagedResponse,
  PatientSummaryResponse,
  PatientLookupResponse
} from '../models/patient.model';

@Injectable({
  providedIn: 'root',
})
export class PatientService {

  private http = inject(HttpClient);

  private patientUrl = 'http://localhost:5034/api/Patient';

  searchPatients(keyword: string) {
    return this.http.get<PatientLookupResponse[]>(
      `${this.patientUrl}/search`,
      {
        params: {
          keyword
        }
      }
    );
  }
  
  getPatients(params: PatientQueryParams) {
    return this.http.get<PagedResponse<PatientSummaryResponse>>(
      this.patientUrl,
      {
        params: buildHttpParams(params)
      }
    );
  }

  getPatientById(id: number) {
    return this.http.get(`${this.patientUrl}/${id}`);
  }
}

export function buildHttpParams(params: any): HttpParams {

  let httpParams = new HttpParams();

  Object.keys(params).forEach(key => {

    const value = params[key];

    if (
      value !== null &&
      value !== undefined &&
      value !== ''
    ) {
      httpParams = httpParams.set(key, value);
    }
  });

  return httpParams;
}
