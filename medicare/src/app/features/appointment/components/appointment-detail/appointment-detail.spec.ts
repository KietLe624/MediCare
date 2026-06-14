import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentDetail } from './appointment-detail';

describe('AppointmentDetail', () => {
  let component: AppointmentDetail;
  let fixture: ComponentFixture<AppointmentDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppointmentDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
