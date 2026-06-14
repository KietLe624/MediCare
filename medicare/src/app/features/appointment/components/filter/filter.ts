import { Component, EventEmitter, inject, Output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
// models
import { AppointmentQueryParams, AppointmentStatus } from '../../models/appointment.model';
// services
import { AppointmentService } from '../../services/appointment';


@Component({
  selector: 'app-filter',
  imports: [ReactiveFormsModule],
  templateUrl: './filter.html',
  styleUrl: './filter.scss',
})
export class FilterComponent {
  private appointmentService = inject(AppointmentService);
  private fb = inject(FormBuilder);

  @Output() filterChange = new EventEmitter<any>();

  readonly statuses = Object.values(AppointmentStatus);
  form = this.fb.group({
    date: [''],
    doctorId: [''],
    status: ['all'],
    sortOrder: ['desc']
  });

  applyFilters() {
    const value = this.form.getRawValue();
    console.log('Filter emit:', value);
    this.filterChange.emit({

      date: value.date || undefined,
      doctorId: value.doctorId
        ? Number(value.doctorId)
        : undefined,
      status: value.status !== 'all' ? value.status : undefined,
      sortOrder: value.sortOrder
    });

  }

  resetFilters() {
    this.form.reset({
      status: 'all',
      sortOrder: 'desc'
    });
    this.applyFilters();
  }
  // status
  getStatusInfo(status: AppointmentStatus) {
    switch (status) {
      case AppointmentStatus.Confirmed:
        return { text: 'Đã xác nhận' };
      case AppointmentStatus.Completed:
        return { text: 'Đã hoàn thành' };
      case AppointmentStatus.Cancelled:
        return { text: 'Đã hủy' };
      case AppointmentStatus.NoShow:
        return { text: 'Không đến' };
      case AppointmentStatus.Rescheduled:
        return { text: 'Đã đặt lại' };
      default:
        return { text: status };
    }
  }
}
