import { Component, inject, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  FormGroup,
  FormControl,
} from '@angular/forms';
import { debounceTime, switchMap, distinctUntilChanged, of } from 'rxjs';
// Services
import { Appointment } from '../../services/appointment';
import { PatientService } from '../../../patient/services/patient';
// Models
import { CreateAppointmentRequest } from '../../models/appointment.model';
import {
  PatientSummaryResponse,
  PagedResponse,
  PatientLookupResponse,
} from '../../../patient/models/patient.model';
import { DoctorLookupResponse } from '../../../doctor/models/doctor.model';
import { DoctorService } from '../../../doctor/services/doctor';

@Component({
  selector: 'app-appointment-create',
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './appointment-create.html',
  styleUrl: './appointment-create.scss',
})
export class AppointmentCreate implements OnInit {
  private fb = inject(FormBuilder);
  private appointmentService = inject(Appointment);
  private patientService = inject(PatientService);
  private doctorService = inject(DoctorService);

  @Output() close = new EventEmitter<void>();
  @Output() submitForm = new EventEmitter<any>();

  appointmentForm: FormGroup = this.fb.group({
    patientId: [null, Validators.required],
    doctorId: [null, Validators.required],
    appointmentDate: ['', Validators.required],
    startTime: ['', Validators.required],
    endTime: ['', Validators.required],
    reason: ['', Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    this.searchPatient.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        switchMap((value) => {
          console.log('SEARCH:', value);

          if (!value || value.length < 2) {
            return of([]);
          }

          return this.patientService.searchPatients(value);
        }),
      )
      .subscribe((res) => {
        console.log(res);

        this.patients = res;
      });

    this.searchDoctor.valueChanges
      .pipe(
        debounceTime(400),

        distinctUntilChanged(),

        switchMap(value => {

          if (!value || value.length < 2) {
            return of([]);
          }

          return this.doctorService
            .searchDoctors(value);
        })
      )
      .subscribe(res => {

        this.doctors = res;
      });
  }

  closeForm() {
    this.close.emit();
  }

  onSubmit() {
    if (this.appointmentForm.valid) {
      const payload = {
        ...this.appointmentForm.value,
        patientId: Number(this.appointmentForm.value.patientId),
        doctorId: Number(this.appointmentForm.value.doctorId),
      };
      console.log('Form data:', payload);
      this.submitForm.emit(payload);
    } else {
      console.log('Form is invalid');
    }
  }

  // PATIENT
  searchPatient = new FormControl('');
  patients: PatientLookupResponse[] = [];
  selectedPatient?: PatientLookupResponse;
  selectPatient(patient: PatientLookupResponse) {
    this.selectedPatient = patient;

    this.appointmentForm.patchValue({
      patientId: patient.id,
    });

    this.searchPatient.setValue(patient.fullName, { emitEvent: false });

    this.patients = [];
  }
  // DOCTOR
  searchDoctor = new FormControl('');
  doctors: DoctorLookupResponse[] = [];
  selectedDoctor?: DoctorLookupResponse;
  selectDoctor(doctor: DoctorLookupResponse) {

    this.selectedDoctor = doctor;

    this.appointmentForm.patchValue({
      doctorId: doctor.id
    });

    this.searchDoctor.setValue(
      doctor.fullName,
      { emitEvent: false }
    );

    this.doctors = [];
  }
}
