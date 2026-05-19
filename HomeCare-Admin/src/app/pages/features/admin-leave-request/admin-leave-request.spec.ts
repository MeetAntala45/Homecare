import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminLeaveRequest } from './admin-leave-request';

describe('AdminLeaveRequest', () => {
  let component: AdminLeaveRequest;
  let fixture: ComponentFixture<AdminLeaveRequest>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminLeaveRequest]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminLeaveRequest);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
