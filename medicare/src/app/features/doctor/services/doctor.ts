import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// Models
import { DoctorLookupResponse } from '../models/doctor.model';

@Injectable({
  providedIn: 'root',
})
export class DoctorService {
  private doctorUrl = 'http://localhost:5034/api/Doctor';
  private http = inject(HttpClient);

  searchDoctors(keyword: string) {
    return this.http.get<DoctorLookupResponse[]>(
      `${this.doctorUrl}/search`,
      {
        params: {
          keyword
        }
      }
    );
  }

}
