import { Component, inject, OnInit, signal } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { DatePipe } from '@angular/common';
import { TimezonePipe } from '../../../../shared/pipes/timezone/timezone-pipe';
import { NotificationService } from '../../../../core/services/booking-management/notification-service';
import { BookingSignalRService } from '../../../../core/services/signalr/booking-signalr-service';
import { IAdminNotification } from '../../../../core/models/booking-management/admin-notification';


@Component({
  selector: 'app-all-notification-dialog',
  standalone: true,
  imports: [DatePipe, TimezonePipe],
  templateUrl: './all-notification-dialog.html',
  styleUrl: './all-notification-dialog.css'
})
export class AllNotificationsDialog implements OnInit {
  private readonly notificationService = inject(NotificationService);
  private readonly signalRService = inject(BookingSignalRService);
  private readonly dialogRef = inject(MatDialogRef<AllNotificationsDialog>);

  notifications = signal<IAdminNotification[]>([]);
  unreadCount = signal<number>(0);
  isLoading = signal(true);

  ngOnInit(): void {
    this.notificationService.ViewAll().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.notifications.set(res.data.items);
          this.unreadCount.set(res.data.unreadCount);
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  markAllRead(): void {
    this.notificationService.markAllRead();
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
    this.dialogRef.close();
    this.signalRService.navigateToCustomer({
      customerId: n.customerId,
      bookingId: n.bookingId,
      paymentMethodValue: n.paymentMethodValue,
      message: n.message,
    });
  }

  close(): void {
    this.dialogRef.close();
  }
}