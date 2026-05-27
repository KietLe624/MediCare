import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateAppointmentRequest } from '../models/appointment.model';
import { catchError, Observable, throwError } from 'rxjs';


@Injectable({
  providedIn: 'root',
})
export class AppointmentService {
  private http = inject(HttpClient);
  private appointmentUrl = 'http://localhost:5034/api/Appointment';

  constructor() { }

  getAppointmentById(id: number) {
    try {
      return this.http.get(`${this.appointmentUrl}/${id}`);
    } catch (error) {
      console.error('Không thể lấy thông tin cuộc hẹn:', error);
      throw error;
    }
  }

  createAppointment(request: CreateAppointmentRequest): Observable<any> {
    {
      return this.http.post(this.appointmentUrl, request).pipe(
        catchError((error) => {
          console.error('Lỗi khi tạo cuộc hẹn:', error);
          return throwError(() => new Error('Không thể tạo cuộc hẹn. Vui lòng thử lại sau.'));
        })
      );
    }
  }



}

