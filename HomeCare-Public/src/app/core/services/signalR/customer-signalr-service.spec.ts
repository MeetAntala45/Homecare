import { TestBed } from '@angular/core/testing';

import { CustomerSignalrService } from './customer-signalr-service';

describe('CustomerSignalrService', () => {
  let service: CustomerSignalrService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerSignalrService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
