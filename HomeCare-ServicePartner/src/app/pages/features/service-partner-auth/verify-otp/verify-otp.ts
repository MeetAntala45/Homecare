import { Component, OnDestroy, OnInit } from '@angular/core';
import { InputComponent } from "../../../../shared/components/input-component/input-component";
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
import { jwtDecode } from 'jwt-decode';
import { SERVICE_PARTNER_ACCESS_TOKEN_KEY } from '../../../../core/constants/environment-config';



@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [
    InputComponent,
    ReactiveFormsModule,
    MatFormFieldModule,
    CommonModule,
    RouterModule
  ],
  templateUrl: './verify-otp.html',
  styleUrl: './verify-otp.css',
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  email: string = '';
  isSubmitting = false;
  isResending = false;
  resendCooldown = 0;
  isRateLimited = false;

  readonly MESSAGES = LOGIN_MESSAGES;

  private cooldownInterval: ReturnType<typeof setInterval> | undefined;
  private readonly OTP_SENT_KEY = 'otp_sent_at';
  private readonly ACCESS_TOKEN_KEY = SERVICE_PARTNER_ACCESS_TOKEN_KEY;

  constructor(
    private router: Router,
    private auth: AuthService,
    private toaster: Toaster,
    private otpStateService: OtpStateService,
    private otpTimer: OtpTimerService
  ) { }

  ngOnInit(): void {
    this.email =
      history.state.email ||
      localStorage.getItem(this.MESSAGES.STORAGE_KEYS.OTP_EMAIL) ||
      '';

    if (!this.email) {
      this.router.navigate(['/login']);
      return;
    }

    const remaining = this.otpTimer.getRemainingSeconds();
    if (remaining > 0) {
      this.resendCooldown = remaining;
      this.startCountdown();
    }
  }

  otpForm = new FormGroup({
    otp: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  get otpControl(): FormControl<string> {
    return this.otpForm.get('otp') as FormControl<string>;
  }

  private startCountdown(): void {
    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
    }

    this.cooldownInterval = setInterval(() => {
      this.resendCooldown--;

      if (this.resendCooldown <= 0) {
        if (this.cooldownInterval) {
          clearInterval(this.cooldownInterval);
        }
        this.resendCooldown = 0;
        this.otpTimer.clearTimer();
      }
    }, 1000);
  }

  resendOtp(): void {
    if (!this.email || this.isResending || this.resendCooldown > 0) return;

    this.isResending = true;
    this.otpControl.reset('');
    this.otpControl.markAsUntouched();

    const req: ISendOtpRequest = {
      email: this.email.toLowerCase()
    };

    this.auth.sendOtp(req).subscribe({
      next: (res) => {
        this.isResending = false;

        if (res.success) {
          this.toaster.success(res.message);
          this.otpTimer.setOtpSentTime();
          this.resendCooldown = 60;
          this.startCountdown();
          this.otpStateService.setOtpRequested(true);

          this.router.navigate(['/login/verify-otp'], {
            state: { email: this.email.toLowerCase() }
          });
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isResending = false;
        const message =
          err.error?.errors
            ? Object.values(err.error.errors).flat().join('\n')
            : err.error?.message ?? 'Something went wrong';
        this.toaster.error(message);
      }
    });
  }

  onSubmit(): void {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      this.otpControl.markAsTouched();
      return;
    }

    this.isSubmitting = true;

    const req: IVerifyOtpRequest = {
      email: this.email.toLowerCase(),
      otpCode: this.otpForm.value.otp!
    };

    this.auth.verifyOtp(req).subscribe({
      next: (res) => {
        this.isSubmitting = false;

        if (res.success && res.data) {
          this.otpStateService.setOtpRequested(false);
          this.otpTimer.clearTimer();

          localStorage.removeItem(this.MESSAGES.STORAGE_KEYS.OTP_EMAIL);
          localStorage.removeItem(this.OTP_SENT_KEY);
          localStorage.setItem(this.ACCESS_TOKEN_KEY, res.data.accessToken);

          const decoded = jwtDecode<any>(res.data.accessToken);
          const partnerId = parseInt(decoded["nameid"] 
            ?? decoded["sub"] 
            ?? decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] 
            ?? "0");
          localStorage.setItem('partner_id', partnerId.toString());

          this.auth.setLoggedIn(true);
          this.toaster.success(res.message);

          this.router.navigate(['/service-partner/dashboard']);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        const message =
          err.error?.errors
            ? Object.values(err.error.errors).flat().join('\n')
            : err.error?.message ?? 'Something went wrong';
        this.toaster.error(message);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.cooldownInterval) {
      clearInterval(this.cooldownInterval);
    }
  }
}