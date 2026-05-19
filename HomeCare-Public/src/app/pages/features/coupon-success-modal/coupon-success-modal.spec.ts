import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CouponSuccessModal } from './coupon-success-modal';

describe('CouponSuccessModal', () => {
  let component: CouponSuccessModal;
  let fixture: ComponentFixture<CouponSuccessModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CouponSuccessModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CouponSuccessModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
