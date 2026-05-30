import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// Models
import { DoctorLookupResponse, DoctorAppointmentResponse } from '../models/doctor.model';

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

  getAppointmentByDoctor(doctorId: number) {
    try {
      return this.http.get<DoctorAppointmentResponse[]>(`${this.doctorUrl}/${doctorId}/appointments`);
    } catch (error) {
      console.error('Không lấy được dữ liệu cuộc hẹn của bác sĩ:', error);
      throw error;
    }
  }

}
