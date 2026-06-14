import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import {
  PatientQueryParams,
  PagedResponse,
  PatientSummaryResponse,
  PatientLookupResponse,
  UpdatePatientRequest,
  PatientResponse,
  CreatePatientRequest,
  PatientVisitResponse,
  PatientInvoiceResponse,
  PatientAppointmentResponse,
  PatientPrescriptionResponse
} from '../models/patient.model';

@Injectable({
  providedIn: 'root',
})
export class PatientService {
  private http = inject(HttpClient);

  private patientUrl = 'http://localhost:5034/api/Patient';

  searchPatients(keyword: string) {
    return this.http.get<PatientLookupResponse[]>(
      `${this.patientUrl}/search`,
      {
        params: {
          keyword
        }
      }
    );
  }

  // get all
  getPatients(params: PatientQueryParams) {
    try {
      const httpParams = buildHttpParams(params);
      return this.http.get<PagedResponse<PatientSummaryResponse>>(
        `${this.patientUrl}`,
        {
          params: httpParams
        }
      );
    } catch (error) {
      console.error('Không lấy được thông tin:', error);
      throw error;
    }
  }

  getPatientById(id: number) {
    try {
      return this.http.get<PatientResponse>(`${this.patientUrl}/${id}`);
    } catch (error) {
      console.error('Không lấy được thông tin bệnh nhân:', error);
      throw error;
    }
  }

  getPatientHistory(patientId: number, params: PatientQueryParams) {
    const httpParams = buildHttpParams(params);
    try {
      return this.http.get<PagedResponse<any>>(
        `${this.patientUrl}/${patientId}/history`,
        {
          params: httpParams
        }
      );
    } catch (error) {
      console.error('Không lấy được lịch sử bệnh án:', error);
      throw error;
    }
  }

  createPatient(data: CreatePatientRequest) {
    try {
      return this.http.post(`${this.patientUrl}`, data);
    } catch (error) {
      console.error('Tạo bệnh nhân thất bại:', error);
      throw error;
    }
  }

  updatePatientById(patientId: number, data: UpdatePatientRequest) {
    try {
      return this.http.patch(`${this.patientUrl}/${patientId}`, data);
    } catch (error) {
      console.error('Cập nhật bệnh nhân thất bại:', error);
      throw error;
    }
  }

  getPatientVisits(patientId: number, params: PatientQueryParams) {
    const httpParams = buildHttpParams(params);
    try {
      return this.http.get<PagedResponse<PatientVisitResponse>>(
        `${this.patientUrl}/visits?patientId=${patientId}`,
        {
          params: httpParams
        }
      );
    } catch (error) {
      console.error('Không lấy được thông tin lịch sử khám bệnh:', error);
      throw error;
    }
  }

  getPatientInvoices(patientId: number, params: PatientQueryParams) {
    const httpParams = buildHttpParams(params);
    try {
      return this.http.get<PagedResponse<PatientInvoiceResponse>>(
        `${this.patientUrl}/invoices?patientId=${patientId}`,
        {
          params: httpParams
        }
      );
    } catch (error) {
      console.error('Không lấy được thông tin hóa đơn:', error);
      throw error;
    }
  }

  getPatientAppointments(patientId: number, params: PatientQueryParams) {
    const httpParams = buildHttpParams(params);
    try {
      return this.http.get<PagedResponse<PatientAppointmentResponse>>(
        `${this.patientUrl}/appointments?patientId=${patientId}`,
        {
          params: httpParams
        }
      );
    } catch (error) {
      console.error('Không lấy được thông tin lịch hẹn:', error);
      throw error;
    }
  }

  getPatientPrescriptions(patientId: number, params: PatientQueryParams) {
    const httpParams = buildHttpParams(params);
    try {
      return this.http.get<PagedResponse<PatientPrescriptionResponse>>(
        `${this.patientUrl}/prescriptions?patientId=${patientId}`,
        {
          params: httpParams
        }
      );
    } catch (error) {
      console.error('Không lấy được thông tin đơn thuốc:', error);
      throw error;
    }
  }
}
export function buildHttpParams(params: any): HttpParams {

  let httpParams = new HttpParams();

  Object.keys(params).forEach(key => {

    const value = params[key];

    if (
      value !== null &&
      value !== undefined &&
      value !== ''
    ) {
      httpParams = httpParams.set(key, value);
    }
  });

  return httpParams;
}
