export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// QUERIES
export interface AppointmentQueryParams {
  date?: string; // yyyy-MM-dd
  doctorId?: number;
  patientId?: number;
  status?: string;

  page?: number;
  pageSize?: number;
  sortOrder?: 'asc' | 'desc';
}

// ==============================
// REQUESTS
// ==============================

export interface CreateAppointmentRequest {
  patientId: number;
  doctorId: number;
  appointmentDate: string; // yyyy-MM-dd
  startTime: string; // HH:mm:ss
  reason?: string;
  notes?: string;
}

export interface UpdateAppointmentRequest {
  appointmentDate: string;

  startTime: string;
  endTime: string;

  reason?: string;
  notes?: string;
}

export interface RescheduleAppointmentRequest {
  newDate: string;
  newStartTime: string;
  reason?: string;
}

export interface CancelAppointmentRequest {
  reason?: string;
}

// ==============================
// RESPONSES
// ==============================

export interface AppointmentResponse {
  id: number;

  appointmentDate: string;
  startTime: string;
  endTime: string;

  notes?: string | null;
  reason?: string | null;

  status: string;

  createdAt: Date;
  updatedAt?: Date | null;

  patient: PatientBriefResponse;
  doctor: DoctorBriefResponse;
  department: DepartmentBriefResponse;
}

export interface AppointmentSummaryResponse {
  id: number;

  appointmentDate: string;
  startTime: string;
  endTime: string;

  status: string;
  reason?: string | null;

  patient: PatientBriefResponse;
  doctor: DoctorBriefResponse;
}

// ==============================
// BRIEF RESPONSES
// ==============================

export interface PatientBriefResponse {
  id: number;
  userId?: number;
  uhid?: string;
  fullName?: string;
  phoneNumber?: string;
  // Các trường dưới đây API hiện chưa trả về, cứ để optional (?) để UI hiện dấu '--'
  age?: number;
  gender?: string;
  bloodType?: string;
  avatarUrl?: string;
}
export interface DoctorBriefResponse {
  id: number;
  fullName: string;
  specialization?: string;
}
export interface DepartmentBriefResponse {
  id: number;
  name: string;
}
