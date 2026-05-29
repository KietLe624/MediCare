import { TestBed } from '@angular/core/testing';

import { ToastInfo } from './toast-info';

describe('ToastInfo', () => {
  let service: ToastInfo;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastInfo);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
