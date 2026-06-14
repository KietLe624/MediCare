import { Component, Input, ChangeDetectorRef, inject, EventEmitter, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
// services
import { AppointmentService } from '../../services/appointment';
// models
import { AppointmentResponse, AppointmentStatus, UpdateAppointmentRequest } from '../../models/appointment.model';
import { EditAppointment } from "../edit-appointment/edit-appointment";

@Component({
  selector: 'app-appointment-detail',
  imports: [CommonModule, EditAppointment],
  templateUrl: './appointment-detail.html',
  styleUrl: './appointment-detail.scss',
})
export class AppointmentDetailComponent {
  private appointmentService = inject(AppointmentService);

  private cdr = inject(ChangeDetectorRef);

  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() statusChanged = new EventEmitter<{ id: number, status: string }>();

  isLoading = false;
  appointmentDetail: AppointmentResponse | null = null;
  private _appointmentId: number | null = null;

  @Input() set id(value: number | null | undefined) {
    this._appointmentId = value || null;
    if (this._appointmentId) {
      this.loadAppointmentDetail(this._appointmentId);
    }
  }

  get appointmentId(): number | null {
    return this._appointmentId;
  }

  loadAppointmentDetail(id: number) {
    this.isLoading = true;
    this.appointmentService.getAppointmentById(id).subscribe({
      next: (res) => {
        this.appointmentDetail = res;
        this.isLoading = false;
        this.cdr.markForCheck();
        console.log('Chi tiết cuộc hẹn đã được tải:', this.appointmentDetail);
      },
      error: (error) => {
        console.error('Lỗi khi tải chi tiết cuộc hẹn:', error);
        this.isLoading = false;
      }
    });
  }

  closeDrawerDetail() {
    this.appointmentDetail = null;
    this.close.emit();
  }

  hiddenDrawerDetail() {
    this.isOpen = false;
  }

  updateStatus(action: string) {
    if (!this.appointmentDetail) return;

    if (action === 'confirm') this.appointmentDetail.AppointmentStatus = AppointmentStatus.Confirmed;
    if (action === 'complete') this.appointmentDetail.AppointmentStatus = AppointmentStatus.Completed;
    if (action === 'cancel') this.appointmentDetail.AppointmentStatus = AppointmentStatus.Cancelled;
    if (action === 'no-show') this.appointmentDetail.AppointmentStatus = AppointmentStatus.NoShow;
    this.appointmentDetail.updatedAt = new Date();

    this.statusChanged.emit({ id: this.appointmentDetail.id, status: this.appointmentDetail.AppointmentStatus });
  }

  getStatusInfo(status: AppointmentStatus) {
    switch (status) {

      case AppointmentStatus.Confirmed:
        return { text: 'Đã xác nhận', class: 'bg-blue-50 text-blue-600', icon: 'check_circle' };
      case AppointmentStatus.Completed:
        return { text: 'Đã hoàn thành', class: 'bg-emerald-50 text-emerald-600', icon: 'done_all' };
      case AppointmentStatus.Cancelled:
        return { text: 'Đã hủy', class: 'bg-red-50 text-red-600', icon: 'cancel' };
      case AppointmentStatus.NoShow:
        return { text: 'Vắng mặt', class: 'bg-slate-100 text-slate-600', icon: 'person_off' };
      default:
        return { text: 'Chưa rõ', class: 'bg-slate-50 text-slate-500', icon: 'help' };
    }
  }

  // form edit appointment
  @Output() edit = new EventEmitter<number>();

  isEditFormOpen = signal(false);
  openEditForm() {
    this.isEditFormOpen.set(true);
    this.edit.emit(this.appointmentId!);
    this.hiddenDrawerDetail(); // tạm ẩn khi mở form edit
  }

  closeEditForm() {
    this.isEditFormOpen.set(false);
    this.close.emit();
  }

  onSubmitEditForm() {
    this.closeEditForm();
    if (this.appointmentId) {
      this.loadAppointmentDetail(this.appointmentId);
    }
  }
}
