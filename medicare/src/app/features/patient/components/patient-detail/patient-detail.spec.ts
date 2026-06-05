import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientDetail } from './patient-detail';

describe('PatientDetail', () => {
  let component: PatientDetail;
  let fixture: ComponentFixture<PatientDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
