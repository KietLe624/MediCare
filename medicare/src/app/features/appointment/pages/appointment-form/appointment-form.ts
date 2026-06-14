import {
  Component,
  Output,
  EventEmitter,
  Input,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import {
  FormsModule,
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  FormGroup,
  FormControl,
} from '@angular/forms';
import {
  debounceTime,
  switchMap,
  distinctUntilChanged,
  of,
  forkJoin,
} from 'rxjs';
// Services
import { PatientService } from '../../../patient/services/patient';
import { DoctorService } from '../../../doctor/services/doctor';
import { DoctorScheduleService } from '../../../doctor/services/doctor-schedule';
// Models
import { PatientLookupResponse } from '../../../patient/models/patient.model';
import {
  DoctorLookupResponse,
  TimeSlot,
} from '../../../doctor/models/doctor.model';

@Component({
  selector: 'app-appointment-form',
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './appointment-form.html',
  styleUrl: './appointment-form.scss',
})
export class AppointmentFormComponent {
  private patientService = inject(PatientService);
  private doctorService = inject(DoctorService)
  private scheduleService = inject(DoctorScheduleService);
  private cdr = inject(ChangeDetectorRef);
  private fb = inject(FormBuilder);

  @Input() mode: 'create' | 'edit' = 'create';
  @Input() patientId!: number;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<any>();

  appointmentForm: FormGroup;

  constructor() {
    this.appointmentForm = this.fb.group({
      patientId: [null, Validators.required],
      doctorId: [null, Validators.required],
      appointmentDate: ['', Validators.required],
      startTime: ['', Validators.required],
      reason: ['', Validators.required],
      notes: [''],
    });
  }

  isSubmitting = false;

  ngOnInit(): void {
    this.searchPatient.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        switchMap((value) => {
          if (!value || value.length < 2) {
            return of([]);
          }
          return this.patientService.searchPatients(value);
        }),
      )
      .subscribe((res) => {
        this.patients = res;
      });

    this.searchDoctor.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        switchMap((value) => {
          if (!value || value.length < 2) {
            return of([]);
          }
          return this.doctorService.searchDoctors(value);
        }),
      )
      .subscribe((res) => {
        this.doctors = res;
      });
  }

  // DOCTOR SCHEDULE
  selectedDoctorId: number | null = null;
  selectedDate: string | null = null;

  morningSlots: TimeSlot[] = [];
  afternoonSlots: TimeSlot[] = [];
  isLoadingSlots = false;
  selectedTime: string | null = null;

  onDateOrDoctorChange() {
    if (this.selectedDoctorId && this.selectedDate) {
      this.isLoadingSlots = true;
      this.selectedTime = null;

      const targetDayOfWeek = this.scheduleService.getDayOfWeekFromDate(
        this.selectedDate,
      );

      // GỌI SONG SONG: Lịch trực gốc & Danh sách lịch hẹn thực tế trong ngày
      forkJoin({
        masterSchedules: this.scheduleService.getDoctorSchedules(
          this.selectedDoctorId,
        ),
        appointments: this.scheduleService.getDoctorAppointments(
          this.selectedDoctorId,
          this.selectedDate,
        ), // API mới cập nhật
      }).subscribe({
        // Trong hàm forkJoin ở Component của bạn:
        next: ({ masterSchedules, appointments }) => {
          // appointments lúc này chắc chắn là mảng Array(5) nhờ hàm map ở Service

          const schedulesForDay = masterSchedules.filter(
            (s) => s.dayOfWeek === targetDayOfWeek,
          );
          const generatedSlots = this.scheduleService.generateTimeSlots(
            schedulesForDay,
            30,
          );

          generatedSlots.forEach((slot) => {
            const isSlotBooked = appointments.data?.some((app) => {
              const appTimeShort = app.startTime.substring(0, 5); // Cắt "09:00:00" -> "09:00"

              // Khóa giờ nếu trùng thời gian và trạng thái KHÔNG PHẢI Cancelled
              return appTimeShort === slot.time && app.status !== 'Cancelled';
            });

            if (isSlotBooked) {
              slot.isAvailable = false;
            }
          });

          // Lọc lấy giờ trống đưa lên UI
          this.morningSlots = generatedSlots.filter(
            (s) => s.period === 'AM' && s.isAvailable,
          );
          this.afternoonSlots = generatedSlots.filter(
            (s) => s.period === 'PM' && s.isAvailable,
          );
          this.isLoadingSlots = false;
          this.cdr.detectChanges(); // Cập nhật UI ngay sau khi có dữ liệu mới
        },
        error: (err) => {
          console.error('Lỗi khi tải dữ liệu lịch của bác sĩ', err);
          this.isLoadingSlots = false;
        },
      });
    }
  }

  onDateChange(event: any) {
    const dateValue = event.target.value;
    this.selectedDate = dateValue; // Gán giá trị chuỗi YYYY-MM-DD

    // Đồng bộ giá trị vào Reactive Form
    this.appointmentForm.patchValue({
      appointmentDate: dateValue,
    });

    // Kích hoạt kiểm tra lịch (đề phòng trường hợp chọn bác sĩ trước, chọn ngày sau)
    this.onDateOrDoctorChange();
  }

  selectTimeSlot(slot: TimeSlot) {
    if (slot.isAvailable) {
      this.selectedTime = slot.time;
      this.appointmentForm.patchValue({
        startTime: slot.time,
      });
    }
  }

  onSubmit() {
    if (this.appointmentForm.invalid) {
      this.appointmentForm.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    const formValue = this.appointmentForm.getRawValue();
    if (formValue.startTime && formValue.startTime.length === 5) {
      formValue.startTime = `${formValue.startTime}:00`;
    }
    this.saved.emit(formValue);
  }

  closeForm() {
    this.closed.emit();
  }

  // PATIENT
  selectPatientId?: number;

  searchPatient = new FormControl('');
  patients: PatientLookupResponse[] = [];
  selectedPatient?: PatientLookupResponse;

  loadPatientById(id: number) {
    this.patientService.getPatientById(id).subscribe({
      next: (patient) => {
        this.appointmentForm.patchValue({
          patientId: patient.id,
        });
      }
    });
  }

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
    this.selectedDoctorId = doctor.id;
    this.appointmentForm.patchValue({
      doctorId: doctor.id,
    });
    this.searchDoctor.setValue(doctor.fullName, { emitEvent: false });
    this.doctors = [];
    this.onDateOrDoctorChange(); // reload lịch
  }

  // FILL PATIENT NAME

  private _preFillPatientName?: { id: number; fullName: string };

  @Input() set preFillPatientName(value: { id: number; fullName: string } | undefined) {

    this._preFillPatientName = value;
    if (value && this.appointmentForm) {
      this.appointmentForm.patchValue({
        patientId: value.id
      });
      this.searchPatient.setValue(value.fullName, { emitEvent: false });
      this.patients = [];
    }
  }

  get preFillPatientName() {
    return this._preFillPatientName;
  }
}
