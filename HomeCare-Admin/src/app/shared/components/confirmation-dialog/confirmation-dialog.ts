import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IConfirmDialog } from '../../../core/models/shared-components/IConfirmation';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatIcon],
  templateUrl: './confirmation-dialog.html',
  styleUrl: './confirmation-dialog.css',
})
export class ConfirmationDialog {
  loading = false;

  constructor(
    private dialogRef: MatDialogRef<ConfirmationDialog>,
    @Inject(MAT_DIALOG_DATA) public data: IConfirmDialog,
    private toaster: Toaster
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.loading = true;

    this.data.apiCall().subscribe({
      next: (res: any) => {
        this.loading = false;
        this.dialogRef.close(true);
        this.toaster.success(res.message);
      },
      error: (err: any) => {
        this.loading = false;
        this.toaster.error(err?.error?.message ?? 'Something went wrong.');
      },
    });
  }
}