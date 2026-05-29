import { CommonModule, DOCUMENT } from '@angular/common';
import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  Renderer2,
  inject,
} from '@angular/core';

// Models
import { AppointmentResponse, RescheduleAppointmentRequest } from '../../../appointment/models/appointment.model';

// Services
import { AppointmentService } from '../../../appointment/services/appointment';
import { ToastService } from '../../../../core/shared/services/toast-info';

@Component({
  selector: 'app-drawer-appointment',
  imports: [CommonModule],
  templateUrl: './drawer-appointment.html',
  styleUrl: './drawer-appointment.scss',
})
export class DrawerAppointment implements OnInit, OnDestroy {
  private document = inject(DOCUMENT);
  private renderer = inject(Renderer2);
  private toast = inject(ToastService);

  @Input() appointment: AppointmentResponse | null = null;

  @Output() closeDrawer = new EventEmitter<void>();
  @Output() appointmentUpdated = new EventEmitter<AppointmentResponse>();

  onClose() {
    this.closeDrawer.emit();
  }

  constructor(private appointmentService: AppointmentService) { }
  // NgOnInit and NgOnDestroy to manage scroll locking when the drawer is open
  ngOnInit() {
    this.renderer.addClass(this.document.documentElement, 'scroll-locked');
    this.renderer.addClass(this.document.body, 'scroll-locked');
  }

  ngOnDestroy() {
    this.renderer.removeClass(this.document.documentElement, 'scroll-locked');
    this.renderer.removeClass(this.document.body, 'scroll-locked');
  }

  confirmAppointment() {
    if (!this.appointment) return;
    this.appointmentService.confirmAppointment(this.appointment.id).subscribe({
      next: (response) => {
        this.toast.success('Thành công', 'Đã xác nhận Check-in cho bệnh nhân!');
        this.onClose(); // Đóng drawer sau khi xác nhận
        this.appointmentUpdated.emit(response); // Thông báo cho component cha về cuộc hẹn đã được cập nhật
      },
      error: (error) => {
        console.error('Lỗi khi xác nhận cuộc hẹn:', error);
        this.toast.error('Không thể xác nhận cuộc hẹn', 'Vui lòng thử lại sau.');
      }
    });
  }

  rescheduleAppointment() {
    if (!this.appointment) return;
    const request: RescheduleAppointmentRequest = {
      newDate: this.appointment.appointmentDate,
      newStartTime: this.appointment.startTime,
    };
    this.appointmentService.reScheduleAppointment(this.appointment.id, request).subscribe({
      next: (response) => {
        this.toast.success('Thành công', 'Đã đặt lại cuộc hẹn!');
        this.onClose(); // Đóng drawer sau khi đặt lại
        this.appointmentUpdated.emit(response); // Thông báo cho component cha về cuộc hẹn đã được cập nhật
      },
      error: (error) => {
        console.error('Lỗi khi đặt lại cuộc hẹn:', error);
        this.toast.error('Không thể đặt lại cuộc hẹn', 'Vui lòng thử lại sau.');
      }
    });
  }

}
