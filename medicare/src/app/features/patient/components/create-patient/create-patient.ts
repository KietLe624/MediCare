import { Component, EventEmitter, Output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
// models
import { CreatePatientRequest } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';
import { PatientFormComponent } from '../../pages/patient-form/patient-form';
import { ToastService } from '../../../../core/shared/services/toast-info';

@Component({
  selector: 'app-create-patient',
  imports: [CommonModule, ReactiveFormsModule, PatientFormComponent],
  templateUrl: './create-patient.html',
  styleUrl: './create-patient.scss',
})
export class CreatePatientComponent {
  @Output() saved = new EventEmitter<void>();

  @Output() closed = new EventEmitter<void>();

  constructor(
    private patientService: PatientService,
    private toast: ToastService,
  ) { }

  onSubmit(payload: CreatePatientRequest) {
    this.patientService.createPatient(payload).subscribe({
      next: () => {
        this.saved.emit();
        this.closed.emit();
        this.toast.success('Tạo mới', 'bệnh nhân mới đã được tạo thành công');
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Tạo mới', 'có lỗi xảy ra khi tạo mới bệnh nhân');
      },
    });
  }
  onFormClose() {
    this.closed.emit();
  }
}
