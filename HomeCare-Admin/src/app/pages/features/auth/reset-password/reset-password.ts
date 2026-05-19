import {
  Component,
  OnInit,
} from '@angular/core';
import {
  FormGroup,
  ReactiveFormsModule,
  FormControl,
  Validators,
  FormsModule,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { InputComponent } from '../../../../shared/components/input-component/input-component';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ActivatedRoute, Router } from '@angular/router';
import { IResetPasswordRequest } from '../../../../core/models/auth/reset-password.request';
import { AUTH_MESSAGES } from '../../../../core/constants/auth-messages';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [InputComponent, ReactiveFormsModule, MatFormFieldModule, CommonModule, FormsModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  token        = '';
  isTokenValid = false;
  resetMessage = '';
  errorMessage = '';
  isSubmitting = false;

  constructor(
    private auth:    AuthService,
    private route:   ActivatedRoute,
    private toaster: Toaster,
    private router:  Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'] || '';
    });

    if (!this.token) {
      this.toaster.error(AUTH_MESSAGES.RESET_PASSWORD.INVALID_LINK);
      return;
    }

    this.auth.validateResetToken(this.token).subscribe({
      next: (res: { message: string; success: boolean }) => {
        this.resetMessage = res.message;

        if (res.success) {
          this.isTokenValid = true;
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.toaster.error(AUTH_MESSAGES.RESET_PASSWORD.INVALID_EXPIRED);
      },
    });
  }

  adminResetForm = new FormGroup(
    {
      password: new FormControl('', [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$/),
      ]),
      confirmPassword: new FormControl('', [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$/),
      ]),
    },
    { validators: passwordMatchValidator() }
  );

  get passwordControl(): FormControl {
    return this.adminResetForm.get('password') as FormControl;
  }

  get confirmPasswordControl(): FormControl {
    return this.adminResetForm.get('confirmPassword') as FormControl;
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (this.adminResetForm.invalid) {
      this.adminResetForm.markAllAsTouched();

      this.errorMessage = this.adminResetForm.hasError('passwordMismatch')
        ? AUTH_MESSAGES.RESET_PASSWORD.PASSWORD_MISMATCH
        : AUTH_MESSAGES.RESET_PASSWORD.INVALID_DETAILS;

      return;
    }

    this.isSubmitting = true;

    const request: IResetPasswordRequest = {
      token:           this.token,
      newPassword:     this.adminResetForm.value.password!,
      confirmPassword: this.adminResetForm.value.confirmPassword!,
    };

    this.auth.resetPassword(request).subscribe({
      next: (res) => {
        this.isSubmitting = false;

        if (res.success) {
          this.toaster.success(res.message);
          this.router.navigate(['/login']);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.isSubmitting = false;
        this.toaster.error(AUTH_MESSAGES.RESET_PASSWORD.GENERIC_ERROR);
      },
    });
  }
}

export function passwordMatchValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password        = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  };
}