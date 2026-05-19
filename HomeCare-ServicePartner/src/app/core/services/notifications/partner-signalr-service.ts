import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { Toaster } from '../toaster/toaster';
import { PartnerNotificationService } from './partner-notification-service';
import { IPartnerNotification } from '../../models/notifications/partner-notification';
import { Subject } from 'rxjs';
import { API_PUBLIC_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class PartnerSignalRService {
    private readonly router = inject(Router);
    private readonly toaster = inject(Toaster);
    private readonly notificationService = inject(PartnerNotificationService);
    private hubConnection!: signalR.HubConnection;
    readonly newBookingAssigned$ = new Subject<IPartnerNotification>();
    private partnerId!: number;

    startConnection(partnerId: number): void {
        if (
            this.hubConnection &&
            (this.hubConnection.state === signalR.HubConnectionState.Connected ||
                this.hubConnection.state === signalR.HubConnectionState.Connecting)
        ) {
            return;
        }

        this.partnerId = partnerId;

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_PUBLIC_URL}/hubs/booking`, {
                withCredentials: true,
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Debug)
            .build();

        this.hubConnection.onreconnected(() => {
            this.hubConnection
                .invoke('JoinPartnerGroup', this.partnerId)
                .catch(() => { });
        });

        this.hubConnection
            .start()
            .then(() => {
                this.hubConnection.invoke('JoinPartnerGroup', this.partnerId);
            })
            .catch(() => { });

        this.hubConnection.on('NewPartnerBooking', (notification: IPartnerNotification) => {
            const partnerNotif: IPartnerNotification = {
                id: 0,
                bookingId: notification.bookingId,
                customerId: notification.customerId,
                customerName: notification.customerName ?? '',
                serviceName: notification.serviceName ?? '',
                paymentMethod: notification.paymentMethod ?? '',
                paymentMethodValue: notification.paymentMethodValue,
                slotDate: notification.slotDate ?? '',
                slotTime: notification.slotTime ?? '',
                amount: notification.amount,
                message: notification.message ?? '',
                isRead: false,
                createdAt: new Date().toISOString(),
                status: notification.status ?? 'pending',
            };

            this.notificationService.prepend(partnerNotif);
            this.newBookingAssigned$.next(partnerNotif);
            this.toaster.bookingNotification(notification.message!, () => {
                this.navigateToBooking(notification.bookingId, notification.status ?? 'pending');
            });
        });
    }

    stopConnection(): void {
        if (this.hubConnection) {
            this.hubConnection.invoke('LeavePartnerGroup', this.partnerId).catch(() => { });
            this.hubConnection.stop();
        }
    }


    navigateToBooking(bookingId: number, status: string = 'pending'): void {
        this.router.navigate(['/service-partner/my-jobs'], {
            queryParams: {
                highlightBookingId: bookingId,
                status: status.toLowerCase()
            },
        });
    }

    async joinBookingTrackingGroup(bookingId: number): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinBookingTrackingGroup', bookingId);
        }
    }

    async leaveBookingTrackingGroup(bookingId: number): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('LeaveBookingTrackingGroup', bookingId);
        }
    }

    onPartnerLocationUpdated(callback: (data: {
        bookingId: number;
        latitude: number;
        longitude: number;
        updatedAt: string;
    }) => void): void {
        this.hubConnection.on('PartnerLocationUpdated', callback);
    }

    offPartnerLocationUpdated(): void {
        this.hubConnection?.off('PartnerLocationUpdated');
    }
}