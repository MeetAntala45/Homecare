import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CouponBanner } from './coupon-banner';

describe('CouponBanner', () => {
  let component: CouponBanner;
  let fixture: ComponentFixture<CouponBanner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CouponBanner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CouponBanner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
