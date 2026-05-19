import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  OnDestroy,
  OnInit,
  QueryList,
  ViewChildren
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MyBookings as MyBookingsService } from '../../../core/services/my-bookings/my-bookings';
import { IApiResponse } from '../../../core/models/apiResponse/IApiResponse';
import { IMyBooking } from '../../../core/models/my-bookings/IMyBookings';
import { MatIcon } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PaymentService } from '../../../core/services/payment/payment-services';
import { Toaster } from '../../../core/services/toaster/toaster';
import { BOOKING_SUCCESS_MESSAGES } from '../../../core/constants/booking-success-messages';
import { finalize } from 'rxjs';
import { ReviewModalComponent } from '../review-modal/review-modal';
import { TrackPartner } from '../tracking/track-partner/track-partner';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, MatIcon, MatTooltipModule, ReviewModalComponent, TrackPartner],
  templateUrl: './my-bookings.html',
  styleUrl: './my-bookings.css',
})
export class MyBookings implements OnInit, AfterViewInit, OnDestroy {
  activeTab: 'pending' | 'completed' | 'cancelled' = localStorage.getItem('tab') as any || 'pending';

  @ViewChildren('descRef') descRefs!: QueryList<ElementRef>;

  BASE_URL = API_BASE_URL;
  private readonly TRACKING_KEY = 'tracking_booking_id';

  bookings: IMyBooking[] = [];
  expanded: boolean[] = [];
  isOverflow: boolean[] = [];
  isOpen = false;
  downloadingIds = new Set<number>();

  isLoading = false;

  reviewModalBookingId: number | null = null;
  reviewModalServiceName = '';
  reviewedBookingIds = new Set<number>();

  trackingBookingId: number | null = null;
  trackingBooking: any = null;

  readonly MESSAGES = BOOKING_SUCCESS_MESSAGES;


  private resizeObserver?: ResizeObserver;

  constructor(
    private myBookingsService: MyBookingsService,
    private cdr: ChangeDetectorRef,
    private paymentService: PaymentService,
    private toaster: Toaster
  ) { }

  ngOnInit(): void {
    const savedTab = localStorage.getItem('booking_tab');
    const saved = sessionStorage.getItem(this.TRACKING_KEY);
    if (saved) {
      const data = JSON.parse(saved);
      this.trackingBookingId = data.id;
      this.trackingBooking = data;
    }

    if (savedTab === 'pending' || savedTab === 'completed' || savedTab === 'cancelled') {
      this.activeTab = savedTab;
    } else {
      this.activeTab = 'pending';
    }

    this.getBookings();
  }

  ngAfterViewInit(): void {
    this.descRefs.changes.subscribe(() => {
      setTimeout(() => this.calculateOverflow(), 0);
    });

    setTimeout(() => this.calculateOverflow(), 0);
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
  }

  get shouldScroll(): boolean {
    return this.bookings.length > 4;
  }

  setTab(tab: 'pending' | 'completed' | 'cancelled'): void {
    this.activeTab = tab;
    localStorage.setItem('booking_tab', tab);
    this.getBookings();
  }

  toggleAddress(index: number): void {
    this.expanded[index] = !this.expanded[index];
    this.cdr.detectChanges();
  }

  closePanel(): void {
    this.isOpen = false;
    document.body.style.overflow = '';
  }

  @HostListener('window:resize')
  onResize(): void {
    if (window.innerWidth >= 768 && this.isOpen) {
      this.closePanel();
    }

    setTimeout(() => this.calculateOverflow(), 0);
  }

  calculateOverflow(): void {
    this.descRefs.forEach((ref, i) => {
      const el = ref.nativeElement as HTMLElement;

      el.classList.remove('clamp');
      const fullHeight = el.scrollHeight;

      if (!this.expanded[i]) {
        el.classList.add('clamp');
      }

      const visibleHeight = el.clientHeight;
      this.isOverflow[i] = fullHeight > visibleHeight;

      if (this.expanded[i] === undefined) {
        this.expanded[i] = false;
      }
    });

    this.cdr.detectChanges();
  }

  getFullAddress(booking: IMyBooking): string {
    return [booking.houseFlatNo, booking.landMark, booking.address]
      .filter(Boolean)
      .join(', ');
  }

