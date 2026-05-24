import { Component, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { CreateAppointmentRequest } from '../../models/appointment.model';
import { Appointment } from '../../services/appointment';

@Component({
  selector: 'app-appointment-create',
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './appointment-create.html',
  styleUrl: './appointment-create.scss',
})
export class AppointmentCreate {
  private fb = inject(FormBuilder);

  @Output() close = new EventEmitter<void>();
  @Output() submitForm = new EventEmitter<any>();

  appointmentForm: FormGroup = this.fb.group({
    patientId: [null, Validators.required],
    doctorId: [null, Validators.required],
    appointmentDate: ['', Validators.required],
    startTime: ['', Validators.required],
    endTime: ['', Validators.required],
    reason: ['', Validators.required],
    notes: ['']
  });

  onSubmit() {
    if (this.appointmentForm.valid) {
      const payload = {
        ...this.appointmentForm.value,
        patientId: Number(this.appointmentForm.value.patientId),
        doctorId: Number(this.appointmentForm.value.doctorId),
      }
      console.log('Form data:', payload);
      this.submitForm.emit(payload);
    } else {
      console.log('Form is invalid');
    }

  }
  onDiscard() {
    this.close.emit();
  }

}
