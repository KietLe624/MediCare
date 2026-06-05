import { Component, inject, OnInit } from '@angular/core';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexChart,
  ApexNonAxisChartSeries,
  ApexResponsive,
  ApexLegend
} from 'ng-apexcharts';
// Services
import { DashboardService } from '../../services/dashboard';
// Models
import { PatientsByDepartment } from '../../models/dashboard.model';

export type DonutChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: string[];
  responsive: ApexResponsive[];
  colors: string[];
  legend: ApexLegend;
};

@Component({
  selector: 'app-department-chart',
  imports: [NgApexchartsModule],
  templateUrl: './department-chart.html',
  styleUrl: './department-chart.scss',
})

export class DepartmentChartComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  patientsByDepartment: PatientsByDepartment[] = [];

  ngOnInit() {
    this.loadDepartmentChart();
  }
  chartOptions: Partial<DonutChartOptions> = {
    series: [],
    chart: {
      type: 'donut',
      height: 320
    },
    labels: [],
    colors: [
      '#2563eb',
      '#10b981',
      '#f59e0b',
      '#8b5cf6',
      '#ef4444'
    ],
    legend: {
      position: 'bottom'
    },
    responsive: [
      {
        breakpoint: 480,
        options: {
          chart: {
            width: 280
          }
        }
      }
    ]
  };

  loadDepartmentChart() {
    this.dashboardService
      .getVisitByDepartment()
      .subscribe(res => {
        this.chartOptions = {
          ...this.chartOptions,
          series: res.map(x => x.count),
          labels: res.map(x => x.departmentName)
        };
      });
  }

}
