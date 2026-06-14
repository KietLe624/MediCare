import { ChangeDetectorRef, Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// Services
import { AppointmentService } from '../../services/appointment';
import { ToastService } from '../../../../core/shared/services/toast-info';
// Models
import { AppointmentResponse, UpdateAppointmentRequest } from '../../models/appointment.model';
import { AppointmentFormComponent } from "../../pages/appointment-form/appointment-form";

@Component({
  selector: 'app-edit-appointment',
  imports: [AppointmentFormComponent],
  templateUrl: './edit-appointment.html',
  styleUrl: './edit-appointment.scss',
})
export class EditAppointment {
  private appointmentService = inject(AppointmentService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  @Input() patientData?: { id: number; fullName: string };
  @Input() appointmentId!: number;

  @Output() save = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();

  editForm!: FormGroup;
  patientId!: number;
  private _appointmentData: any | null = null; // Ở dự án thật dùng AppointmentResponse

  @Input() set appointmentData(value: any | null) {
    this._appointmentData = value;

    // Nếu có data VÀ form đã được khởi tạo xong thì mới fill
    if (this._appointmentData && this.editForm) {
      this.fillFormData(this._appointmentData);
    }
  }

  get appointmentData() {
    return this._appointmentData;
  }

  ngOnInit() {

    this.editForm = this.fb.group({
      reason: ['', Validators.required],
      notes: [''],
    });

    if (this._appointmentData) {
      this.fillFormData(this._appointmentData);
    }
  }

  fillFormData(data: any) {
    this.editForm.patchValue({
      patientName: this.patientData?.fullName || '',
      reason: data.reason,
      notes: data.notes
    });
  }

  updateAppointment(payload: UpdateAppointmentRequest, appointmentId: number) {
    this.appointmentService.updateAppointment(appointmentId, payload).subscribe({
      next: () => {
        console.log('Cập nhật cuộc hẹn thành công');
        this.toast.success('Cập nhật', 'cuộc hẹn đã được cập nhật thành công');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Lỗi khi cập nhật cuộc hẹn:', err);
        this.toast.error('Cập nhật', 'Có lỗi xảy ra khi cập nhật cuộc hẹn');
        this.cdr.detectChanges();
      },
    });
  }
  closeEditForm() {
    this.close.emit();
  }
}
