import { Component } from '@angular/core';
import { CouponAdvertisementService, ICouponAdvertisement } from '../../../../core/services/coupon-banner/coupon-advertisement-service';

@Component({
  selector: 'app-coupon-banner',
  imports: [],
  templateUrl: './coupon-banner.html',
  styleUrl: './coupon-banner.css',
})
export class CouponBanner {
  coupon: ICouponAdvertisement | null = null;
  visible = false;
  copied = false;

  constructor(private adService: CouponAdvertisementService) { }

  ngOnInit(): void {
    this.adService.getBestAdvertisement().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.coupon = res.data;
          setTimeout(() => (this.visible = true), 800);
        }
      }
    });
  }

  copyCode(): void {
    if (!this.coupon) return;
    navigator.clipboard.writeText(this.coupon.couponCode).then(() => {
      this.copied = true;
      setTimeout(() => (this.copied = false), 2000);
    });
  }

  dismiss(): void {
    this.visible = false;
    setTimeout(() => (this.coupon = null), 450);
  }
}
