import { Component, inject, Input, OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
// models
import { PatientResponse } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';
// components
import { PatientVisitComponent } from '../patient-visit/patient-visit';
import { PatientInvoiceComponent } from "../patient-invoice/patient-invoice";
import { PatientAppointmentComponent } from "../patient-appointment/patient-appointment";
import { PatientPrescriptionComponent } from '../patient-prescription/patient-prescription';

@Component({
  selector: 'app-patient-detail',
  imports: [CommonModule, PatientVisitComponent, PatientInvoiceComponent, PatientAppointmentComponent, PatientPrescriptionComponent],
  templateUrl: './patient-detail.html',
  styleUrl: './patient-detail.scss',
})
export class PatientDetailComponent implements OnInit {
  private patientService = inject(PatientService);
  private cdr = inject(ChangeDetectorRef);

  @Input() id?: number | string;
  patientData?: PatientResponse;

  activeTab: 'visits' | 'invoices' | 'appointments' | 'prescriptions' = 'visits';

  switchTab(tab: 'visits' | 'invoices' | 'appointments' | 'prescriptions') {
    this.activeTab = tab;
  }

  ngOnInit() {
    this.loadPatientData();
  }

  loadPatientData() {
    if (!this.id) {
      console.error('Patient ID is required to load patient data.', this.id);
      return;
    }
    const patientId = Number(this.id);
    this.patientService.getPatientById(patientId).subscribe({
      next: (res) => {
        this.patientData = res;
        console.log('Loaded patient data:', this.patientData);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading patient data:', error);
      }
    });
  }
}
