import { ChangeDetectorRef, Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

// models
import { PatientAppointmentResponse } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';

@Component({
  selector: 'app-patient-appointment',
  imports: [CommonModule],
  templateUrl: './patient-appointment.html',
  styleUrl: './patient-appointment.scss',
})
export class PatientAppointmentComponent implements OnInit {
  private patientService = inject(PatientService);
  private cdr = inject(ChangeDetectorRef);

  @Input() id?: number | string;

  appointments?: PatientAppointmentResponse[];

  ngOnInit() {
    this.loadAppointments();
  }

  loadAppointments() {
    if (!this.id) {
      console.error('Patient ID is required to load appointments.', this.id);
      return;
    }
    const patientId = Number(this.id);
    this.patientService.getPatientAppointments(patientId, {}).subscribe({
      next: (res) => {
        console.log('Loaded appointments:', res);
        this.appointments = res.data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading appointments:', error);
      }
    });
  }


  // helper format status  // helper format status
  getStatusText(status: string): string {
    const normalized = status?.toLowerCase() || '';
    switch (normalized) {
      case 'scheduled': return 'Sắp tới';
      case 'confirmed': return 'Đã xác nhận';
      case 'completed': return 'Đã hoàn thành';
      case 'cancelled': return 'Đã hủy';
      default: return status || 'Chưa rõ';
    }
  }
}
