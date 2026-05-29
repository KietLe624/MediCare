import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
// apexcharts
import { ApexAxisChartSeries, ApexChart, ApexDataLabels, ApexFill, ApexGrid, ApexStroke, ApexTooltip, ApexXAxis, ApexYAxis, NgApexchartsModule } from 'ng-apexcharts';

// Services
import { DashboardService } from '../../services/dashboard';
// Models
import { RevenueByDate, RevenueByMonth } from '../../models/dashboard.model';

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  stroke: ApexStroke;
  tooltip: ApexTooltip;
  dataLabels: ApexDataLabels;
  grid: ApexGrid;
  fill: ApexFill;
  colors: string[];
};

@Component({
  selector: 'app-revenue-chart',
  imports: [NgApexchartsModule, CommonModule],
  templateUrl: './revenue-chart.html',
  styleUrl: './revenue-chart.scss',
})
export class RevenueChartComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private revenueByDate: RevenueByDate[] = []
  private revenueByMonth: RevenueByMonth[] = []

  chartOptions: Partial<ChartOptions> = {
    series: [],

    chart: {
      type: 'line',
      height: 320,
      toolbar: {
        show: false
      },
      zoom: {
        enabled: false
      }
    },

    colors: ['#10b981'],

    stroke: {
      curve: 'smooth',
      width: 4
    },

    fill: {
      type: 'gradient',
      gradient: {
        opacityFrom: 0.25,
        opacityTo: 0
      }
    },

    dataLabels: {
      enabled: false
    },

    grid: {
      borderColor: '#e2e8f0',
      strokeDashArray: 4
    },

    tooltip: {
      enabled: true,

      y: {
        formatter(value) {

          return new Intl.NumberFormat(
            'vi-VN',
            {
              style: 'currency',
              currency: 'VND'
            }
          ).format(value);
        }
      }
    },

    yaxis: {
      labels: {
        formatter(value) {

          return `${(value / 1000).toFixed(0)}k`;
        }
      }
    },

    xaxis: {
      categories: [],

      labels: {
        hideOverlappingLabels: true
      }
    }
  };

  selectedRange: number = 1;
  changeRange(range: number) {
    this.selectedRange = range;
    this.loadRevenueByDate();
  }
  
  constructor() {
    this.loadRevenueByDate();
    // this.loadRevenueByMonth();
  }

  ngOnInit() {
    this.loadRevenueByDate();
  }

  loadRevenueByDate() {

    this.dashboardService
      .getRevenueByDate(this.selectedRange)
      .subscribe(res => {

        this.chartOptions = {

          ...this.chartOptions,

          series: [
            {
              name: 'Doanh thu',
              data: res.map(x => x.revenue)
            }
          ],

          xaxis: {
            ...this.chartOptions.xaxis,

            categories: res.map(x =>
              this.formatXAxisLabel(x.date)
            )
          }
        };
      });
  }


  private formatXAxisLabel(
    date: string
  ): string {

    const parsedDate = new Date(date);

    if (this.selectedRange === 1) {

      return new Intl.DateTimeFormat(
        'vi-VN',
        {
          weekday: 'short'
        }
      ).format(parsedDate);
    }

    if (this.selectedRange === 30) {

      return new Intl.DateTimeFormat(
        'vi-VN',
        {
          day: '2-digit'
        }
      ).format(parsedDate);
    }

    return new Intl.DateTimeFormat(
      'vi-VN',
      {
        month: 'short'
      }
    ).format(parsedDate);
  }

}
