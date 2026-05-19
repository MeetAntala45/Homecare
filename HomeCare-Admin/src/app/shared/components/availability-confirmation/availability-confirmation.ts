import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { Toaster } from '../../../core/services/toaster/toaster';

export interface IAvailabilityConfirmation {
  serviceName: string;
  isActivating: boolean;
  apiCall: () => Observable<{ message: string }>;
}

@Component({
  selector: 'app-availability-confirmation',
  standalone: true,
  imports: [MatIconModule, MatProgressSpinnerModule, CommonModule],
  templateUrl: './availability-confirmation.html',
  styleUrl: './availability-confirmation.css',
})
export class AvailabilityConfirmation {
  loading = false;

  constructor(
    private dialogRef: MatDialogRef<AvailabilityConfirmation>,
    @Inject(MAT_DIALOG_DATA) public data: IAvailabilityConfirmation,
    private toaster: Toaster
  ) {}

  get actionLabel(): string {
    return this.data.isActivating ? 'activate' : 'deactivate';
  }

  get actionLabelCapitalized(): string {
    return this.data.isActivating ? 'Activate' : 'Deactivate';
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.loading = true;

    this.data.apiCall().subscribe({
      next: (res) => {
        this.loading = false;
        this.dialogRef.close(true);
        this.toaster.success(res.message);
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error(err?.error?.message ?? err.message);
      },
    });
  }
}
