import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PaginatedResponse, DoctorAppointmentResponse, DoctorScheduleResponse, TimeSlot } from '../models/doctor.model';

@Injectable({
  providedIn: 'root',
})
export class DoctorScheduleService {
  private doctorUrl = 'http://localhost:5034/api/Doctor';
  private http = inject(HttpClient);

  getDoctorSchedules(doctorId: number) {
    return this.http.get<DoctorScheduleResponse[]>(`${this.doctorUrl}/${doctorId}/schedules`);

  }

  getDoctorAppointments(doctorId: number, date: string) {
    return this.http.get<PaginatedResponse<DoctorAppointmentResponse>>(`${this.doctorUrl}/${doctorId}/appointments`, {
      params: { date }
    });
  }

  // HELPER: Tạo các TimeSlot từ DoctorScheduleResponse
  generateTimeSlots(schedules: DoctorScheduleResponse[], intervalMinutes: number = 30): TimeSlot[] {
    let allSlots: TimeSlot[] = [];

    // Chỉ lấy những ca đang Active
    const activeSchedules = schedules.filter(s => s.isActive);

    activeSchedules.forEach(schedule => {
      let currentMins = this.timeToMinutes(schedule.startTime);
      const endMins = this.timeToMinutes(schedule.endTime);

      while (currentMins < endMins) {
        const timeString = this.minutesToTimeString(currentMins);
        const isMorning = currentMins < 12 * 60; // Dưới 12:00 là Sáng

        allSlots.push({
          time: timeString,
          isAvailable: true, // Mặc định là trống, xử lý booking sau
          period: isMorning ? 'AM' : 'PM'
        });

        currentMins += intervalMinutes;
      }
    });

    return allSlots;
  }

  /**
   * HELPER: Lấy DayOfWeek chuẩn API (1 = Thứ 2, 7 = CN) từ chuỗi ngày YYYY-MM-DD
   */
  getDayOfWeekFromDate(dateString: string): number {
    const dateObj = new Date(dateString);
    const jsDay = dateObj.getDay();
    return jsDay === 0 ? 7 : jsDay;
  }

  /**
   * HELPER: Chuyển đổi thời gian từ chuỗi "HH:mm" sang số phút kể từ 00:00
   */
  private timeToMinutes(timeStr: string): number {
    const [hours, mins] = timeStr.split(':').map(Number);
    return hours * 60 + mins;
  }

  /**
   * HELPER: Chuyển đổi số phút kể từ 00:00 sang chuỗi thời gian "HH:mm"
   */
  private minutesToTimeString(totalMins: number): string {
    const hours = Math.floor(totalMins / 60);
    const mins = totalMins % 60;
    return `${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}`;
  }
}
