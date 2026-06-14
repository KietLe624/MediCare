import { Component, inject, ChangeDetectorRef, Input, EventEmitter, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
// Services
import { ClickOutside } from "../../../../core/shared/directives/click-outside";
// Models
import { PatientSummaryResponse } from '../../models/patient.model';
import { CreateAppointmentComponent } from "../../../appointment/components/create-appointment/create-appointment";

@Component({
  selector: 'app-patient-table',
  imports: [CommonModule, ClickOutside, CreateAppointmentComponent],
  templateUrl: './patient-table.html',
  styleUrls: ['./patient-table.scss'],
})
export class PatientTableComponent {
  @Output() edit = new EventEmitter<number>();
  @Input() patients: PatientSummaryResponse[] = [];

  @Output() createAppointment = new EventEmitter<number>();

  private router = inject(Router);

  openedMenuId: number | null = null;

  toggleMenu(id: number) {
    this.openedMenuId =
      this.openedMenuId === id
        ? null
        : id;
  }
  // đóng dropdown menu khi click ra ngoài
  closeMenu() {
    this.openedMenuId = null;

  }
  // form chỉnh sửa
  onEdit(id: number) {
    this.edit.emit(id);
    console.log('Edit patient with ID:', id);
  }

  // form tạo lịch hẹn
  isFormOpen = signal(false);
  selectedPatient?: { id: number, fullName: string };

  onCreateAppointment(id: number) {
    this.selectedPatient = { id, fullName: this.patients.find(p => p.id === id)?.fullName || '' }
    this.isFormOpen.set(true);
    console.log('Selected patient data for appointment:', this.selectedPatient);
  }

  closeForm() {
    this.isFormOpen.set(false);
    this.selectedPatient = undefined;
    console.log('Closed appointment form');
  }

  // View patient detail
  onView(id: number) {
    console.log('View patient with ID:', id);
    this.router.navigate(['patients', id, 'detail']);
    this.closeMenu();
  }

  // drawer lịch sử khám
  isDrawerVisitOpen = false;
  @Output() visitHistory = new EventEmitter<number>();
  selectedId?: number;

  onViewHistory(id: number) {
    this.selectedId = id;
    this.visitHistory.emit(id);
    console.log('View visit history for patient ID:', id);
    this.closeMenu();
  }
  // helper
  getInitials(fullName: string): string {
    return fullName
      .split(' ')
      .slice(-2)
      .map(x => x[0])
      .join('')
      .toUpperCase();
  }
}


