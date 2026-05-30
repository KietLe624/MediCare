export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DoctorLookupResponse {
  id: number;
  fullName: string;
  specialization?: string;
  isAvailable: boolean;
}

export interface DoctorScheduleResponse {
  id: number;
  dayOfWeek: number; // 1 = Thứ 2, 7 = CN
  startTime: string;
  endTime: string;
  slotDurationMinutes: number; // in minutes
  isActive: boolean;
}

export interface DoctorAppointmentResponse {
  id: number;
  appointmentDate: string; // YYYY-MM-DD
  startTime: string; // HH:mm
  endTime: string; // HH:mm
  reason?: string | null;
  notes?: string | null;
  createdAt: string; // ISO date string
  updatedAt?: string | null; // ISO date string
  status: string;
  patient: PatientBriefResponse;
}



// HELPER
export interface TimeSlot {
  time: string;
  isAvailable: boolean;
  period: 'AM' | 'PM';
}

export interface PatientBriefResponse {
  id: number;
  uhid: string;
  fullName: string;
  phone: string;
}
