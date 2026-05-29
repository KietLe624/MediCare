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

export interface AppointmentToday {
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
// For visit chart
export interface VisitByDate {
  date: string;
  count: number;
}

export interface PatientsByDepartment {
  departmentName: string;
  count: number;
}

export interface RevenueByDate {
  date: string;
  revenue: number;
}

export interface RevenueByMonth {
  month: string;
  revenue: number;
}
