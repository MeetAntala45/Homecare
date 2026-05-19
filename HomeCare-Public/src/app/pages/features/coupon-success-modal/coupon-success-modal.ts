import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import confetti from 'canvas-confetti';

@Component({
  selector: 'app-coupon-success-modal',
  templateUrl: './coupon-success-modal.html',
  styleUrl: './coupon-success-modal.css',
})
export class CouponSuccessModal implements OnInit {

  progress = 0;

  constructor(
    private dialogRef: MatDialogRef<CouponSuccessModal>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      couponCode: string;
      discountAmount: number;
    }
  ) {}

  ngOnInit(): void {

    this.fireCrackers();
    setTimeout(() => {
      this.progress = 100;
    });

    setTimeout(() => {
      this.close();
    }, 3000);
  }

  close() {
    this.dialogRef.close();
  }

  fireCrackers() {
    confetti({
      particleCount: 80,
      spread: 70,
      origin: { x: 0, y: 0.6 }
    });

    confetti({
      particleCount: 80,
      spread: 70,
      origin: { x: 1, y: 0.6 }
    });

    setTimeout(() => {
      confetti({
        particleCount: 120,
        spread: 100,
        origin: { y: 0.4 }
      });
    }, 300);
  }

}