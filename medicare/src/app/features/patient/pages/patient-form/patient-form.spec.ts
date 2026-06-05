import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientForm } from './patient-form';

describe('PatientForm', () => {
  let component: PatientForm;
  let fixture: ComponentFixture<PatientForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
