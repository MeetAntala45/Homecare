import { TestBed } from '@angular/core/testing';

import { ServiceDetail } from './service-detail';

describe('ServiceDetail', () => {
  let service: ServiceDetail;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServiceDetail);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
