import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
// models
import { PatientPrescriptionResponse } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';


@Component({
  selector: 'app-patient-prescription',
  imports: [CommonModule],
  templateUrl: './patient-prescription.html',
  styleUrl: './patient-prescription.scss',
})
export class PatientPrescriptionComponent implements OnInit {

  @Input() id?: number | string;

  constructor(private patientService: PatientService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.loadPrescriptions();
  }

  prescriptions?: PatientPrescriptionResponse[];

  loadPrescriptions() {
    if (!this.id) {
      console.error('Patient ID is required to load prescriptions.', this.id);
      return;
    }
    const patientId = Number(this.id);
    this.patientService.getPatientPrescriptions(patientId, {}).subscribe({
      next: (res) => {
        console.log('Loaded prescriptions:', res);
        this.prescriptions = res.data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading prescriptions:', error);
      }
    });
  }
}
