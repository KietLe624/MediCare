import { TestBed } from '@angular/core/testing';

import { Visit } from './visit';

describe('Visit', () => {
  let service: Visit;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Visit);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
