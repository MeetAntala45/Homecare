import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors, FormGroup } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { AdminProfileService } from '../../../../core/services/profile/admin-profile-service';
import { Toaster } from '../../../../core/services/toaster/toaster';

function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const newPwd = group.get('newPassword')?.value;
  const confirmPwd = group.get('confirmPassword')?.value;
  return newPwd === confirmPwd ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-change-password-modal',
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatIconModule],
  templateUrl: './password-modal.html',
  styleUrls: ['./password-modal.css']
})

export class PasswordModal {

  showCurrent = false;
  showNew = false;
  showConfirm = false;
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<PasswordModal>,
    private profileService: AdminProfileService,
    private toaster: Toaster
  ) {
    this.form = this.fb.group(
      {
        currentPassword: ['', Validators.required],
        newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(15), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$/)]],
        confirmPassword: ['', Validators.required]
      },
      { validators: passwordMatchValidator }
    );
  }

  get f() { return this.form.controls; }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    this.form.markAllAsTouched();
    this.form.updateValueAndValidity();

    if (this.form.invalid || this.form.errors?.['passwordMismatch']) return;

    this.profileService.changePassword({
      currentPassword: this.f['currentPassword'].value!,
      newPassword: this.f['newPassword'].value!
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success(res.message);
          this.dialogRef.close();
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.toaster.error('Failed to change password.');
      }
    });
  }
}