import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientTable } from './patient-table';

describe('PatientTable', () => {
  let component: PatientTable;
  let fixture: ComponentFixture<PatientTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientTable]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PatientTable);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
