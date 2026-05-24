import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateAppointmentRequest } from '../models/appointment.model';


@Injectable({
  providedIn: 'root',
})
export class Appointment {
  private http = inject(HttpClient);
  private appointmentUrl = 'http://localhost:5034/api/Appointment';

  constructor() { }
  getApppointmentById(id: number) {
    try {
      return this.http.get(`${this.appointmentUrl}/${id}`);
    } catch (error) {
      console.error('Không thể lấy thông tin cuộc hẹn:', error);
      throw error;
    }
  }

  createAppointment(request: CreateAppointmentRequest) {
    try {
      return this.http.post(this.appointmentUrl, request);
    } catch (error) {
      console.error('Không thể tạo cuộc hẹn:', error);
      throw error;
    }
  }
}
