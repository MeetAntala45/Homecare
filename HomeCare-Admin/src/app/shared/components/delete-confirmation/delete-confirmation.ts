import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIcon } from '@angular/material/icon';
import { IDeleteConfirmation } from '../../../core/models/delete-confirmation/delete-confirmation';
import { Toaster } from '../../../core/services/toaster/toaster';

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
      next: (res) => {
        this.loading = false;
        if(res.success){
          this.toaster.success(res.message);
          this.dialogRef.close(true);
        }else{
          this.toaster.error(res.message);
        }
        
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error(err.message);
      }
    });
  }

}
