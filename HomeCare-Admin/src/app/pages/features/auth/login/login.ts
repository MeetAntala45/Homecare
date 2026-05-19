import { Component } from '@angular/core';
import { InputComponent } from '../../../../shared/components/input-component/input-component';
import { FormGroup, ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { Router, RouterModule } from '@angular/router';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ILoginRequest } from '../../../../core/models/auth/login.request';
import { AUTH_MESSAGES } from '../../../../core/constants/auth-messages';
import { ADMIN_ACCESS_TOKEN_KEY, ADMIN_USER_ROLE } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [InputComponent, ReactiveFormsModule, MatFormFieldModule, CommonModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  errorMessage  = '';
  isSubmitting  = false;

  private readonly ACCESS_TOKEN_KEY = ADMIN_ACCESS_TOKEN_KEY;

  adminLoginForm = new FormGroup({
    email: new FormControl<string>('', {
      nonNullable: true,
      validators:  [Validators.required, Validators.email],
    }),
    password: new FormControl<string>('', {
      nonNullable: true,
      validators:  [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$/),
      ],
    }),
  });

  constructor(
    private authService: AuthService,
    private router:      Router,
    private toaster:     Toaster
  ) {}

  get emailControl(): FormControl<string> {
    return this.adminLoginForm.get('email') as FormControl<string>;
  }

  get passwordControl(): FormControl<string> {
    return this.adminLoginForm.get('password') as FormControl<string>;
  }

  onSubmit(): void {
    if (this.adminLoginForm.invalid) {
      this.adminLoginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const request: ILoginRequest = this.adminLoginForm.getRawValue();

    this.authService.login(request).subscribe({
      next: (res) => {
        this.isSubmitting = false;

        if (res.success && res.data) {
          localStorage.setItem(this.ACCESS_TOKEN_KEY, res.data.accessToken);
          localStorage.setItem(ADMIN_USER_ROLE, res.data.role = res.data.role == '1' ? 'Super Admin' : 'Admin');
          this.authService.role.set(res.data.role);
          this.toaster.success(res.message);
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isSubmitting = false;

        if (err.status === 401) {
          this.toaster.error(AUTH_MESSAGES.LOGIN.INVALID_CREDENTIALS);
        } else {
          this.toaster.error(AUTH_MESSAGES.LOGIN.GENERIC_ERROR);
        }
      },
    });
  }
}