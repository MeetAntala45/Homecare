import { TestBed } from '@angular/core/testing';

import { BookingCount } from './booking-count';

describe('BookingCount', () => {
  let service: BookingCount;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BookingCount);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
