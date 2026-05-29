import { Component, ViewChild, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
// apexcharts
import { NgApexchartsModule } from 'ng-apexcharts';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ChartComponent,
} from 'ng-apexcharts';

// Services
import { DashboardService } from '../../services/dashboard';
// Models
import { VisitByDate } from '../../models/dashboard.model';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  stroke: ApexStroke;
  tooltip: ApexTooltip;
  dataLabels: ApexDataLabels;
  grid: ApexGrid;
  fill: ApexFill;
  colors: string[];
};

@Component({
  selector: 'app-visit-chart',
  imports: [NgApexchartsModule, CommonModule],
  templateUrl: './visit-chart.html',
  styleUrl: './visit-chart.scss',
})
export class VisitChartComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);
  private visitsByDate: VisitByDate[] = [];

  chartOptions: Partial<ChartOptions>;

  constructor() {
    this.chartOptions = {
      series: [
        {
          name: 'Lượt khám',
          data: this.visitsByDate.map((x) => x.count),
        },
      ],

      chart: {
        type: 'area',
        height: 320,
        toolbar: {
          show: false,
        },
        zoom: {
          enabled: false,
        },
      },

      colors: ['#004ac6'],

      stroke: {
        curve: 'smooth',
        width: 4,
      },

      fill: {
        type: 'gradient',
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.25,
          opacityTo: 0.02,
        },
      },

      dataLabels: {
        enabled: false,
      },

      grid: {
        borderColor: '#e2e8f0',
        strokeDashArray: 4,
      },

      xaxis: {
        categories: this.visitsByDate.map((x) =>
          this.formatXAxisLabel(x.date)
        ),
      },

      tooltip: {
        enabled: true,
      },
    };
  }

  ngOnInit() {
    this.loadVisitsChart();
  }

  selectedRange: number = 7; // Mặc định là 7 ngày gần nhất

  changeRange(range: number) {
    this.selectedRange = range;
    this.loadVisitsChart();
  }

  loadVisitsChart() {

    this.dashboardService
      .getVisitByDate(this.selectedRange)
      .subscribe(res => {

        const seriesData = res.map(x => x.count);

        const categories = res.map(x =>
          this.formatXAxisLabel(x.date)
        );

        this.chartOptions = {

          series: [
            {
              name: 'Lượt khám',
              data: seriesData
            }
          ],

          chart: {
            type: 'area',
            height: 320,
            toolbar: {
              show: false
            },
            zoom: {
              enabled: false
            }
          },

          colors: ['#004ac6'],

          stroke: {
            curve: 'smooth',
            width: 4
          },

          fill: {
            type: 'gradient',
            gradient: {
              shadeIntensity: 1,
              opacityFrom: 0.25,
              opacityTo: 0.02
            }
          },

          dataLabels: {
            enabled: false
          },

          grid: {
            borderColor: '#e2e8f0',
            strokeDashArray: 4
          },

          xaxis: {
            categories
          },

          tooltip: {
            enabled: true
          }
        };

        this.cdr.detectChanges();
      });
  }

  // helper
  private formatXAxisLabel(date: string): string {

    const parsedDate = new Date(date);

    // 7 ngày
    if (this.selectedRange === 7) {
      return new Intl.DateTimeFormat('vi-VN', {
        weekday: 'short'
      }).format(parsedDate);
    }

    // 30 ngày
    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit',
    }).format(parsedDate);
  }
}
