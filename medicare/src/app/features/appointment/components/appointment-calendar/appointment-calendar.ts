import { ChangeDetectorRef, Component, inject, Input, OnChanges, OnInit } from '@angular/core';
import { CommonModule } from "@angular/common";
// models
import { AppointmentSummaryResponse, AppointmentStatus } from '../../models/appointment.model';


export interface CalendarCell {
  isEmpty: boolean;
  day?: number;
  date: string;
  isToday?: boolean;
  events: AppointmentSummaryResponse[];
}


@Component({
  selector: 'app-appointment-calendar',
  imports: [CommonModule],
  templateUrl: './appointment-calendar.html',
  styleUrl: './appointment-calendar.scss',
})
export class AppointmentCalendarComponent implements OnChanges {
  private cdr = inject(ChangeDetectorRef);

  @Input() appointments: AppointmentSummaryResponse[] | null = null;

  weekDays = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
  calendarGrid: CalendarCell[] = [];
  currentMonthYear = 'Tháng 6, 2026'; // hardcode tạm
  status?: AppointmentStatus;

  ngOnChanges(): void {
    this.buildCalendarGrid(2026, 5); // Tháng 6 (0-indexed)
  }

  buildCalendarGrid(year: number, month: number) {
    this.calendarGrid = [];
    const firstDay = new Date(year, month, 1).getDay();
    const startOffset = firstDay === 0 ? 6 : firstDay - 1;
    const daysInMonth = new Date(year, month + 1, 0).getDate();

    // 🌟 SỬA Ở ĐÂY: Thêm thuộc tính events: [] vào các ô trống
    for (let i = 0; i < startOffset; i++) {
      this.calendarGrid.push({ isEmpty: true, date: '', events: [] });
    }

    // Các ô có ngày thật giữ nguyên không đổi
    for (let i = 1; i <= daysInMonth; i++) {
      const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
      const dayAppointments = this.appointments?.filter(a => a.appointmentDate === dateStr) || [];
      dayAppointments.sort((a, b) => a.startTime.localeCompare(b.startTime));

      this.calendarGrid.push({
        isEmpty: false,
        day: i,
        date: dateStr,
        isToday: i === 5,
        events: dayAppointments // Chỗ này đã có sẵn events rồi nên an toàn
      });
    }
    this.cdr.detectChanges();
  }

  // select day
  isDayModalOpen = false;
  selectedDateStr = '';
  selectedDayEvents: AppointmentSummaryResponse[] = [];

  // Mở Pop-up khi click vào ô ngày
  openDayDetails(dateStr: string, events: AppointmentSummaryResponse[]) {
    if (!dateStr || !events || events.length === 0) return; // Nếu ngày trống thì không mở Pop-up

    this.selectedDateStr = dateStr;
    this.selectedDayEvents = events;
    this.isDayModalOpen = true;
  }

  // Đóng Pop-up
  closeDayDetails() {
    this.isDayModalOpen = false;
  }


  // helper
  getStatusInfo(AppointmentStatus: string) {
    const s = AppointmentStatus?.toLowerCase() || '';
    switch (s) {
      case 'confirmed': return { text: 'Đã xác nhận', class: 'bg-blue-50 text-blue-600' };
      case 'completed': return { text: 'Đã hoàn thành', class: 'bg-emerald-50 text-emerald-600' };
      case 'cancelled': return { text: 'Đã hủy', class: 'bg-red-50 text-red-600' };
      case 'no_show': return { text: 'Vắng mặt', class: 'bg-slate-100 text-slate-600' };
      default: return { text: 'Chưa rõ', class: 'bg-slate-50 text-slate-500' };
    }
  }

  getEventColor(AppointmentStatus: string) {
    const s = AppointmentStatus?.toLowerCase() || '';
    switch (s) {
      case 'confirmed': return 'bg-blue-500';
      case 'completed': return 'bg-emerald-500';
      case 'cancelled': return 'bg-red-500';
      case 'no_show': return 'bg-slate-400';
      default: return 'bg-slate-300';
    }
  }

}
