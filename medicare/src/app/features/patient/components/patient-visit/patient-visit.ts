import { ChangeDetectorRef, Component, inject, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
// models
import { PatientVisitResponse } from '../../models/patient.model';
// services
import { PatientService } from '../../services/patient';
import { ClickOutside } from "../../../../core/shared/directives/click-outside";
import { DrawerVisitDetailComponent } from "../drawer-visit-detail/drawer-visit-detail";

@Component({
  selector: 'app-patient-visit',
  imports: [CommonModule, DrawerVisitDetailComponent],
  templateUrl: './patient-visit.html',
  styleUrl: './patient-visit.scss',
})
export class PatientVisitComponent {
  private patientService = inject(PatientService);
  private cdr = inject(ChangeDetectorRef);

  @Input() id?: number | string;

  visits?: PatientVisitResponse[];

  ngOnInit() {
    this.loadPatientVisits();
  }

  loadPatientVisits() {
    if (!this.id) {
      console.error('Patient ID is required to load patient visits.', this.id);
      return;
    }
    const patientId = Number(this.id);
    this.patientService.getPatientVisits(patientId, {}).subscribe({
      next: (res) => {
        console.log('Loaded patient visits:', res);
        this.visits = res.data;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading patient visits:', error);
      }
    });
  }


  // mở drawer chi tiết lần khám
  isDrawerOpen: boolean = false;

  openVisitDrawer(visitId: number | string) {
    this.id = visitId;
    this.isDrawerOpen = true;
  }

  closeVisitDrawer() {
    this.isDrawerOpen = false;
    this.id = undefined;
  }
}
