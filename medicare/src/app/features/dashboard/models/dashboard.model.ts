export interface OverviewResponse {
  date: string;
  // patient
  patientsToday: number;
  newPatientsToday: number;
  totalPatients: number;
  // appointment
  appointmentsToday: number;
  upcomingAppointments: number;
  completedToday: number;
  // revenue
  revenueToday: number;
  revenueThisMonth: number;
  pendingInvoices: number;
  // doctors
  doctorsAvailable: number;
}

export interface AppointmentTodayDto {
  id: number;
  startTime: string;
  endTime: string;
  status: string;
  reason?: string | null;
  patientName: string;
  doctorName: string;
  departmentName: string;
}

export interface BriefPatientInfo {
  HUID: string;
  fullname: string;
  phonenumber: string;
}
