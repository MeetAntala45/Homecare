import { Component, EventEmitter, Inject, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { AdminProfileService } from '../../../../core/services/profile/admin-profile-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { UpdateContactPayload } from '../../../../core/models/profile/profile.model';

@Component({
  selector: 'app-contact-modal',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './contact-modal.html',
  styleUrl: './contact-modal.css',
})

export class ContactModal {

  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ContactModal>,
    private profileService: AdminProfileService,
    private toaster: Toaster,
    @Inject(MAT_DIALOG_DATA) public data: { mobileNumber: string; email: string; address: string }
  ) {
    this.form = this.fb.group({
      mobileNumber: [
        data.mobileNumber,
        [Validators.required, Validators.pattern(/^[1-9]\d{9}$/)]
      ],
      email: [
        data.email,
        [Validators.required, Validators.email]
      ],
      address: [
        data.address,
        [Validators.required, Validators.maxLength(300)]
      ]
    });
  }

  get f() { return this.form.controls; }

  onCancel(): void {
    this.cancelled.emit();
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.form.patchValue({ email: this.form.value.email?.toLowerCase() });

    const payload = this.form.getRawValue() as UpdateContactPayload;
    
    this.profileService.updateContactInfo(payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success(res.message);
          this.dialogRef.close(payload);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.toaster.error('Failed to update. Please try again.');
      }
    });
  }
}