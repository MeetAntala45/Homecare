import { Component, ViewChild } from '@angular/core';
import {
  FormGroup,
  ReactiveFormsModule,
  FormControl,
  Validators,
  FormGroupDirective,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { InputComponent } from '../../../../shared/components/input-component/input-component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { IForgotPasswordRequest } from '../../../../core/models/auth/forgot-password.request';
import { AUTH_MESSAGES } from '../../../../core/constants/auth-messages';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    InputComponent,
    ReactiveFormsModule,
    MatFormFieldModule,
    CommonModule,
    MatSnackBarModule,
  ],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  errorMessage = '';
  isSubmitting = false;

  constructor(
    private toaster: Toaster,
    private auth:    AuthService
  ) {}

  adminForgotForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
  });

  @ViewChild(FormGroupDirective)
  formDirective!: FormGroupDirective;

  get emailControl(): FormControl {
    return this.adminForgotForm.get('email') as FormControl;
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (this.adminForgotForm.invalid) {
      this.adminForgotForm.markAllAsTouched();
      this.errorMessage = AUTH_MESSAGES.FORGOT_PASSWORD.INVALID_EMAIL;
      return;
    }

    this.isSubmitting = true;

    const request: IForgotPasswordRequest = {
      email: this.adminForgotForm.value.email!,
    };

    this.auth.forgotPassword(request).subscribe({
      next: (res) => {
        this.isSubmitting = false;

        if (res.success) {
          this.toaster.success(res.message);
          this.formDirective.resetForm();
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.isSubmitting = false;
        this.toaster.error(AUTH_MESSAGES.FORGOT_PASSWORD.GENERIC_ERROR);
      },
    });
  }
}