import { TestBed } from '@angular/core/testing';

import { MyJobs } from './my-jobs';

describe('MyJobs', () => {
  let service: MyJobs;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MyJobs);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
