import { TestBed } from '@angular/core/testing';

import { TimezonePipe } from './timezone-pipe';

describe('TimezonePipe', () => {
  let service: TimezonePipe;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimezonePipe);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
