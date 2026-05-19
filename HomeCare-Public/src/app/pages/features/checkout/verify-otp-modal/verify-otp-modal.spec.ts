import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VerifyOtpModal } from './verify-otp-modal';

describe('VerifyOtpModal', () => {
  let component: VerifyOtpModal;
  let fixture: ComponentFixture<VerifyOtpModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerifyOtpModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerifyOtpModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
