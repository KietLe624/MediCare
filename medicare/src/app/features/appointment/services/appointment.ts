import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateAppointmentRequest, RescheduleAppointmentRequest } from '../models/appointment.model';
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

  updateAppointment(id: number, request: CreateAppointmentRequest): Observable<any> {
    return this.http.put(`${this.appointmentUrl}/${id}`, request).pipe(
      catchError((error) => {
        console.error('Lỗi khi cập nhật cuộc hẹn:', error);
        return throwError(() => new Error('Không thể cập nhật cuộc hẹn. Vui lòng thử lại sau.'));
      })
    );
  }

  confirmAppointment(id: number): Observable<any> {
    return this.http.patch(`${this.appointmentUrl}/${id}/confirm`, {}).pipe(
      catchError((error) => {
        console.error('Lỗi khi xác nhận cuộc hẹn:', error);
        return throwError(() => new Error('Không thể xác nhận cuộc hẹn. Vui lòng thử lại sau.'));
      })
    );
  }

  reScheduleAppointment(id: number, request: RescheduleAppointmentRequest): Observable<any> {
    return this.http.patch(`${this.appointmentUrl}/${id}/reschedule`, request).pipe(
      catchError((error) => {
        console.error('Lỗi khi đặt lại cuộc hẹn:', error);
        return throwError(() => new Error('Không thể đặt lại cuộc hẹn. Vui lòng thử lại sau.'));
      })
    );
  }

}

