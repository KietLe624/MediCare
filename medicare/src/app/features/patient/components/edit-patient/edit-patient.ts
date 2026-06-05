import { ChangeDetectorRef, Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
// models
import {
  PatientResponse,
  UpdatePatientRequest,
} from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';
import { PatientFormComponent } from "../../pages/patient-form/patient-form";
import { ToastService } from "../../../../core/shared/services/toast-info";

@Component({
  selector: 'app-edit-patient',
  imports: [PatientFormComponent],
  templateUrl: './edit-patient.html',
  styleUrl: './edit-patient.scss',
})
export class EditPatientComponent implements OnInit {
  private patientService = inject(PatientService);
  private cdr = inject(ChangeDetectorRef)
  private toast = inject(ToastService);

  @Input()
  patientId!: number;

  @Output()
  saved = new EventEmitter<void>();

  @Output()
  closed = new EventEmitter<void>();

  patient?: PatientResponse;

  ngOnInit() {
    this.loadPatient();
  }

  loadPatient() {
    this.patientService.getPatientById(this.patientId).subscribe((res) => {
      this.patient = res;
      this.cdr.detectChanges();
    });
  }

  updatePatient(payload: UpdatePatientRequest) {
    this.patientService.updatePatientById(this.patientId, payload).subscribe({
      next: () => {
        this.saved.emit();
        this.closed.emit();
        this.toast.success('Cập nhật', 'thông tin bệnh nhân thành công');
      },
      error: (err) => {
        console.error(err);
        this.toast.error('Cập nhật', 'có lỗi xảy ra khi cập nhật thông tin bệnh nhân');
      },
    });
  }
  onFormClose() {
    this.closed.emit();
  }
}
