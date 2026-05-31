import { Component, inject, ChangeDetectorRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
// Services
import { DashboardService } from '../../services/dashboard';
// Models
import { DoctorAppointmentByDate } from '../../models/dashboard.model';
// Chart
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexPlotOptions,
  ApexXAxis,
  ApexYAxis,
  ApexTooltip,
  ChartComponent,
  NgApexchartsModule
} from 'ng-apexcharts';

export type DoctorBarChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;

  plotOptions: ApexPlotOptions;
  dataLabels: ApexDataLabels;

  xaxis: ApexXAxis;
  yaxis: ApexYAxis;

  tooltip: ApexTooltip;

  colors: string[];
};

@Component({
  selector: 'app-doctor-appointment-chart',
  imports: [CommonModule, NgApexchartsModule],
  templateUrl: './doctor-appointment-chart.html',
  styleUrl: './doctor-appointment-chart.scss',
})
export class DoctorAppointmentChart implements OnInit {
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);
  doctorStats: any[] = [];

  ngOnInit() {
    this.loadDoctorStats();
  }

  loadDoctorStats() {
    this.dashboardService
      .getDoctorAppointments(7)
      .subscribe(res => {

        const max =
          Math.max(...res.map(x => x.appointmentCount), 1);

        this.doctorStats = res.map((x, index) => ({
          ...x,

          percent: Math.round(
            (x.appointmentCount / max) * 100
          ),

          color:
            index === 0
              ? '#2563eb'
              : index === 1
                ? '#10b981'
                : index === 2
                  ? '#f59e0b'
                  : '#64748b'
        }));
      });
  }

}
