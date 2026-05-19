import { TestBed } from '@angular/core/testing';

import { CustomerCount } from './customer-count';

describe('CustomerCount', () => {
  let service: CustomerCount;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerCount);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
