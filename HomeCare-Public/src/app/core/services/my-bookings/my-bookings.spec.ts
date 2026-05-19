import { TestBed } from '@angular/core/testing';

import { MyBookings } from './my-bookings';

describe('MyBookings', () => {
  let service: MyBookings;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MyBookings);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
