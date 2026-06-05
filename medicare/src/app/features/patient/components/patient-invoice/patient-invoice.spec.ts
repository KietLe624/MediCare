import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientInvoice } from './patient-invoice';

describe('PatientInvoice', () => {
  let component: PatientInvoice;
  let fixture: ComponentFixture<PatientInvoice>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientInvoice]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientInvoice);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
