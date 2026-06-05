import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import {
  FormsModule,
  ReactiveFormsModule,
} from '@angular/forms';
// Services
import { AppointmentService } from '../../services/appointment';
// Models
import { AppointmentForm } from '../../pages/appointment-form/appointment-form';
import { ToastService } from '../../../../core/shared/services/toast-info';

@Component({
  selector: 'app-create-appointment',
  imports: [ReactiveFormsModule, FormsModule, CommonModule, AppointmentForm],
  templateUrl: './create-appointment.html',
  styleUrl: './create-appointment.scss',
})
export class CreateAppointmentComponent {

  @Input() patientData?: { id: number; fullName: string };
  @Output() saved = new EventEmitter<void>();
  @Output() closed = new EventEmitter<void>();

  patientId!: number;

  constructor(
    private appointmentService: AppointmentService,
    private cdr: ChangeDetectorRef,
    private toast: ToastService,
  ) { }

  onSubmit(payload: any) {
    this.appointmentService.createAppointment(payload).subscribe({
      next: () => {
        const bodyToSend = {
          ...payload,
          startTime: payload.startTime
            ? `${payload.startTime.substring(0, 5)}:00`
            : payload.startTime
        };

        console.log('Cục data đã thêm giây chuẩn bị gửi lên API:', bodyToSend);
        this.saved.emit();

        this.closed.emit();
        this.toast.success('Tạo mới', 'cuộc hẹn mới đã được tạo thành công');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Xảy ra lỗi', 'có lỗi xảy ra khi tạo mới cuộc hẹn');
      }
    });
  }

  onCloseAppointment() {
    this.closed.emit();
    console.log('closed create appointment form');
  }

}
