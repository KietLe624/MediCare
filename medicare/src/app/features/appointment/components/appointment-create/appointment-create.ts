import { Component, inject, Output, EventEmitter, OnInit } from '@angular/core';
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
import { debounceTime, switchMap, distinctUntilChanged, of, forkJoin, finalize } from 'rxjs';
// Services
import { AppointmentService } from '../../services/appointment';
import { PatientService } from '../../../patient/services/patient';
import { DoctorService } from '../../../doctor/services/doctor';
import { DoctorScheduleService } from '../../../doctor/services/doctor-schedule';
// Models
import { CreateAppointmentRequest } from '../../models/appointment.model';
import {
  PatientSummaryResponse,
  PagedResponse,
  PatientLookupResponse,
} from '../../../patient/models/patient.model';
import { DoctorLookupResponse, TimeSlot } from '../../../doctor/models/doctor.model';


@Component({
  selector: 'app-appointment-create',
  imports: [ReactiveFormsModule, FormsModule, CommonModule],
  templateUrl: './appointment-create.html',
  styleUrl: './appointment-create.scss',
})
export class AppointmentCreate implements OnInit {
  private fb = inject(FormBuilder);
  private changeDetectorRef = inject(ChangeDetectorRef);

  // Services
  private appointmentService = inject(AppointmentService);
  private patientService = inject(PatientService);
  private doctorService = inject(DoctorService);
  private scheduleService = inject(DoctorScheduleService);


  @Output() close = new EventEmitter<void>();
  @Output() submitForm = new EventEmitter<any>();

  appointmentForm: FormGroup = this.fb.group({
    patientId: [null, Validators.required],
    doctorId: [null, Validators.required],
    appointmentDate: ['', Validators.required],
    startTime: ['', Validators.required],
    reason: ['', Validators.required],
    notes: [''],
  });

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

      const targetDayOfWeek = this.scheduleService.getDayOfWeekFromDate(this.selectedDate);

      // GỌI SONG SONG: Lịch trực gốc & Danh sách lịch hẹn thực tế trong ngày
      forkJoin({
        masterSchedules: this.scheduleService.getDoctorSchedules(this.selectedDoctorId),
        appointments: this.scheduleService.getDoctorAppointments(this.selectedDoctorId, this.selectedDate) // API mới cập nhật
      }).subscribe({
        // Trong hàm forkJoin ở Component của bạn:
        next: ({ masterSchedules, appointments }) => {
          // appointments lúc này chắc chắn là mảng Array(5) nhờ hàm map ở Service

          const schedulesForDay = masterSchedules.filter(s => s.dayOfWeek === targetDayOfWeek);
          const generatedSlots = this.scheduleService.generateTimeSlots(schedulesForDay, 30);

          generatedSlots.forEach(slot => {
            const isSlotBooked = appointments.data?.some(app => {
              const appTimeShort = app.startTime.substring(0, 5); // Cắt "09:00:00" -> "09:00"

              // Khóa giờ nếu trùng thời gian và trạng thái KHÔNG PHẢI Cancelled
              return appTimeShort === slot.time && app.status !== 'Cancelled';
            });

            if (isSlotBooked) {
              slot.isAvailable = false;
            }
          });

          // Lọc lấy giờ trống đưa lên UI
          this.morningSlots = generatedSlots.filter(s => s.period === 'AM' && s.isAvailable);
          this.afternoonSlots = generatedSlots.filter(s => s.period === 'PM' && s.isAvailable);
          this.isLoadingSlots = false;
          this.changeDetectorRef.markForCheck(); // Cập nhật UI ngay sau khi có dữ liệu mới
        },
        error: (err) => {
          console.error('Lỗi khi tải dữ liệu lịch của bác sĩ', err);
          this.isLoadingSlots = false;
        }
      });
    }
  }
  onDateChange(event: any) {
    const dateValue = event.target.value;
    this.selectedDate = dateValue; // Gán giá trị chuỗi YYYY-MM-DD

    // Đồng bộ giá trị vào Reactive Form
    this.appointmentForm.patchValue({
      appointmentDate: dateValue
    });

    // Kích hoạt kiểm tra lịch (đề phòng trường hợp chọn bác sĩ trước, chọn ngày sau)
    this.onDateOrDoctorChange();
  }
  selectTimeSlot(slot: TimeSlot) {
    if (slot.isAvailable) {
      this.selectedTime = slot.time;
      this.appointmentForm.patchValue({
        startTime: slot.time
      });
    }
  }

  closeForm() {
    this.close.emit();
  }

  onSubmit() {
    if (this.isSubmitting) {
      return;
    }

    if (this.appointmentForm.valid) {
      this.isSubmitting = true;
      const payload = {
        ...this.appointmentForm.value,
        patientId: Number(this.appointmentForm.value.patientId),
        doctorId: Number(this.appointmentForm.value.doctorId),
        startTime: this.appointmentForm.value.startTime + ':00', // Thêm giây mặc định
        reason: this.appointmentForm.value.reason || '',
        notes: this.appointmentForm.value.notes || ''
      };
      console.log('values gửi đi:', payload);

      this.appointmentService.createAppointment(payload).pipe(
        finalize(() => {
          this.isSubmitting = false;
        })
      ).subscribe({
        next: (res) => {
          console.log('Tạo cuộc hẹn thành công:', res);
          this.submitForm.emit(payload);
          this.closeForm();
          this.changeDetectorRef.markForCheck();
        },
        error: (err) => {
          if (this.isRouteMismatchError(err)) {
            console.warn('Backend đã lưu lịch hẹn nhưng trả lỗi route mismatch. Đóng form để tránh tạo trùng.');
            this.submitForm.emit(payload);
            this.closeForm();
            this.changeDetectorRef.markForCheck();
            return;
          }

          console.error('Lỗi khi tạo cuộc hẹn:', err);
          // Có thể thêm thông báo lỗi cho người dùng ở đây

        }
      });

    } else {
      console.warn('Gửi form thất bại: Form không hợp lệ (Invalid)!');

      Object.keys(this.appointmentForm.controls).forEach(key => {
        const control = this.appointmentForm.get(key);

        if (control && control.invalid) {
          console.log(`-> Trường [${key}] đang lỗi. Chi tiết lỗi:`, control.errors);
          console.log(`   Giá trị hiện tại của [${key}]:`, control.value);
        }
      });

      this.appointmentForm.markAllAsTouched();
    }
  }

  private isRouteMismatchError(err: any): boolean {
    const message = typeof err?.error === 'string' ? err.error : err?.message || '';
    return message.includes('No route matches the supplied values');
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
    this.selectedDoctorId = doctor.id;

    this.appointmentForm.patchValue({
      doctorId: doctor.id
    });

    this.searchDoctor.setValue(
      doctor.fullName,
      { emitEvent: false }
    );

    this.doctors = [];
    this.onDateOrDoctorChange(); // re load lịch
  }
}
