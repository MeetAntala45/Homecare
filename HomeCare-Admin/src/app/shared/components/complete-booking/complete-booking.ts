import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { Toaster } from '../../../core/services/toaster/toaster';
import { FormsModule } from '@angular/forms';
import { BOOKING_MESSAGES } from '../../../core/constants/customer-user-messages';


@Component({
  selector: 'app-complete-booking',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatIcon],
  templateUrl: './complete-booking.html',
  styleUrl: './complete-booking.css',
})
export class CompleteBooking {

  loading = false;

  constructor(
    private dialogRef: MatDialogRef<CompleteBooking>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private toaster: Toaster
  ) {}

  onCancel() {
    if (this.loading) return;
    this.dialogRef.close(false);
  }

  onConfirm() {
    if (this.loading) return;

    this.loading = true;

    this.data.apiCall().subscribe({
      next: (res: any) => {
        this.loading = false;

        if (res.success) {
          this.toaster.success(res.message || BOOKING_MESSAGES.COMPLETE_SUCCESS);
          this.dialogRef.close(true);
        } else {
          this.toaster.error(res.message || BOOKING_MESSAGES.COMPLETE_ERROR);
        }
      },
      error: (err: any) => {
        this.loading = false;
        this.toaster.error(err?.error?.message || BOOKING_MESSAGES.COMPLETE_ERROR);
      }
    });
  }
}