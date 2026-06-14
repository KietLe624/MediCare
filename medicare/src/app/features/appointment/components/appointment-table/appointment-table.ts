import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  inject,
  Input,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
// services
import { AppointmentService } from '../../services/appointment';
import { ToastService } from '../../../../core/shared/services/toast-info';
// models
import {
  AppointmentStatus,
  AppointmentSummaryResponse,
  PaginatedResponse,
} from '../../models/appointment.model';
import { AppointmentDetailComponent } from '../appointment-detail/appointment-detail';

@Component({
  selector: 'app-appointment-table',
  imports: [CommonModule, AppointmentDetailComponent],
  templateUrl: './appointment-table.html',
  styleUrl: './appointment-table.scss',
})
export class AppointmentTableComponent {
  private appointmentService = inject(AppointmentService);
  private cdr = inject(ChangeDetectorRef);
  private toast = inject(ToastService);

  @Input() appointments: AppointmentSummaryResponse[] | null = null;
  // @Input() appointmentData: PaginatedResponse<AppointmentSummaryResponse> | null = null;

  AppointmentStatus = AppointmentStatus; // Đưa enum vào để sử dụng trong template

  activeMenuId: number | null = null;

  dropdownPosition: 'top' | 'bottom' = 'bottom';

  toggleMenu(id: number, event: Event) {
    event.stopPropagation();
    if (this.activeMenuId === id) {
      this.activeMenuId = null;
      return;
    }
    this.activeMenuId = id;
    const screenHeight = window.innerHeight;
    const clickY = (event as MouseEvent).clientY;
    const spaceBelow = screenHeight - clickY;
    if (spaceBelow < 250) {
      this.dropdownPosition = 'top';
    } else {
      this.dropdownPosition = 'bottom';
    }
  }

  closeMenu() {
    this.activeMenuId = null;
  }

  // Open modal with appointment details
  selectedId: number | null = null;

  onConfirm(id: number) {
    this.appointmentService.confirmAppointment(id).subscribe({
      next: () => {
        this.toast.success('Đã xác nhận', 'Cập nhật trạng thái thành công');
        this.patchStatus(id, AppointmentStatus.Confirmed);
      },
      error: () => {
        this.toast.error(
          'Lỗi',
          'Không thể cập nhật trạng thái. Vui lòng thử lại.',
        );
      },
    });
  }

  onComplete(id: number) {
    this.appointmentService.completeAppointment(id).subscribe({
      next: () => {
        this.toast.success('Đã hoàn thành', 'Cập nhật trạng thái thành công');
        this.patchStatus(id, AppointmentStatus.Completed);
      },
      error: () => {
        this.toast.error(
          'Lỗi',
          'Không thể cập nhật trạng thái. Vui lòng thử lại.',
        );
      },
    });
  }

  onCancel(id: number) {
    this.appointmentService.cancelAppointment(id).subscribe({
      next: () => {
        this.toast.success('Đã hủy', 'Cập nhật trạng thái thành công');
        this.patchStatus(id, AppointmentStatus.Cancelled);
      },
      error: () => {
        this.toast.error(
          'Lỗi',
          'Không thể cập nhật trạng thái. Vui lòng thử lại.',
        );
      },
    });
  }

  onNoShow(id: number) {
    this.appointmentService.noShowAppointment(id).subscribe({
      next: () => {
        this.toast.success(
          'Đã đánh dấu vắng mặt',
          'Cập nhật trạng thái thành công',
        );
        this.patchStatus(id, AppointmentStatus.NoShow);
      },
      error: () => {
        this.toast.error(
          'Lỗi',
          'Không thể cập nhật trạng thái. Vui lòng thử lại.',
        );
      },
    });
  }

  // Phân trang
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Output() pageChange = new EventEmitter<number>();

  changePage(page: number): void {
    this.pageChange.emit(page);
    this.currentPage = page;
  }

  // helper
  patchStatus(id: number, action: AppointmentStatus) {
    const item = this.appointments?.find((a) => a.id === id);
    if (item) {
      if (action === 'Confirmed') item.status = AppointmentStatus.Confirmed;
      if (action === 'Completed') item.status = AppointmentStatus.Completed;
      if (action === 'Cancelled') item.status = AppointmentStatus.Cancelled;
      if (action === 'NoShow') item.status = AppointmentStatus.NoShow;
    }
    this.closeMenu();
  }

  // Helper hiển thị trạng thái
  getStatusInfo(status: AppointmentStatus) {
    switch (status) {
      case AppointmentStatus.Confirmed:
        return { text: 'Đã xác nhận', class: 'bg-blue-50 text-blue-600' };
      case AppointmentStatus.Completed:
        return {
          text: 'Đã hoàn thành',
          class: 'bg-emerald-50 text-emerald-600',
        };
      case AppointmentStatus.Cancelled:
        return { text: 'Đã hủy', class: 'bg-red-50 text-red-600' };
      case AppointmentStatus.NoShow:
        return { text: 'Vắng mặt', class: 'bg-slate-100 text-slate-600' };
      default:
        return { text: 'Chưa rõ', class: 'bg-slate-50 text-slate-500' };
    }
  }
}
