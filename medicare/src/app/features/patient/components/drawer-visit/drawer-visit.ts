import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
// models
import { PatientVisitResponse } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';

@Component({
  selector: 'app-drawer-visit',
  imports: [CommonModule],
  templateUrl: './drawer-visit.html',
  styleUrl: './drawer-visit.scss',
})
export class DrawerVisitComponent implements OnChanges {
  // Biến điều khiển đóng/mở
  @Input() isOpen = false;
  @Input() id?: number;

  @Output() close = new EventEmitter<void>();
  private patientService = inject(PatientService);
  private cdr = inject(ChangeDetectorRef);


  visits?: PatientVisitResponse[];
  isLoading: boolean = false;

  ngOnChanges() {
    if (this.id) {
      this.loadPatientVisits(this.id);
    }
  }

  loadPatientVisits(id: number) {
    this.isLoading = true;

    this.patientService.getPatientVisits(id, {}).subscribe({
      next: (res) => {
        console.log('Loaded patient visits:', res);
        this.visits = res.data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(`Lỗi khi load chi tiết ca khám số #${id}:`, err);
        this.isLoading = false;
      },
    });
  }

  closeDrawer() {
    this.close.emit();
    console.log('Closed visit drawer');
  }

  getStatusText(status: string): string {
    const norm = status?.toLowerCase() || '';
    if (norm === 'completed') return 'Đã hoàn thành';
    if (norm === 'scheduled') return 'Sắp tới';
    if (norm === 'cancelled') return 'Đã hủy';
    return status || 'Chưa rõ';
  }
}
