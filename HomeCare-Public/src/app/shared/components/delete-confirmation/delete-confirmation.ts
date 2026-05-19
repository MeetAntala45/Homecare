import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IDeleteConfirmation } from '../../../core/models/delete-confirmation/delete-confirmation';

@Component({
  selector: 'app-delete-conformation',
  imports: [MatIcon],
  templateUrl: './delete-confirmation.html',
  styleUrl: './delete-confirmation.css',
})
export class DeleteConfirmation {
  loading = false;

  constructor(private dialogRef: MatDialogRef<DeleteConfirmation>,
    @Inject(MAT_DIALOG_DATA) public data: IDeleteConfirmation,
    private toaster : Toaster
  ) { }

  onCancel() {
    this.dialogRef.close(false);
  }

  onConfirm() {
    this.loading = true;

    this.data.apiCall().subscribe({
      next: (res: any) => {
        this.loading = false;
        this.dialogRef.close(true);
        this.toaster.success(res.message);
      },
      error: (err: any) => {
        this.loading = false;
        this.toaster.error(err.error.message);
      }
    });
  }

}
