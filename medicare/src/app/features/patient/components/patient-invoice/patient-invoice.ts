import { ChangeDetectorRef, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

// models
import { PatientInvoiceResponse } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';

@Component({
  selector: 'app-patient-invoice',
  imports: [CommonModule],
  templateUrl: './patient-invoice.html',
  styleUrl: './patient-invoice.scss',
})
export class PatientInvoiceComponent {
  @Input() id?: number | string;
  invoices?: PatientInvoiceResponse[];

  constructor(
    private patientService: PatientService,
    private cdr: ChangeDetectorRef,
  ) { }

  ngOnInit() {
    this.loadInvoice();
  }

  loadInvoice() {
    if (!this.id) {
      console.error('Patient ID is required to load invoice.', this.id);
      return;
    }
    const patientId = Number(this.id);
    this.patientService.getPatientInvoices(patientId, {}).subscribe({
      next: (res) => {
        console.log('Loaded patient invoice:', res);
        this.invoices = res.data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading patient invoice:', error);
      },
    });
  }

  // helper format status
  getStatusText(status: string): string {
    const normalized = status?.toLowerCase() || '';
    switch (normalized) {
      case 'paid':
        return 'Đã thanh toán';
      case 'pending':
        return 'Chờ thanh toán';
      case 'overdue':
        return 'Quá hạn';
      case 'cancelled':
        return 'Đã hủy';
      default:
        return status || 'Không rõ';
    }
  }

  getPaymentMethodText(method: string): string {
    const normalized = method?.toLowerCase() || '';
    switch (normalized) {
      case 'cash':
        return 'Tiền mặt';
      case 'credit_card':
        return 'Thẻ tín dụng';
      case 'insurance':
        return 'Bảo hiểm';
      case 'bank_transfer':
        return 'Chuyển khoản ngân hàng';
      default:
        return method || 'Không rõ';
    }
  }

}
