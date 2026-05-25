import { Component, OnInit } from '@angular/core';
import { InputComponent } from '../../../../shared/components/input-component/input-component';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { ISendOtpRequest } from '../../../../core/models/auth/ISendOtpRequest';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { OtpStateService } from '../../../../core/services/auth/otp-state-service';
import { OtpTimerService } from '../../../../core/services/auth/otp-timer-service';
import { LOGIN_MESSAGES } from '../../../../core/constants/login-messages';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [InputComponent, ReactiveFormsModule, MatFormFieldModule, CommonModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  errorMessage = '';
  isSubmitting = false;
  showReferral = false;
  prefillCode = '';

  isValidatingReferral = false;
  referralValidationError: string | null = null;
  referralValidated = false;

  readonly MESSAGES = LOGIN_MESSAGES;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private auth: AuthService,
    private toaster: Toaster,
    private otpStateService: OtpStateService,
    private otpTimer: OtpTimerService
  ) {}

  loginForm = new FormGroup({
    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    email: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    referralCode: new FormControl<string>('', { nonNullable: true }),
  });

  get emailControl() {
    return this.loginForm.get('email') as FormControl<string>;
  }
  get nameControl() {
    return this.loginForm.get('name') as FormControl<string>;
  }
  get referralControl() {
    return this.loginForm.get('referralCode') as FormControl<string>;
  }

  ngOnInit(): void {
    const ref = this.route.snapshot.queryParamMap.get('ref');
    if (ref) {
      this.prefillCode = ref.toUpperCase();
      this.showReferral = true;
      this.referralControl.setValue(this.prefillCode);
      this.validateReferralCode(this.prefillCode);
    }
  }

  toggleReferral(): void {
    this.showReferral = !this.showReferral;
    if (!this.showReferral) {
      this.referralControl.setValue('');
      this.referralValidationError = null;
      this.referralValidated = false;
      this.isValidatingReferral = false;
    }
  }

  onReferralBlur(): void {
    const code = this.referralControl.value.trim();
    if (!code) {
      this.referralValidationError = null;
      this.referralValidated = false;
      return;
    }
    this.validateReferralCode(code);
  }

  private validateReferralCode(code: string): void {
    this.isValidatingReferral = true;
    this.referralValidationError = null;
    this.referralValidated = false;

    this.auth.validateReferralCode(code).subscribe({
      next: (res) => {
        this.isValidatingReferral = false;
        if (res.data?.isValid) {
          this.referralValidated = true;
          this.referralValidationError = null;
        } else {
          this.referralValidated = false;
          this.referralValidationError =
            res.data?.errorMessage ?? res.message ?? 'Invalid referral code.';
        }
      },
      error: () => {
        this.isValidatingReferral = false;
        this.referralValidated = false;
        this.referralValidationError = 'Could not validate referral code. Please try again.';
      },
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    if (this.showReferral && !this.referralControl.value.trim()) {
      this.toaster.error('Please enter a referral code or turn off the referral toggle.');
      return;
    }

    if (this.showReferral && this.referralControl.value.trim() && this.referralValidationError) {
      this.toaster.error(this.referralValidationError);
      return;
    }

    if (
      this.showReferral &&
      this.referralControl.value.trim() &&
      !this.referralValidated &&
      !this.referralValidationError
    ) {
      const code = this.referralControl.value.trim().toUpperCase();
      this.isValidatingReferral = true;

      this.auth.validateReferralCode(code).subscribe({
        next: (res) => {
          this.isValidatingReferral = false;
          if (res.data?.isValid) {
            this.referralValidated = true;
            this.referralValidationError = null;
            this.proceedToOtp();
          } else {
            this.referralValidationError =
              res.data?.errorMessage ?? res.message ?? 'Invalid referral code.';
            this.toaster.error(this.referralValidationError!);
          }
        },
        error: () => {
          this.isValidatingReferral = false;
          this.referralValidationError = 'Could not validate referral code. Please try again.';
          this.toaster.error(this.referralValidationError!);
        },
      });
      return;
    }

    this.proceedToOtp();
  }

  private proceedToOtp(): void {
    this.isSubmitting = true;

    const req: ISendOtpRequest = {
      name: this.loginForm.value.name!,
      email: this.loginForm.value.email!.toLowerCase(),
    };

    this.auth.sendOtp(req).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success && res.data) {
          this.toaster.success(res.message);
          this.otpTimer.setOtpSentTime();
          this.otpStateService.setOtpRequested(true);
          localStorage.setItem(this.MESSAGES.STORAGE_KEYS.OTP_EMAIL, this.loginForm.value.email!);
          localStorage.setItem(this.MESSAGES.STORAGE_KEYS.OTP_NAME, this.loginForm.value.name!);

          const code = this.showReferral ? this.referralControl.value.trim().toUpperCase() : '';

          if (code) {
            localStorage.setItem('pending_referral_code', code);
          } else {
            localStorage.removeItem('pending_referral_code');
          }

          this.router.navigate(['login/verify-otp'], {
            state: {
              email: this.loginForm.value.email!.toLowerCase(),
              name: this.loginForm.value.name,
              referralCode: code,
            },
          });
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.isSubmitting = false;
        this.toaster.error('Failed to send OTP. Please try again.');
      },
    });
  }
}
