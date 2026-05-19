import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { NgClass } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { Toaster } from '../../../core/services/toaster/toaster';
import { BOOKING_MESSAGES } from '../../../core/constants/customer-user-messages';
import { environment } from '../../../../environments/environment.js';


@Component({
  selector: 'app-change-expert',
  standalone: true,
  imports: [ MatDialogModule, MatIcon],
  templateUrl: './change-expert.html',
  styleUrl: './change-expert.css',
})
export class ChangeExpert {

  selectedPartnerId: number | null = null;
  loading = false;
  baseUrl = environment.apiUrl;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<ChangeExpert>,
    private toaster: Toaster
  ) {}

  ngOnInit() {
    const current = this.data.partners.find((p: any) => p.isCurrentlyAssigned);
    if (current) {
      this.selectedPartnerId = current.id;
    }
  }

  selectPartner(id: number) {
    if (this.loading) return;
    this.selectedPartnerId = id;
  }

  confirm() {
    if (!this.selectedPartnerId || this.loading) return;

    this.loading = true;

    this.data.apiCall(this.selectedPartnerId).subscribe({
      next: (res: any) => {
        this.loading = false;

        if (res.success) {
          this.toaster.success(res.message || BOOKING_MESSAGES.CHANGE_SUCCESS);
          this.dialogRef.close(true);
        } else {
          this.toaster.error(res.message || BOOKING_MESSAGES.CHANGE_ERROR);
        }
      },
      error: (err: any) => {
        this.loading = false;
        this.toaster.error(err?.error?.message || BOOKING_MESSAGES.CHANGE_ERROR);
      }
    });
  }

  cancel() {
    if (this.loading) return;
    this.dialogRef.close(false);
  }

  onImgError(event: any) {
    event.target.src = 'assets/profile-image.png';
  }
}