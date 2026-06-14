import {
  ChangeDetectorRef,
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { signal } from '@angular/core';
// Components
import { PatientTableComponent } from '../../components/patient-table/patient-table';
import { PatientService } from '../../services/patient';
import {
  PatientQueryParams,
  PatientSummaryResponse,
} from '../../models/patient.model';
import { FilterComponent } from '../../components/filter/filter';
import { CreatePatientComponent } from '../../components/create-patient/create-patient';
import { EditPatientComponent } from '../../components/edit-patient/edit-patient';
// import { AppointmentForm } from '../../../appointment/pages/appointment-form/appointment-form';
import { DrawerVisitComponent } from '../../components/drawer-visit/drawer-visit';

@Component({
  selector: 'app-patient-page',
  imports: [
    PatientTableComponent,
    FilterComponent,
    CommonModule,
    CreatePatientComponent,
    EditPatientComponent,
    // AppointmentForm,
    DrawerVisitComponent,
  ],
  templateUrl: './patient-page.html',
  styleUrl: './patient-page.scss',
})
export class PatientPageComponent implements OnInit {
  // private patientService = inject(PatientService);
  private cdr = inject(ChangeDetectorRef);
  private patientService = inject(PatientService);

  patients: PatientSummaryResponse[] = [];

  thisPatientList = signal<PatientSummaryResponse[]>([]);

  filters: PatientQueryParams = {
    search: '',
    patientType: '',
    bloodType: '',
  };

  ngOnInit() {
    this.loadPatients();
  }

  // event
  onFilterChanged(filters: PatientQueryParams) {
    this.filters = filters;
    this.loadPatients();
  }

  // lấy dữ liệu bệnh nhân từ API
  loadPatients() {
    this.patientService
      .getPatients({
        page: 1,
        pageSize: 20,
        search: this.filters.search,
        patientType: this.filters.patientType,
        bloodType: this.filters.bloodType,
      })
      .subscribe((res) => {
        this.patients = res.data;
        this.cdr.detectChanges();
      });
  }

  selectedPatientId?: number;
  // create
  showCreateModal = false;
  openCreate() {
    this.showCreateModal = true;
  }

  onCreateClosed() {
    this.showCreateModal = false;
  }
  // edit
  showEditModal = false;
  openEdit(patientId: number) {
    this.selectedPatientId = patientId;
    this.showEditModal = true;
  }

  onEditClosed() {
    this.showEditModal = false;
    this.selectedPatientId = undefined;
  }

  // appointment
  showAppointmentModal = false;

  openAppointment(patientId: number) {
    this.selectedPatientId = patientId;
    this.showAppointmentModal = true;
  }

  // drawer visit
  isDrawerOpen = false;

  openHistory(patientId: number) {
    console.log('Opening visit drawer for patient ID:', patientId);
    this.selectedPatientId = patientId;
    this.isDrawerOpen = true;
  }
  closeHistory() {
    this.isDrawerOpen = false;
    this.selectedPatientId = undefined;
  }
}
