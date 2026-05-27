import { TestBed } from '@angular/core/testing';

import { DoctorSchedule } from './doctor-schedule';

describe('DoctorSchedule', () => {
  let service: DoctorSchedule;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DoctorSchedule);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
