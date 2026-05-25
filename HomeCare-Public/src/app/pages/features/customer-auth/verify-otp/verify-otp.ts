import { Component, inject, OnDestroy } from '@angular/core';
import { InputComponent } from '../../../../shared/components/input-component/input-component';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { IVerifyOtpRequest } from '../../../../core/models/auth/IVerifyOtpRequest';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { OtpStateService } from '../../../../core/services/auth/otp-state-service';
import { ISendOtpRequest } from '../../../../core/models/auth/ISendOtpRequest';
import { OtpTimerService } from '../../../../core/services/auth/otp-timer-service';
import { LOGIN_MESSAGES } from '../../../../core/constants/login-messages';
import { CUSTOMER_ACCESS_TOKEN_KEY } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-verify-otp',
  imports: [InputComponent, ReactiveFormsModule, MatFormFieldModule, CommonModule, RouterModule],
  templateUrl: './verify-otp.html',
  styleUrl: './verify-otp.css',
})
export class VerifyOtp implements OnDestroy {
  email: string = '';
  name: string = '';
  isSubmitting = false;
  isResending = false;
  resendCooldown = 0;
  isRateLimited = false;

  readonly MESSAGES = LOGIN_MESSAGES;

  private cooldownInterval: ReturnType<typeof setInterval> | undefined;
  private readonly OTP_SENT_KEY = 'otp_sent_at';
  private readonly ACCESS_TOKEN_KEY = CUSTOMER_ACCESS_TOKEN_KEY;
  referralCode = '';

  constructor(
    private router: Router,
    private auth: AuthService,
    private toaster: Toaster,
    private otpStateService: OtpStateService,
    private otpTimer: OtpTimerService
  ) {}

  ngOnInit() {
    this.referralCode =
      history.state.referralCode || localStorage.getItem('pending_referral_code') || '';
    this.email =
      history.state.email || localStorage.getItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_EMAIL) || '';
    this.name =
      history.state.name || localStorage.getItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_NAME) || '';
    const remaining = this.otpTimer.getRemainingSeconds();
    if (remaining > 0) {
      this.resendCooldown = remaining;
      this.startCountdown();
    }
  }

  otpForm = new FormGroup({
    otp: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  get otpControl(): FormControl<string> {
    return this.otpForm.get('otp') as FormControl<string>;
  }

  private startCountdown() {
    clearInterval(this.cooldownInterval);
    this.cooldownInterval = setInterval(() => {
      this.resendCooldown--;
      if (this.resendCooldown <= 0) {
        clearInterval(this.cooldownInterval);
        this.otpTimer.clearTimer();
      }
    }, 1000);
  }

  resendOtp() {
    this.isResending = true;
    this.otpControl.reset();
    this.otpControl.markAsUntouched();
    const req: ISendOtpRequest = {
      name: this.name,
      email: this.email,
    };
    this.auth.sendOtp(req).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        this.isResending = false;
        if (res.success) {
          this.toaster.success(res.message);
          this.otpTimer.setOtpSentTime();
          this.resendCooldown = 60;
          this.startCountdown();
          this.otpStateService.setOtpRequested(true);
          this.router.navigate(['/login/verify-otp'], {
            state: { email: this.email, name: this.name },
          });
        } else if (res.data?.isRateLimited) {
          this.isRateLimited = true;
          this.resendCooldown = 0;
          this.toaster.error(res.message);
        } else if (res.data?.cooldownSeconds) {
          this.resendCooldown = res.data.cooldownSeconds;
          this.otpTimer.setOtpSentTime(this.resendCooldown);
          this.startCountdown();
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isResending = false;
        const message = err.error?.errors
          ? Object.values(err.error.errors).flat().join('\n')
          : err.error?.message ?? 'error';
        this.toaster.error(message);
      },
    });
  }

  onSubmit(): void {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    const req: IVerifyOtpRequest = {
      name: this.name,
      email: this.email,
      otpCode: this.otpForm.value.otp!,
      referralCode: this.referralCode || undefined,
    };
    this.auth.verifyOtp(req).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success) {
          this.otpStateService.setOtpRequested(false);
          this.otpTimer.clearTimer();
          localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_EMAIL);
          localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_NAME);
          localStorage.setItem(this.ACCESS_TOKEN_KEY, res.data.accessToken);
          localStorage.removeItem('pending_referral_code');

          this.auth.setLoggedIn(true);
          history.replaceState({}, '', '/verify-otp');
          localStorage.removeItem(this.OTP_SENT_KEY);
          this.router.navigate(['customer/home']);
          this.toaster.success(res.message);

          if (res.data.referralMessage === 'EXISTING_USER_REFERRAL_REJECTED') {
            setTimeout(() => {
              this.toaster.error(
                'Referral codes are only for new users. You have been logged in successfully.'
              );
            }, 800);
          }

          if (
            res.data.isNewUser &&
            this.referralCode &&
            res.data.referralMessage &&
            res.data.referralMessage !== 'EXISTING_USER_REFERRAL_REJECTED'
          ) {
            setTimeout(() => {
              this.toaster.success(res.data.referralMessage!);
            }, 800);
          }
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        const message = err.error?.errors
          ? Object.values(err.error.errors).flat().join('\n')
          : err.error?.message ?? 'error';
        this.toaster.error(message);
      },
    });
  }

  ngOnDestroy() {
    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
    }
  }
}
