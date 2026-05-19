import { TestBed } from '@angular/core/testing';

import { ServiceType } from './service-type';

describe('ServiceType', () => {
  let service: ServiceType;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServiceType);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
