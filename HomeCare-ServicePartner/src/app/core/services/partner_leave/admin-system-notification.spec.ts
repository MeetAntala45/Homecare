import { TestBed } from '@angular/core/testing';

import { AdminSystemNotification } from './admin-system-notification';

describe('AdminSystemNotification', () => {
  let service: AdminSystemNotification;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminSystemNotification);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
