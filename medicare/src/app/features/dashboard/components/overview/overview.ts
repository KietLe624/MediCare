import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject, signal } from '@angular/core';
// services
import { DashboardService } from '../../services/dashboard';
import { AppointmentService } from '../../../appointment/services/appointment';
// Models
import { OverviewResponse, AppointmentToday } from '../../models/dashboard.model';
// Helper
import { AppointmentStatusHelper, StatusConfig } from '../../../../core/shared/helper/appointment-status.hepler';
// Chart
import { NgApexchartsModule } from 'ng-apexcharts';


@Component({
  selector: 'app-overview',
  imports: [CommonModule, NgApexchartsModule],
  templateUrl: './overview.html',
  styleUrl: './overview.scss',
})
export class OverviewComponent {
  // SERVICES
  private dashboardService = inject(DashboardService);
  // OTHERS
  private changeDetector = inject(ChangeDetectorRef);

  stats: any[] = [];
  appointments: any[] = [];
  isFilterOpen: boolean = false;

  constructor() {
    this.dashboardService.getOverview().subscribe({
      next: (data) => {
        this.stats = this.buildStats(data);
        this.changeDetector.markForCheck();
      },
      error: (error) => {
        console.error('Lỗi không lấy được dữ liệu:', error);
      },
    });

    this.dashboardService.getAppointmentsToday().subscribe({
      next: (items) => {
        this.appointments = items.map((item) => this.mapAppointment(item));
        this.thisAppoitntment.set(this.appointments);
        this.changeDetector.markForCheck();
      },
      error: (error) => {
        console.error('Lỗi không lấy được lịch hẹn hôm nay  :', error);
      },
    });
  }


  // filter
  thisAppoitntment = signal<any[]>([]);
  selectedStatus = signal<string>('All');

  // method filler
  fillerAppointmentByStatus(status: string) {
    this.selectedStatus.set(status);
    if (!status || status === 'All') {
      this.thisAppoitntment.set(this.appointments);
      return;
    }
    this.thisAppoitntment.set(
      this.appointments.filter(
        (item) => item.status?.toLowerCase() === status.toLowerCase()
      )
    );
  }


  private buildStats(data: OverviewResponse) {
    return [
      {
        title: 'Tổng bệnh nhân',
        value: this.formatNumber(data.totalPatients),
        note: `${this.formatNumber(data.newPatientsToday)} mới hôm nay`,
        accent: 'tone-primary',
        chip: 'UPTREND',
        icon: 'patients',
      },
      {
        title: 'Tổng bác sĩ',
        value: this.formatNumber(data.doctorsAvailable),
        note: 'Số bác sĩ đang làm việc',
        accent: 'tone-secondary',
        chip: 'STEADY',
        icon: 'doctors',
      },
      {
        title: "Lịch hẹn hôm nay",
        value: this.formatNumber(data.appointmentsToday),
        note: `${this.formatNumber(data.upcomingAppointments)} sắp tới`,
        accent: 'tone-tertiary',
        chip: 'TODAY',
        icon: 'appointments',
      },
      {
        title: 'Số lượt khám hôm nay',
        value: this.formatNumber(data.completedToday),
        note: `Revenue MTD:`,
        accent: 'tone-primary',
        chip: 'PEAK',
        icon: 'visits',

      },
    ];
  }

  // Charts

  // OPEN APPOINTMENT FORM
  isFormOpen = signal(false);

  openForm() {
    this.isFormOpen.set(true);
  }

  closeForm() {
    this.isFormOpen.set(false);
  }

  // HELPERS
  private formatNumber(value: number): string {
    return new Intl.NumberFormat('en-US').format(value ?? 0);
  }

  private mapAppointment(item: AppointmentToday) {
    return {
      id: item.id,
      initials: this.getInitials(item.patientName),
      name: item.patientName,
      department: item.departmentName,
      doctor: item.doctorName,
      time: this.formatTimeRange(item.startTime, item.endTime),
      status: item.status,
      tone: this.statusTone(item.status),
    };
  }

  private getInitials(name: string): string {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join('');
  }

  private formatTimeRange(start: string, end: string): string {
    const startLabel = this.formatTime(start);
    const endLabel = this.formatTime(end);
    return `${startLabel} - ${endLabel}`;
  }

  private formatTime(value: string): string {
    if (!value) {
      return '';
    }
    const parts = value.split(':');
    const hours = Number(parts[0] ?? 0);
    const minutes = Number(parts[1] ?? 0);
    const period = hours >= 12 ? 'PM' : 'AM';
    const normalized = hours % 12 || 12;
    return `${normalized}:${minutes.toString().padStart(2, '0')} ${period}`;
  }

  private statusTone(status: string): string {
    const normalized = status.toLowerCase();
    if (normalized.includes('Completed') || normalized.includes('Complete')) {
      return 'badge-success';
    }
    if (normalized.includes('Confirmed') || normalized.includes('Confirm')) {
      return 'badge-warning';
    }
    if (normalized.includes('Scheduled') || normalized.includes('Schedule')) {
      return 'badge-warning';
    }
    if (normalized.includes('Cancelled') || normalized.includes('reject')) {
      return 'badge-danger';
    }
    if (normalized.includes('urgent') || normalized.includes('high')) {
      return 'badge-warning';
    }
    return 'badge-neutral';
  }
  // helper: badge status
  getStatusConfig(status: string): StatusConfig {
    return AppointmentStatusHelper.getConfig(status);
  }

}
