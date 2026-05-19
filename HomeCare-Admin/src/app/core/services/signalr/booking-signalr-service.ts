import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { Toaster } from '../toaster/toaster';
import { IBookingNotification } from '../../models/booking-management/booking-notification';
import { BOOKING_MANAGEMENT_MESSAGES } from '../../constants/booking-management-messages';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../booking-management/notification-service';
import { IAdminNotification } from '../../models/booking-management/admin-notification';
import { API_PUBLIC_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class BookingSignalRService {
  private readonly router = inject(Router);
  private readonly toaster = inject(Toaster);
  private readonly dialog = inject(MatDialog);

  private hubConnection!: signalR.HubConnection;
  private readonly notificationService = inject(NotificationService);

  readonly MESSAGES = BOOKING_MANAGEMENT_MESSAGES;

  readonly newBooking$ = new Subject<IBookingNotification>();

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_PUBLIC_URL}/hubs/booking`, {
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onreconnected(() => {
      this.hubConnection
        .invoke('JoinAdminGroup')
        .catch(() => this.toaster.error(this.MESSAGES.SIGNALR_REJOIN_FAILED));
    });

    this.hubConnection
      .start()
      .then(() => {
        this.hubConnection.invoke('JoinAdminGroup');
      })
      .catch(() => this.toaster.error(this.MESSAGES.SIGNALR_CONNECTION_FAILED));

      this.hubConnection.on('NewBooking', (notification: IBookingNotification) => {
        this.newBooking$.next(notification);

        const adminNotif: IAdminNotification = {
          id: 0,                        
          bookingId: notification.bookingId,
          customerId: notification.customerId,
          customerName: notification.customerName ?? '',
          serviceName: notification.serviceName ?? '',
          paymentMethod: notification.paymentMethod ?? '',
          paymentMethodValue: notification.paymentMethodValue!,
          slotDate: notification.slotDate ?? '',
          slotTime: notification.slotTime ?? '',
          amount: notification.amount!,
          message: notification.message ?? '',
          isRead: false,
          createdAt: new Date().toISOString(),
        };
      
        this.notificationService.prepend(adminNotif);
        this.showBookingToast(notification);
      });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.invoke('LeaveAdminGroup').catch(() => { });
      this.hubConnection.stop();
    }
  }

  private showBookingToast(notification: IBookingNotification): void {
    this.toaster.bookingNotification(notification.message!, () => {
      this.dialog.closeAll();
      this.navigateToCustomer(notification)
    }
    );
  }

  navigateToCustomer(notification: IBookingNotification): void {
    this.router.navigate(['/admin/booking-management'], {
      queryParams: {
        expandCustomerId: notification.customerId,
        expandPaymentMethod: notification.paymentMethodValue,
        highlightBookingId: notification.bookingId,
        resetFilters: true,
      },
    });
  }
}
