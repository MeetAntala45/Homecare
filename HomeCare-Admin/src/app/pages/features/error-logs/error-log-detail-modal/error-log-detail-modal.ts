import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { IErrorLogDetail } from '../../../../core/models/error-logs/IErrorLog';
import { Toaster } from '../../../../../../../HomeCare-Public/src/app/core/services/toaster/toaster';

@Component({
  selector: 'app-error-log-detail-modal',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './error-log-detail-modal.html',
  styleUrl: './error-log-detail-modal.css',
})
export class ErrorLogDetailModal {
  copied = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public log: IErrorLogDetail,
    private dialogRef: MatDialogRef<ErrorLogDetailModal>,
    private toaster: Toaster
  ) {}

  close(): void {
    this.dialogRef.close();
  }

  copyStackTrace(): void {
    if (!this.log.stackTrace) return;
    navigator.clipboard.writeText(this.log.stackTrace).then(() => {
      this.copied = true;
      this.toaster.success('Stack trace copied.');
      setTimeout(() => (this.copied = false), 2000);
    });
  }

  formatDate(raw: string): string {
    const d = new Date(raw);
    return d.toLocaleString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  }
}
