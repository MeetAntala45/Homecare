import { Component, HostListener, inject, signal } from '@angular/core';


import { DatePipe } from '@angular/common';
import { PartnerSignalRService } from '../../../core/services/notifications/partner-signalr-service';
import { PartnerNotificationService } from '../../../core/services/notifications/partner-notification-service';
import { IPartnerNotification } from '../../../core/models/notifications/partner-notification';
import { TimezonePipe } from "../../../shared/pipes/timezone/timezone-pipe";

@Component({
  selector: 'app-notification-bell',
  imports: [DatePipe, TimezonePipe],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.css',
})
export class NotificationBell {
  readonly notificationService = inject(PartnerNotificationService);
  private readonly signalRService = inject(PartnerSignalRService);

  panelOpen = signal(false);

  togglePanel(): void {
    this.panelOpen.update(v => !v);
  }

  markAllRead(event: MouseEvent): void {
    event.stopPropagation();
    this.notificationService.markAllRead();
  }

  markOneRead(event: MouseEvent, n: IPartnerNotification): void {
    event.stopPropagation();
    if (n.isRead) return;
    this.notificationService.markOneRead(n.id);
  }

  viewAndRead(event: MouseEvent, n: IPartnerNotification): void {
    event.stopPropagation();
    if (!n.isRead) this.notificationService.markOneRead(n.id);
    this.panelOpen.set(false);
    this.signalRService.navigateToBooking(n.bookingId, n.status);
  }

  @HostListener('document:click', ['$event'])
  onOutsideClick(event: MouseEvent): void {
    if (!(event.target as HTMLElement).closest('.bell-wrapper')) {
      this.panelOpen.set(false);
    }
  }
}