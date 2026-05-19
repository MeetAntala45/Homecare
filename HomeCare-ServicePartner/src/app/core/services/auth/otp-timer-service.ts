import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class OtpTimerService {
  private readonly STORAGE_KEY = 'otp_sent_at';

  setOtpSentTime(cooldownSeconds?: number) {
    if (cooldownSeconds) {
      const sentAt = Date.now() - ((60 - cooldownSeconds) * 1000);
      localStorage.setItem(this.STORAGE_KEY, sentAt.toString());
    } else {
      localStorage.setItem(this.STORAGE_KEY, Date.now().toString());
    }
  }

  getRemainingSeconds(cooldownSeconds: number = 60): number {
    const sentAt = localStorage.getItem(this.STORAGE_KEY);
    if (!sentAt) return 0;

    const elapsed = Math.floor((Date.now() - parseInt(sentAt)) / 1000);
    const remaining = cooldownSeconds - elapsed;

    if (remaining <= 0) {
      this.clearTimer();
      return 0;
    }

    return remaining;
  }

  clearTimer() {
    localStorage.removeItem(this.STORAGE_KEY);
  }
}