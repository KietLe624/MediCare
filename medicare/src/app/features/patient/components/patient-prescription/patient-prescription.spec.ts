import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientPrescription } from './patient-prescription';

describe('PatientPrescription', () => {
  let component: PatientPrescription;
  let fixture: ComponentFixture<PatientPrescription>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientPrescription]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientPrescription);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
