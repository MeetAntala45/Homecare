import { TestBed } from '@angular/core/testing';

import { PartnerSystermNotification } from './partner-systerm-notification';

describe('PartnerSystermNotification', () => {
  let service: PartnerSystermNotification;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PartnerSystermNotification);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
