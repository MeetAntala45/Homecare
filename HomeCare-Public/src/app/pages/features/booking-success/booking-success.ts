import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../../core/services/payment/payment-services';
import { IBookingSuccess } from '../../../core/models/payments/IBookingSuccess';
import { Toaster } from '../../../core/services/toaster/toaster';
import { CustomerHeader } from '../../layouts/customer-layout/customer-header/customer-header';
import { BOOKING_SUCCESS_MESSAGES } from '../../../core/constants/booking-success-messages';
import { MatIcon } from "@angular/material/icon";
import { MatTooltipModule } from '@angular/material/tooltip';
import { AiChatbot } from '../ai-chatbot/ai-chatbot';
import { resolveImageUrl } from '../../../core/utils/image-url.util';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-booking-success',
  standalone: true,
  imports: [CommonModule, CustomerHeader, MatIcon, MatTooltipModule, AiChatbot],
  templateUrl: './booking-success.html',
  styleUrl: './booking-success.css',
})
export class BookingSuccess implements OnInit {
  baseUrl = API_BASE_URL;
  booking: IBookingSuccess | null = null;
  loading = true;
  bookingId!: number;
  isDownloading = false;
  photoPreview: string = '';
  isAssigningPartner = true;

  readonly MESSAGES = BOOKING_SUCCESS_MESSAGES;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private toaster: Toaster
  ) { }

  ngOnInit(): void {
    this.bookingId = Number(this.route.snapshot.queryParamMap.get('bookingId'));

    if (this.bookingId) {
      this.loadBookingDetails();
    } else {
      this.loading = false;
      this.toaster.error(this.MESSAGES.INVALID_BOOKING);
      this.router.navigate(['/customer/home']);
    }
  }

  loadBookingDetails(): void {
    this.paymentService.getBookingSuccessDetails(this.bookingId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.booking = res.data;
          this.photoPreview = resolveImageUrl(res.data.partnerImage);


          const photoUrl = this.photoPreview.includes(' ') || this.photoPreview.includes('.webp');
          if (photoUrl) {
            this.photoPreview = '';
          }

          this.loading = false;

          const spinnerShownKey = `partnerSpinnerShown_${this.bookingId}`;
          const alreadyShown = sessionStorage.getItem(spinnerShownKey);

          if (alreadyShown) {
            this.isAssigningPartner = false;
          } else {
            this.isAssigningPartner = true;
            sessionStorage.setItem(spinnerShownKey, 'true');

            setTimeout(() => {
              this.isAssigningPartner = false;
            }, 1500);
          }

          this.toaster.success(this.MESSAGES.BOOKING_DETAILS.CONFIRM_BOOKING);
        } else {
          this.loading = false;
          const serviceId = res.data?.serviceId;
          const reason = res.data?.failureReason;

          const queryParams =
            reason === 'expired'
              ? { expired: 'true' }
              : reason === 'refunded'
                ? { refunded: 'true' }
                : { failed: 'true' };

          this.toaster.error(
            reason === 'expired'
              ? this.MESSAGES.BOOKING_DETAILS.PAYMENT_EXPIRED
              : reason === 'refunded'
                ? this.MESSAGES.BOOKING_DETAILS.PAYMENT_REFUNDED
                : res.message || this.MESSAGES.BOOKING_DETAILS.PAYMENT_ERROR_MESSAGE
          );

          if (serviceId) {
            this.router.navigate(['/customer/checkout', serviceId], { queryParams });
          } else {
            this.router.navigate(['/customer/home']);
          }
        }
      },
      error: () => {
        this.loading = false;
        this.toaster.error(this.MESSAGES.BOOKING_DETAILS.UNABLE_TO_VERIFY_PAYMENT);
        this.router.navigate(['/customer/home']);
      },
    });
  }

  copied = false;

  onCallClick(mobileNumber: string, tooltip: any): void {
    navigator.clipboard.writeText(mobileNumber);

    this.copied = true;

    tooltip.show();

    setTimeout(() => {
      this.copied = false;
      tooltip.hide();
    }, 1500);
  }

  getAvatarText(): string {
    return this.booking?.partnerName
      ? this.booking?.partnerName
        .split(' ')
        .map((n) => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2)
      : 'P';
  }

  formatDateTime(date: string, time: string): string {
    if (!date || !time) return '';
    const d = new Date(date);
    const day = d.getDate();
    const month = d.toLocaleString('en', { month: 'short' });
    const [h, m] = time.split(':').map(Number);
    const ampm = h >= 12 ? 'PM' : 'AM';
    const hour = h % 12 || 12;
    return `${day} ${month}, ${hour}:${String(m).padStart(2, '0')} ${ampm}`;
  }

  addServices(): void {
    this.router.navigate(['/customer/services']);
  }

  goToBookings(): void {
    this.router.navigate(['/customer/my-bookings']);
  }

  downloadInvoice() {
    if (!this.bookingId) return;

    this.isDownloading = true;
    this.paymentService.downloadInvoice(this.bookingId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Invoice-${this.bookingId}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
        this.isDownloading = false;
      },
      error: async (err) => {
        try {
          const text = await (err.error as Blob).text();
          const json = JSON.parse(text);
          this.toaster.error(json.message);
        } catch {
          this.toaster.error(this.MESSAGES.FAILED_TO_DOWNLOAD_INVOICE);
        }
        this.isDownloading = false;
      },
    });
  }
}
