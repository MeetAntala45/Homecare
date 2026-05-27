// shared/components/share-referral-modal/share-referral-modal.component.ts
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileService } from '../../../../core/services/profile/profile-service';
import { Toaster } from '../../../../core/services/toaster/toaster';

@Component({
  selector: 'app-referral-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: 'referral-modal.html',
  styleUrl: 'referral-modal.css',
})
export class ReferralModal {
  @Input() referralCode = '';
  @Output() closed = new EventEmitter<void>();

  recipientEmail = '';
  emailError = '';
  isSending = false;

  constructor(private profileService: ProfileService, private toaster: Toaster) {}

  onClose() {
    this.closed.emit();
  }

  onSend() {
    this.emailError = '';

    if (!this.recipientEmail.trim()) {
      this.emailError = 'Please enter an email address.';
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.recipientEmail)) {
      this.emailError = 'Please enter a valid email address.';
      return;
    }

    this.isSending = true;

    this.profileService.shareReferral(this.recipientEmail).subscribe({
      next: (res) => {
        this.isSending = false;
        if (res.success) {
          this.toaster.success('Invite sent successfully!');
          this.onClose();
        } else {
          this.emailError = res.message || 'Failed to send invite.';
        }
      },
      error: (err) => {
        this.isSending = false;
        this.emailError = err?.error?.message || 'Something went wrong.';
      },
    });
  }
}
