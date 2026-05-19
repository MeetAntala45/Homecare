import { TestBed } from '@angular/core/testing';

import { OtpTimerService } from './otp-timer-service';

describe('OtpTimerService', () => {
  let service: OtpTimerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OtpTimerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
