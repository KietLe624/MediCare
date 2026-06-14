import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  inject,
  Input,
  Output,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
// services
import { AppointmentService } from '../../services/appointment';
// models
import {
  AppointmentSummaryResponse,
  PaginatedResponse,
  AppointmentQueryParams,
} from '../../models/appointment.model';
import { AppointmentTableComponent } from '../../components/appointment-table/appointment-table';
import { AppointmentCalendarComponent } from '../../components/appointment-calendar/appointment-calendar';
import { CreateAppointmentComponent } from '../../components/create-appointment/create-appointment';
import { FilterComponent } from "../../components/filter/filter";

@Component({
  selector: 'app-appointment-page',
  imports: [
    CommonModule,
    AppointmentTableComponent,
    AppointmentCalendarComponent,
    CreateAppointmentComponent,
    FilterComponent
  ],
  templateUrl: './appointment-page.html',
  styleUrl: './appointment-page.scss',
})
export class AppointmentPageComponent {
  private appointmentService = inject(AppointmentService);
  private cdr = inject(ChangeDetectorRef);

  appointments: AppointmentSummaryResponse[] | null = null;
  queryParams: Partial<AppointmentQueryParams> = { page: 1, pageSize: 10, sortOrder: 'desc' };

  pagination = {
    totalCount: 0,
    page: 1,
    pageSize: 10,
    totalPages: 0,
  };

  currentView: 'list' | 'calendar' = 'list';

  switchView(view: 'list' | 'calendar') {
    this.currentView = view;
  }

  activeMenuId: number | null = null;

  toggleMenu(id: number, event: Event) {
    event.stopPropagation(); // Ngăn sự kiện click lan ra ngoài
    this.activeMenuId = this.activeMenuId === id ? null : id;
  }

  closeMenu() {
    this.activeMenuId = null;
  }

  ngOnInit() {
    this.loadAppointments();
  }

  @Output() appointmentData: AppointmentSummaryResponse | null = null;

  loadAppointments() {
    this.appointmentService
      .getAppointments({
        page: this.queryParams.page,
        pageSize: this.queryParams.pageSize,
        date: this.queryParams.date,
        doctorId: this.queryParams.doctorId,
        status: this.queryParams.status,
        sortOrder: this.queryParams.sortOrder
      })
      .subscribe({
        next: (res) => {
          this.appointments = res.data;
          this.pagination.totalCount = res.totalCount;
          this.pagination.totalPages = res.totalPages;
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Lỗi khi tải danh sách cuộc hẹn:', error);
        },
      });
  }

  // Form actions
  isOpenForm = signal(false);

  openForm() {
    this.isOpenForm.set(true);
  }

  closeForm() {
    this.isOpenForm.set(false);
  }

  onSubmitForm() {
    this.closeForm();
    this.loadAppointments();
  }

  // phân trang
  @Output() pageChange = new EventEmitter<number>();

  onPageChange(page: number) {
    this.queryParams.page = page;
    this.pageChange.emit(page);
    this.loadAppointments();
    this.cdr.detectChanges();
  }

  // filter

  onFilterChange(
    filters: Partial<AppointmentQueryParams>
  ) {
    this.queryParams = {
      ...this.queryParams,
      ...filters,
      page: 1
    };
    this.loadAppointments();
    this.cdr.detectChanges();
  }

}
