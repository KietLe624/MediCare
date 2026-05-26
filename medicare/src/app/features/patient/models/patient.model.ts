export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PatientLookupResponse {
  id: number;
  fullName: string;
  UHID: string;
  phoneNumber?: string;
}

export interface PatientQueryParams {
  search?: string;
  patientType?: 'Outpatient' | 'Inpatient';
  bloodType?: string;
  gender?: string;

  page?: number;
  pageSize?: number;

  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PatientHistoryQueryParams {
  search?: string;

  page?: number;
  pageSize?: number;

  sortBy?: string;
  sortOrder?: 'asc' | 'desc';

  totalCount?: number;
}

export interface CreatePatientRequest {
  firstName: string;
  lastName: string;

  dateOfBirth: string;

  gender: string;

  phoneNumber?: string;
  email?: string;
  address?: string;

  bloodType?: string;
  allergies?: string;

  emergencyContactName?: string;
  emergencyContactPhone?: string;

  patientType?: 'Outpatient' | 'Inpatient';

  insuranceProvider?: string;
  insuranceNumber?: string;

  userId?: number | null;
}

export interface UpdatePatientRequest {
  firstName: string;
  lastName: string;

  dateOfBirth: string;

  gender: string;

  phoneNumber?: string;
  email?: string;
  address?: string;

  bloodType?: string;
  allergies?: string;

  emergencyContactName?: string;
  emergencyContactPhone?: string;

  patientType?: 'Outpatient' | 'Inpatient';

  insuranceProvider?: string;
  insuranceNumber?: string;

  userId?: number | null;
}

export interface PatientResponse {
  id: number;
  uhid: string;

  userId?: number | null;

  firstName: string;
  lastName: string;
  fullName: string;

  dateOfBirth: string;
  age: number;

  gender: string;

  phoneNumber?: string;
  email?: string;
  address?: string;

  bloodType?: string;
  allergies?: string;

  emergencyContactName?: string;
  emergencyContactPhone?: string;

  patientType: string;

  insuranceProvider?: string;
  insuranceNumber?: string;

  createdAt: string;
  updatedAt?: string;
}

export interface PatientSummaryResponse {
  id: number;
  uhid: string;

  fullName: string;

  dateOfBirth: string;
  age: number;

  gender: string;

  phoneNumber?: string;

  bloodType?: string;

  patientType: string;

  createdAt: string;
}

export interface PatientAppointmentResponse {
  id: number;

  appointmentDate: string;

  doctorName: string;

  status: string;

  startTime: string;
  endTime: string;

  reason?: string;
  notes?: string;

  createdAt: string;

  doctor: DoctorBriefResponse;

  department: DepartmentBriefResponse;
}

export interface PatientVisitResponse {
  id: number;

  visitDate: string;

  doctorName: string;

  department: string;

  reason: string;

  symptoms?: string;
  diagnosis?: string;
  treatment?: string;
  notes?: string;

  status: string;

  createdAt: string;

  doctor: DoctorBriefResponse;

  prescriptionCount: number;
}

export interface PatientPrescriptionResponse {
  id: number;

  prescriptionDate: string;

  doctorName: string;

  medication: string;

  dosage: string;

  instructions: string;

  unit: number;

  createdAt: string;

  frequency: string;

  duration?: string;

  visitId: number;

  visitDate: string;

  doctor: DoctorBriefResponse;
}

export interface PatientInvoiceResponse {
  id: number;

  invoiceNumber: string;

  status: string;

  paymentMethod: string;

  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;

  notes?: string;

  issuedAt?: string;
  paidAt?: string;

  createdAt: string;
  updatedAt?: string;
}

export interface DoctorBriefResponse {
  id: number;

  fullName: string;

  specialization?: string;

  consultationFee?: number;

  isAvailable: boolean;
}

export interface DepartmentBriefResponse {
  id: number;

  name: string;
}

export interface MedicationBriefResponse {
  id: number;

  name: string;

  genericName: string;

  unit: string;
}

export interface InvoiceItemBriefResponse {
  id: number;

  itemType: string;

  description: string;

  quantity: number;

  unitPrice: number;

  totalPrice: number;
}
