import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentCalendar } from './appointment-calendar';

describe('AppointmentCalendar', () => {
  let component: AppointmentCalendar;
  let fixture: ComponentFixture<AppointmentCalendar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentCalendar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppointmentCalendar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
