import { Component, Input, ChangeDetectorRef, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
// services
import { VisitService } from '../../../visit/services/visit';
// models
import { VisitResponse } from '../../../visit/models/visit.model';


@Component({
  selector: 'app-drawer-visit-detail',
  imports: [CommonModule],
  templateUrl: './drawer-visit-detail.html',
  styleUrl: './drawer-visit-detail.scss',
})
export class DrawerVisitDetailComponent {

  @Input() id?: number | string;
  // state for drawer
  @Input() isOpen: boolean = false;
  @Output() close = new EventEmitter<void>();

  constructor(private visitService: VisitService, private cdr: ChangeDetectorRef) { }

  visit?: VisitResponse;

  ngOnInit() {
    this.loadVisitById();
  }

  loadVisitById() {
    if (!this.id) {
      console.error('Visit ID is required to load visit details.', this.id);
      return;
    }
    this.visitService.getVisitById(Number(this.id)).subscribe({
      next: (res) => {
        console.log('Loaded visit details:', res);
        this.visit = res;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading visit details:', error);
      }
    });
  }


  closeDrawer() {
    this.close.emit();
  }
  // helper
  getStatusText(status: string): string {
    const norm = status?.toLowerCase() || '';
    if (norm === 'completed') return 'Đã hoàn thành';
    if (norm === 'scheduled') return 'Sắp tới';
    if (norm === 'cancelled') return 'Đã hủy';
    return status || 'Chưa rõ';
  }
}
