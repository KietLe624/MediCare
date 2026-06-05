import { Component, EventEmitter, inject, Output, Input, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CommonModule } from '@angular/common';

// models
import { PatientResponse } from '../../models/patient.model';

@Component({
  selector: 'app-patient-form',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './patient-form.html',
  styleUrl: './patient-form.scss',
})
export class PatientFormComponent {
  private fb = inject(FormBuilder);
  @Input() mode: 'create' | 'edit' = 'create';

  @Input() patient?: PatientResponse;
  @Input() isSubmitting = false;

  patientForm: FormGroup;

  @Output() submitForm = new EventEmitter<any>();
  @Output() closed = new EventEmitter<void>();

  constructor() {
    this.patientForm = this.fb.group({
      // -- Bắt buộc --
      lastName: ['', [Validators.required]],
      firstName: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required]],
      gender: ['', [Validators.required]],

      // -- Không bắt buộc --
      phoneNumber: [''],
      email: ['', [Validators.email]],
      address: [''],

      bloodType: [''],
      allergies: [''],

      emergencyContactName: [''],
      emergencyContactPhone: [''],

      patientType: ['Outpatient'],

      insuranceProvider: [''],
      insuranceNumber: [''],

      userId: [null],
    });
  }

  ngOnChanges() {
    if (this.patient) {
      this.patientForm.patchValue(this.patient);
    }
  }

  onSubmit(): void {
    if (this.patientForm.invalid) {
      this.patientForm.markAllAsTouched();
      return;
    }
    this.submitForm.emit(this.patientForm.getRawValue());
  }

  onClose() {
    this.closed.emit();
  }
}
