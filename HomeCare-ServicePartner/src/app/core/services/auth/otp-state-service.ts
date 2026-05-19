import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class OtpStateService {

  private readonly KEY = 'otp_requested';
  private readonly EMAIL_KEY = 'email_change_pending_email';
  private readonly EMAIL_EXPIRES_KEY = 'email_change_expires_at';

  setOtpRequested(value: boolean) {
    if (value) {
      localStorage.setItem(this.KEY, 'true');
    } else {
      localStorage.removeItem(this.KEY);
    }
  }

  isOtpRequested(): boolean {
    return localStorage.getItem(this.KEY) === 'true';
  }

  setEmailChangeOtpRequested(newEmail: string) {
    localStorage.setItem(this.EMAIL_KEY, newEmail);
  }

  setEmailChangeExpiresAt(expiresAt: Date | string) {
    localStorage.setItem(
      this.EMAIL_EXPIRES_KEY,
      new Date(expiresAt).toISOString()
    );
  }

  isEmailChangeOtpActive(): boolean {
    const expiresAt = localStorage.getItem(this.EMAIL_EXPIRES_KEY);
    if (!expiresAt) return false;

    const expiryTime = new Date(expiresAt).getTime();
    const now = new Date().getTime();

    if (now >= expiryTime) {
      this.clearEmailChangeOtp();
      return false;
    }

    return true;
  }

  getPendingEmail(): string | null {
    return localStorage.getItem(this.EMAIL_KEY);
  }

  clearEmailChangeOtp() {
    localStorage.removeItem(this.EMAIL_KEY);
    localStorage.removeItem(this.EMAIL_EXPIRES_KEY);
  }
}