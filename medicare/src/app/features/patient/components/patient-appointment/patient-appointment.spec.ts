import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientAppointment } from './patient-appointment';

describe('PatientAppointment', () => {
  let component: PatientAppointment;
  let fixture: ComponentFixture<PatientAppointment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientAppointment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientAppointment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
