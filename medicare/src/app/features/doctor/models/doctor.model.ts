export interface DoctorLookupResponse {
  id: number;
  fullName: string;
  specialization?: string;
  isAvailable: boolean;
}
