import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from "@angular/forms";
// apexcharts
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  NgApexchartsModule,
} from 'ng-apexcharts';

// Services
import { DashboardService } from '../../services/dashboard';
// Models

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

type RevenueMode = '7d' | '30d' | 'year';

@Component({
  selector: 'app-revenue-chart',
  imports: [NgApexchartsModule, CommonModule, FormsModule],
  templateUrl: './revenue-chart.html',
  styleUrl: './revenue-chart.scss',
})
export class RevenueChartComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit() {
    this.loadRevenueChart();
  }

  chartOptions: Partial<ChartOptions> = {
    series: [],
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
    colors: ['#10b981'],
    stroke: {
      curve: 'smooth',
      width: 4,
    },
    fill: {
      type: 'gradient',
      gradient: {
        opacityFrom: 0.25,
        opacityTo: 0,
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
      categories: [],
    },
    tooltip: {
      enabled: true,
      y: {
        formatter(value) {
          return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
          }).format(value);
        },
      },
    },
  };

  selectedMode: RevenueMode = '7d';
  selectedYear = new Date().getFullYear();

  changeMode(mode: RevenueMode) {
    this.selectedMode = mode;
    this.loadRevenueChart();
  }

  changeYear(year: number) {
    this.selectedYear = year;

    if (this.selectedMode === 'year') {
      this.loadRevenueChart();
    }
  }

  loadRevenueChart() {
    if (this.selectedMode === 'year') {
      this.dashboardService
        .getRevenueByMonth(this.selectedYear)
        .subscribe((res) => {
          this.chartOptions = {
            ...this.chartOptions,
            chart: {
              ...this.chartOptions.chart,
              type: 'bar',
            },
            stroke: {
              curve: 'straight',
              width: 0,
            },
            fill: {
              type: 'solid',
              opacity: 1,
              colors: ['#3b82f6']
            },

            series: [
              {
                name: 'Doanh thu',
                data: res.map((x) => x.revenue),
              },
            ],
            xaxis: {
              categories: res.map((x) => `T${x.month}`),
            },
          };
          this.cdr.detectChanges();
        });
      return;
    }

    const days = this.selectedMode === '7d' ? 7 : 30;

    this.dashboardService.getRevenueByDate(days).subscribe((res) => {
      this.chartOptions = {
        ...this.chartOptions,
        // loại chart
        chart: {
          ...this.chartOptions.chart,
          type: 'bar',
        },
        // loại đường nét
        stroke: {
          curve: 'straight',
          width: 0,
        },
        // màu sắc
        fill: {
          type: 'solid',
          opacity: 1,
          colors: ['#3b82f6']
        },
        // dữ liệu
        series: [
          {
            name: 'Doanh thu',
            data: res.map((x) => x.revenue),
          },
        ],

        xaxis: {
          categories: res.map((x) => this.formatDateLabel(x.date)),
        },
      };
      this.cdr.detectChanges();
    });

  }

  private formatDateLabel(date: string): string {
    const d = new Date(date);

    if (this.selectedMode === '7d') {
      return new Intl.DateTimeFormat('vi-VN', {
        weekday: 'short',
      }).format(d);
    }

    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit',
    }).format(d);
  }
}
