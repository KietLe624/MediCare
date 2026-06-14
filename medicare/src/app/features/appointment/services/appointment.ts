import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
// mdels
import {
  AppointmentResponse,
  CreateAppointmentRequest,
  PaginatedResponse,
  UpdateAppointmentRequest,
  RescheduleAppointmentRequest,
  AppointmentQueryParams,
  AppointmentSummaryResponse,
} from '../models/appointment.model';

@Injectable({
  providedIn: 'root',
})
export class AppointmentService {
  private http = inject(HttpClient);
  private appointmentUrl = 'http://localhost:5034/api/Appointment';

  getAppointments(params?: AppointmentQueryParams,): Observable<PaginatedResponse<AppointmentSummaryResponse>> {
    const httpParams = buildHttpParams(params);
    try {
      console.log('Gửi yêu cầu với params:', params);
      return this.http.get<PaginatedResponse<AppointmentSummaryResponse>>(
        this.appointmentUrl,
        { params: httpParams }
      );
    } catch (error) {
      console.error('Không lấy được thông tin cuộc hẹn:', error);
      throw error;
    }
  }

  getAppointmentById(id: number) {
    try {
      return this.http.get<AppointmentResponse>(`${this.appointmentUrl}/${id}`);
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
          return throwError(
            () => new Error('Không thể tạo cuộc hẹn. Vui lòng thử lại sau.'),
          );
        }),
      );
    }
  }

  updateAppointment(
    id: number,
    request: UpdateAppointmentRequest,
  ): Observable<any> {
    return this.http.put(`${this.appointmentUrl}/${id}`, request).pipe(
      catchError((error) => {
        console.error('Lỗi khi cập nhật cuộc hẹn:', error);
        return throwError(
          () => new Error('Không thể cập nhật cuộc hẹn. Vui lòng thử lại sau.'),
        );
      }),
    );
  }

  confirmAppointment(id: number): Observable<any> {
    return this.http.patch(`${this.appointmentUrl}/${id}/confirm`, {}).pipe(
      catchError((error) => {
        console.error('Lỗi khi xác nhận cuộc hẹn:', error);
        return throwError(
          () => new Error('Không thể xác nhận cuộc hẹn. Vui lòng thử lại sau.'),
        );
      }),
    );
  }

  completeAppointment(id: number): Observable<any> {
    return this.http.patch(`${this.appointmentUrl}/${id}/complete`, {}).pipe(
      catchError((error) => {
        console.error('Lỗi khi hoàn thành cuộc hẹn:', error);
        return throwError(
          () => new Error('Không thể hoàn thành cuộc hẹn. Vui lòng thử lại sau.'),
        );
      }),
    );
  }

  noShowAppointment(id: number): Observable<any> {
    return this.http.patch(`${this.appointmentUrl}/${id}/no-show`, {}).pipe(
      catchError((error) => {
        console.error('Lỗi khi đánh dấu vắng mặt cuộc hẹn:', error);
        return throwError(
          () =>
            new Error('Không thể đánh dấu vắng mặt cuộc hẹn. Vui lòng thử lại sau.'),
        );
      }),
    );
  }

  cancelAppointment(id: number, reason?: string): Observable<any> {
    return this.http.patch(`${this.appointmentUrl}/${id}/cancel`, { reason }).pipe(
      catchError((error) => {
        console.error('Lỗi khi hủy cuộc hẹn:', error);
        return throwError(
          () => new Error('Không thể hủy cuộc hẹn. Vui lòng thử lại sau.'),
        );
      }),
    );
  }

  reScheduleAppointment(
    id: number,
    request: RescheduleAppointmentRequest,
  ): Observable<any> {
    return this.http
      .patch(`${this.appointmentUrl}/${id}/reschedule`, request)
      .pipe(
        catchError((error) => {
          console.error('Lỗi khi đặt lại cuộc hẹn:', error);
          return throwError(
            () =>
              new Error('Không thể đặt lại cuộc hẹn. Vui lòng thử lại sau.'),
          );
        }),
      );
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
