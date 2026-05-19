import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { OtpStateService } from '../../../../core/services/auth/otp-state-service';
import { OtpTimerService } from '../../../../core/services/auth/otp-timer-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { LOGIN_MESSAGES } from '../../../../core/constants/login-messages';
import { InputComponent } from "../../../../shared/components/input-component/input-component";
import { PROFILE_MESSAGES } from '../../../../core/constants/profile-messages';
import { CUSTOMER_ACCESS_TOKEN_KEY } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-verify-otp-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputComponent],
  templateUrl: './verify-otp-modal.html',
  styleUrl: './verify-otp-modal.css'
})
export class VerifyOtpModal {

  @Input() pendingEmail = '';
  @Input() pendingName = '';

  @Output() verified = new EventEmitter<string>();
  @Output() close = new EventEmitter<void>();

  otpControl = new FormControl('', [Validators.required]);
  isVerifying = false;

  constructor(
    private authService: AuthService,
    private otpStateService: OtpStateService,
    private otpTimerService: OtpTimerService,
    private toaster: Toaster
  ) { }

  onVerify() {
    this.otpControl.markAsTouched();
    if (this.otpControl.invalid) return;

    this.isVerifying = true;

    this.authService.verifyOtp({
      name: this.pendingName,
      email: this.pendingEmail,
      otpCode: this.otpControl.value!.trim()
    }).subscribe({
      next: (res) => {
        this.isVerifying = false;
        if (res.success && res.data) {
          localStorage.setItem(CUSTOMER_ACCESS_TOKEN_KEY, res.data.accessToken);
          this.authService.setLoggedIn(true);
          this.otpStateService.setOtpRequested(false);
          this.otpTimerService.clearTimer();
          localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_EMAIL);
          localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_NAME);
          localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.OTP_SENT_AT);
          localStorage.removeItem(LOGIN_MESSAGES.STORAGE_KEYS.CHECKOUT_OTP_EXPIRE_AT);
          this.toaster.success(res.message);
          this.verified.emit(this.pendingEmail);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.isVerifying = false;
        this.toaster.error(PROFILE_MESSAGES.SIGN_IN.OTP_VERIFICATION_FAIL);
      }
    });
  }

  onClose() {
    this.close.emit();
  }
}