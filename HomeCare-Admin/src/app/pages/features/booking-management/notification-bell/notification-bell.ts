import { Component, HostListener, inject, signal } from '@angular/core';
import { IAdminNotification } from '../../../../core/models/booking-management/admin-notification';
import { MatDialog } from '@angular/material/dialog';
import { BookingSignalRService } from '../../../../core/services/signalr/booking-signalr-service';
import { NotificationService } from '../../../../core/services/booking-management/notification-service';
import { DatePipe } from '@angular/common';
import { TimezonePipe } from "../../../../shared/pipes/timezone/timezone-pipe";
import { AllNotificationsDialog } from '../all-notification-dialog/all-notification-dialog';

@Component({
  selector: 'app-notification-bell',
  imports: [DatePipe, TimezonePipe],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.css',
})
export class NotificationBell {
  readonly notificationService = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly signalRService = inject(BookingSignalRService);

  panelOpen = signal(false);

  togglePanel(): void {
    this.panelOpen.update(v => !v);
  }

  markAllRead(event: MouseEvent): void {
    event.stopPropagation();
    this.notificationService.markAllRead();
  }

  viewAll(): void {
    this.panelOpen.set(false);
    this.dialog.open(AllNotificationsDialog, {
      panelClass: 'notifications-dialog',
      maxWidth: '100vw',
      autoFocus: false
    })
  }

  markOneRead(event: MouseEvent, n: IAdminNotification): void {
    event.stopPropagation();
    if (n.isRead) return;
    this.notificationService.markOneRead(n.id);
  }

  viewAndRead(event: MouseEvent, n: IAdminNotification): void {
    event.stopPropagation();
    if (!n.isRead) {
      this.notificationService.markOneRead(n.id);
    }
    this.panelOpen.set(false);
    this.dialog.closeAll();
    this.signalRService.navigateToCustomer({
      customerId: n.customerId,
      bookingId: n.bookingId,
      paymentMethodValue: n.paymentMethodValue,
      message: n.message,
    });
  }

  @HostListener('document:click', ['$event'])
  onOutsideClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.bell-wrapper')) {
      this.panelOpen.set(false);
    }
  }
}