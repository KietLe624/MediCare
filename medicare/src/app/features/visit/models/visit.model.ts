// =============================
// QUERY PARAMS
// =============================

export interface VisitQueryParams {
  patientId?: number;
  doctorId?: number;
  fromDate?: string;
  toDate?: string;
  status?: string;

  page?: number;
  pageSize?: number;

  sortOrder?: 'asc' | 'desc';
}

// =============================
// REQUESTS
// =============================

export interface CreateVisitRequest {
  appointmentId: number;

  visitDate?: string;

  symptoms?: string;
  diagnosis?: string;
  treatment?: string;
  notes?: string;
}

export interface UpdateVisitRequest {
  symptoms?: string;
  diagnosis?: string;
  treatment?: string;
  notes?: string;
}

// =============================
// PRESCRIPTION REQUESTS
// =============================

export interface AddPrescriptionRequest {
  medicationId: number;

  dosage: string;
  frequency: string;

  duration?: string;
  instructions?: string;
}

export interface UpdatePrescriptionRequest {
  medicationId: number;

  dosage: string;
  frequency: string;

  duration?: string;
  instructions?: string;
}

// =============================
// BRIEF RESPONSES
// =============================

export interface PatientBriefResponse {
  id: number;
  uhid: string;
  fullName: string;
  userId?: number | null;
}

export interface DoctorBriefResponse {
  id: number;
  fullName: string;
  specialization?: string;
  department: string;
}

export interface MedicationBriefResponse {
  id: number;
  name: string;
  genericName?: string;
  unit?: string;
  category?: string;
}

// =============================
// PRESCRIPTION RESPONSE
// =============================

export interface PrescriptionResponse {
  id: number;
  visitId: number;

  dosage: string;
  frequency: string;

  duration?: string;
  instructions?: string;

  createdAt: string;
  updatedAt?: string | null;

  medication: MedicationBriefResponse;
}

// =============================
// VISIT RESPONSE
// =============================

export interface VisitResponse {
  id: number;

  appointmentId: number;

  visitDate: string;

  symptoms?: string;
  diagnosis?: string;
  treatment?: string;
  notes?: string;

  status: string;

  createdAt: string;

  patient: PatientBriefResponse;
  doctor: DoctorBriefResponse;

  prescriptions: PrescriptionResponse[];
}

// =============================
// VISIT SUMMARY RESPONSE
// =============================

export interface VisitSummaryResponse {
  id: number;

  appointmentId: number;

  visitDate: string;

  diagnosis?: string;

  status: string;

  prescriptionCount: number;

  createdAt: string;

  patient: PatientBriefResponse;
  doctor: DoctorBriefResponse;
}
