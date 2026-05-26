import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AppointmentTodayDto, OverviewResponse } from '../models/dashboard.model';

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
      console.error('Error fetching dashboard overview:', error);
      throw error;
    }
  }

  getAppointmentsToday(): Observable<AppointmentTodayDto[]> {
    try {
      return this.http.get<AppointmentTodayDto[]>(`${this.dashboardUrl}/appointments-today`);
    } catch (error) {
      console.error('Error fetching today\'s appointments:', error);
      throw error;
    }
  }
}
