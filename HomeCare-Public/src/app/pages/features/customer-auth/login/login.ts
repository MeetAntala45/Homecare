import { Component } from '@angular/core';
import { InputComponent } from "../../../../shared/components/input-component/input-component";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
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

export class Login {
  errorMessage = '';
  isSubmitting = false;

  readonly MESSAGES = LOGIN_MESSAGES;

  constructor(
    private router: Router,
    private auth: AuthService,
    private toaster: Toaster,
    private otpStateService: OtpStateService,
    private otpTimer: OtpTimerService
  ) { }

  loginForm = new FormGroup({
    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    email: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email]
    }),
  });

  get emailControl(): FormControl<string> {
    return this.loginForm.get('email') as FormControl<string>;
  }

  get nameControl(): FormControl<string> {
    return this.loginForm.get('name') as FormControl<string>;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.nameControl.markAsTouched();
      this.emailControl.markAsTouched();
      return;
    }

    this.isSubmitting = true

    const req: ISendOtpRequest = {
      name: this.loginForm.value.name!,
      email: this.loginForm.value.email!.toLocaleLowerCase()
    }

    this.auth.sendOtp(req).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success && res.data) {
          this.toaster.success(res.message);
          this.otpTimer.setOtpSentTime();
          this.otpStateService.setOtpRequested(true);
          localStorage.setItem(this.MESSAGES.STORAGE_KEYS.OTP_EMAIL, this.loginForm.value.email!);
          localStorage.setItem(this.MESSAGES.STORAGE_KEYS.OTP_NAME, this.loginForm.value.name!);
          this.router.navigate(['login/verify-otp'], {
            state: { email: this.loginForm.value.email?.toLocaleLowerCase(), name: this.loginForm.value.name }
          }).then(result => {
          });
        } else {
          this.toaster.error(res.message);
        }
      }, error: (err) => {
        this.toaster.error(err.message);
      }
    })
  }
}
