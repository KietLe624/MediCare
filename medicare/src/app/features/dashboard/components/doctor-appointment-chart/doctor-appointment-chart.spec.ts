import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DoctorAppointmentChart } from './doctor-appointment-chart';

describe('DoctorAppointmentChart', () => {
  let component: DoctorAppointmentChart;
  let fixture: ComponentFixture<DoctorAppointmentChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorAppointmentChart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DoctorAppointmentChart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
