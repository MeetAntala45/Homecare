import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { Toaster } from '../../../core/services/toaster/toaster';
import { FormsModule } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-cancel-booking',
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    MatIcon,
    MatFormFieldModule,
    MatInputModule,
    CommonModule,
  ],
  templateUrl: './cancel-booking.html',
  styleUrl: './cancel-booking.css',
})
export class CancelBooking {

  reason: string = '';
 

  constructor(
    private dialogRef: MatDialogRef<CancelBooking>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private toaster: Toaster
  ) {}

  touched = false;

  confirm(reasonCtrl: any) {
    reasonCtrl.control.markAsTouched(); 
  
    if (!this.reason || !this.reason.trim()) return;
  
    this.data.apiCall(this.reason.trim()).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.dialogRef.close(true);
        } else {
        }
      },
      error: (err: any) => {
      }
    });
  }
  cancel() {
  
    this.dialogRef.close(false);
  }
}