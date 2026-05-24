import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { AppointmentResponse } from '../../../appointment/models/appointment.model';

@Component({
  selector: 'app-drawer-appointment',
  imports: [CommonModule],
  templateUrl: './drawer-appointment.html',
  styleUrl: './drawer-appointment.scss',
})
export class DrawerAppointment {

  @Input() appointment: AppointmentResponse | null = null;

  @Output() closeDrawer = new EventEmitter<void>();

  onClose() {
    this.closeDrawer.emit();
  }
}
