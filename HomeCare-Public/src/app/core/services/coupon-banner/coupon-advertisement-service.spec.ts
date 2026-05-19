import { TestBed } from '@angular/core/testing';

import { CouponAdvertisementService } from './coupon-advertisement-service';

describe('CouponAdvertisementService', () => {
  let service: CouponAdvertisementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CouponAdvertisementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
