import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientVisit } from './patient-visit';

describe('PatientVisit', () => {
  let component: PatientVisit;
  let fixture: ComponentFixture<PatientVisit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientVisit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientVisit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
