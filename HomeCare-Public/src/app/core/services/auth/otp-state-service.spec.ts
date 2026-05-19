import { TestBed } from '@angular/core/testing';

import { OtpStateService } from './otp-state-service';

describe('OtpStateService', () => {
  let service: OtpStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OtpStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
