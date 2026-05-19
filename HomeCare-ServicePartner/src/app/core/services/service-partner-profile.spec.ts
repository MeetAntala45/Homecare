import { TestBed } from '@angular/core/testing';

import { ServicePartnerProfile } from './service-partner-profile';

describe('ServicePartnerProfile', () => {
  let service: ServicePartnerProfile;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServicePartnerProfile);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
