import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AppointmentToday, DoctorAppointmentByDate, OverviewResponse, PatientsByDepartment, RevenueByDate, RevenueByMonth, VisitByDate } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private dashboardUrl = 'http://localhost:5034/api/Dashboard';

  constructor() { }

  getOverview(): Observable<OverviewResponse> {
    try {
      return this.http.get<OverviewResponse>(`${this.dashboardUrl}/overview`);
    } catch (error) {
      console.error('Không lấy được dữ liệu tổng quan:', error);
      throw error;
    }
  }

  getAppointmentsToday(): Observable<AppointmentToday[]> {
    try {
      return this.http.get<AppointmentToday[]>(`${this.dashboardUrl}/appointments-today`);
    } catch (error) {
      console.error('Không lấy được dữ liệu cuộc hẹn hôm nay:', error);
      throw error;
    }
  }

  getVisitByDate(days: number): Observable<VisitByDate[]> {
    try {
      return this.http.get<VisitByDate[]>(`${this.dashboardUrl}/visits/by-date?days=${days}`);
    } catch (error) {
      console.error('Không lấy được dữ liệu thăm khám theo ngày:', error);
      throw error;
    }
  }

  getVisitByDepartment(): Observable<PatientsByDepartment[]> {
    try {
      return this.http.get<PatientsByDepartment[]>(`${this.dashboardUrl}/patients/by-department`);
    } catch (error) {
      console.error('Không lấy được dữ liệu thăm khám theo phòng ban:', error);
      throw error;
    }
  }
  getRevenueByDate(days: number = 7): Observable<RevenueByDate[]> {
    try {
      return this.http.get<RevenueByDate[]>(`${this.dashboardUrl}/revenue/by-date?days=${days}`);
    } catch (error) {
      console.error('Không lấy được dữ liệu doanh thu theo ngày:', error);
      throw error;
    }
  }

  getRevenueByMonth(months: number = 30): Observable<RevenueByMonth[]> {
    try {
      return this.http.get<RevenueByMonth[]>(`${this.dashboardUrl}/revenue/by-month?months=${months}`);
    } catch (error) {
      console.error('Không lấy được dữ liệu doanh thu theo tháng:', error);
      throw error;
    }
  }

  getDoctorAppointments(days: number = 7): Observable<DoctorAppointmentByDate[]> {
    try {
      return this.http.get<DoctorAppointmentByDate[]>(`${this.dashboardUrl}/doctor/appointments/by-date?days=${days}`);
    } catch (error) {
      console.error('Không lấy được dữ liệu cuộc hẹn theo bác sĩ:', error);
      throw error;
    }
  }

}
