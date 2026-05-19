import { Component, ElementRef, EventEmitter, inject, Input, Output, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { IAvailableCouponResponse } from '../../../../core/models/checkout/available-coupon.response';
import { CommonModule } from '@angular/common';
import { TruncateTooltipDirective } from '../../../../shared/directives/truncate-tooltip';
import { MatIcon } from '@angular/material/icon';
import KeenSlider, { KeenSliderInstance } from 'keen-slider';
import { AfterViewInit, OnChanges, SimpleChanges } from '@angular/core';

type SliderKey = 'coupons';

interface SliderState {
  instance?: KeenSliderInstance;
  isPrevDisabled: boolean;
  isNextDisabled: boolean;
}

@Component({
  selector: 'app-payment-summary',
  imports: [CommonModule, TruncateTooltipDirective, MatIcon],
  templateUrl: './payment-summary.html',
  styleUrl: './payment-summary.css',
})
export class PaymentSummary implements AfterViewInit, OnChanges {

  @ViewChild('sliderRef') sliderRef!: ElementRef<HTMLElement>;
  @ViewChild('couponSlider') couponSlider!: ElementRef<HTMLDivElement>;

  @Input() coupons: IAvailableCouponResponse[] = [];
  @Input() selectedCouponCode: string | null = null;
  @Output() couponApplied = new EventEmitter<IAvailableCouponResponse>();
  @Output() couponRemoved = new EventEmitter<void>();

  @Input() summary = {
    serviceName: '',
    servicePrice: 0,
    taxAmount: 0,
    discountAmount: 0,
    totalAmount: 0,
    taxPct: 0
  };

  ngAfterViewInit(): void {
    setTimeout(() => {
      if (this.coupons?.length) {
        this.initSlider('coupons');
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['coupons']) {
      setTimeout(() => {
        if (this.coupons?.length) {
          this.initSlider('coupons');
        }
      });
    }
  }

  private slider?: KeenSliderInstance;

  private initSlider(key: SliderKey): void {
    if (!this.sliderRef) return;

    this.slider?.destroy();

    this.slider = new KeenSlider(this.sliderRef.nativeElement, {
      loop: false,
      mode: 'snap',
      slides: {
        perView: 1.4,
        spacing: 16,
      },
      created: (slider) => this.updateSliderButtons(slider, key),
      slideChanged: (slider) => this.updateSliderButtons(slider, key),
      updated: (slider) => this.updateSliderButtons(slider, key),
    });
  }
  sliderState: Record<SliderKey, SliderState> = {
    coupons: { isPrevDisabled: true, isNextDisabled: false },
  };

  private updateSliderButtons(slider: KeenSliderInstance, key: SliderKey): void {
    const details = slider.track.details;
    if (!details) return;

    this.sliderState[key].isPrevDisabled = details.rel === 0;
    this.sliderState[key].isNextDisabled = details.rel >= details.maxIdx;
  }

  prevSlide(): void {
    this.slider?.prev();
  }

  nextSlide(): void {
    this.slider?.next();
  }

  isExpanded = false;

  toggleView(): void {
    this.isExpanded = !this.isExpanded;

    if (!this.isExpanded) {
      setTimeout(() => {
        if (this.coupons?.length) {
          this.initSlider('coupons');
        }
      });
    } else {
      this.slider?.destroy();
    }
  }

  applyCoupon(coupon: IAvailableCouponResponse): void {
    this.couponApplied.emit(coupon);
  }

  removeCoupon(): void {
    this.couponRemoved.emit();
  }

  ngOnDestroy(): void {
    this.slider?.destroy();
  }
}