  getBookings(): void {
    this.isLoading = true;

    setTimeout(() => {
      this.myBookingsService.bookingsByCustomerId().subscribe({
        next: (res: IApiResponse<IMyBooking[]>) => {
          if (res.success && res.data) {
            const allBookings = res.data;

            this.bookings = allBookings
              .filter((booking) => {
                const status = booking.bookingStatus?.toLowerCase();

                if (this.activeTab === 'pending') {
                  return status === 'pending';
                }

                if (this.activeTab === 'completed') {
                  return status === 'completed';
                }

                if (this.activeTab === 'cancelled') {
                  return status === 'cancelled';
                }

                return false;
              })
              .sort((a, b) => {
                const dateA = new Date(a.bookingDate).getTime();
                const dateB = new Date(b.bookingDate).getTime();

                if (dateA !== dateB) {
                  return this.activeTab === 'pending'
                    ? dateA - dateB
                    : dateB - dateA;
                }

                const timeA = new Date(`1970-01-01T${a.slotStartTime}`).getTime();
                const timeB = new Date(`1970-01-01T${b.slotStartTime}`).getTime();

                return this.activeTab === 'pending'
                  ? timeA - timeB
                  : timeB - timeA;
              })
              .map((booking) => {
                const bookingDate = new Date(booking.bookingDate);

                return {
                  ...booking,
                  date: bookingDate.getDate().toString().padStart(2, '0'),
                  month: bookingDate.toLocaleString('en-US', { month: 'short' })
                };
              });
            this.reviewedBookingIds = new Set(
              this.bookings
                .filter(b => b.hasReview)
                .map(b => b.id)
            );
          } else {
            this.bookings = [];
          }

          this.isLoading = false;
        },
        error: (err) => {
          this.toaster.error(err?.error?.message);
          this.bookings = [];
          this.isLoading = false;
        }
      });
    }, 500);
  }

  copiedIndex: number | null = null;

  onCallClick(mobileNumber: string, index: number, tooltip: any): void {
    navigator.clipboard.writeText(mobileNumber);

    this.copiedIndex = index;

    tooltip.show();

    setTimeout(() => {
      this.copiedIndex = null;
      tooltip.hide();
    }, 1500);
  }

  downloadReceipt(bookingId: number): void {
    if (!bookingId) return;

    this.downloadingIds.add(bookingId);

    this.paymentService
      .downloadInvoice(bookingId)
      .pipe(finalize(() => this.downloadingIds.delete(bookingId)))
      .subscribe({
        next: (blob) => this.saveInvoice(blob, bookingId),
        error: (err) => this.toaster.error(err),
      });
  }

  private saveInvoice(blob: Blob, bookingId: number): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');

    a.href = url;
    a.download = `Invoice-${bookingId}.pdf`;
    a.click();

    URL.revokeObjectURL(url);
  }

  openReviewModal(bookingId: number, serviceName: string): void {
    this.reviewModalBookingId = bookingId;
    this.reviewModalServiceName = serviceName;
  }

  closeReviewModal(): void {
    this.reviewModalBookingId = null;
    this.reviewModalServiceName = '';
  }

  onReviewSubmitted(bookingId: number): void {
    this.reviewedBookingIds.add(bookingId);
    this.closeReviewModal();
  }

  openTracking(booking: IMyBooking): void {
    console.log('lat:', booking.latitude, 'lng:', booking.longitude);

    this.trackingBookingId = booking.id;
    this.trackingBooking = booking;

    sessionStorage.setItem(this.TRACKING_KEY, JSON.stringify({
      id: booking.id,
      partnerName: booking.partnerName,
      latitude: booking.latitude,
      longitude: booking.longitude,
      partnerMobileNumber: booking.mobileNumber
    }));
  }

  closeTracking(): void {
    this.trackingBookingId = null;
    this.trackingBooking = null;
    sessionStorage.removeItem(this.TRACKING_KEY);
  }

  isTrackingAvailable(booking: IMyBooking): boolean {
    if (!booking.bookingDate || !booking.slotStartTime) return false;

    const bookingDate = new Date(booking.bookingDate);
    const [hour, minute] = booking.slotStartTime.split(':').map(Number);
    const slotDateTime = new Date(
      bookingDate.getFullYear(),
      bookingDate.getMonth(),
      bookingDate.getDate(),
      hour,
      minute
    );

    const now = new Date();
    const minutesUntilSlot = (slotDateTime.getTime() - now.getTime()) / 60000;

    return minutesUntilSlot <= 30 && minutesUntilSlot >= -10;
  }

  getTrackingAvailableMessage(booking: IMyBooking): string {
    if (!booking.bookingDate || !booking.slotStartTime) return 'Tracking not available';

    const bookingDate = new Date(booking.bookingDate);
    const [hour, minute] = booking.slotStartTime.split(':').map(Number);
    const slotDateTime = new Date(
      bookingDate.getFullYear(),
      bookingDate.getMonth(),
      bookingDate.getDate(),
      hour,
      minute
    );
    const availableAt = new Date(slotDateTime.getTime() - 30 * 60000);

    return `Partner location will be shared at ${availableAt.toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    })}`;
  }
}