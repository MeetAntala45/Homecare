import { TestBed } from '@angular/core/testing';

import { ServiceListing } from './service-listing';

describe('ServiceListing', () => {
  let service: ServiceListing;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServiceListing);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
