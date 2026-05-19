import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IBlockConfirmation } from '../../../core/models/block-confirmation/IBlockConfirmation';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-block-confirmation',
  imports: [MatIcon],
  templateUrl: './block-confirmation.html',
  styleUrl: './block-confirmation.css',
})
export class BlockConfirmation {
  loading = false;

  constructor(private dialogRef: MatDialogRef<BlockConfirmation>,
    @Inject(MAT_DIALOG_DATA) public data: IBlockConfirmation,
    private toaster : Toaster
  ) { }

  onCancel() {
    this.dialogRef.close(false);
  }

  onConfirm() {
    this.loading = true;

    this.data.apiCall().subscribe({
      next: (res) => {
        this.loading = false;
        this.dialogRef.close(true);
        this.toaster.success(res.message);
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error(err.error.message);
      }
    });
  }
}
