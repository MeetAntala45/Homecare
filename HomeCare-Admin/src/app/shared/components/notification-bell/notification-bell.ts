import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PartnerSystemNotification } from '../../../core/services/partner_leave/partner-systerm-notification.js';
import { AdminSystemNotification } from '../../../core/services/partner_leave/admin-system-notification.js';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.css'
})
export class NotificationBell implements OnInit {
  @Input() role: 'partner' | 'admin' = 'partner';
  @Input() filterTypeId: number | null = null;

  isOpen = false;

  constructor(
    public partnerSvc: PartnerSystemNotification,
    public adminSvc: AdminSystemNotification
  ) {}

  ngOnInit(): void {
    if (this.role === 'partner') this.partnerSvc.loadAll();
    else this.adminSvc.loadAll();
  }

  private get allNotifications() {
    return this.role === 'partner'
      ? this.partnerSvc.notifications()
      : this.adminSvc.notifications();
  }

  get notifications() {
    if (this.filterTypeId !== null) {
      return this.allNotifications.filter(n => n.typeId === this.filterTypeId);
    }
    return this.allNotifications;
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  toggle(): void { this.isOpen = !this.isOpen; }
  close():  void { this.isOpen = false; }

  markAllRead(): void {
    const svc = this.role === 'partner' ? this.partnerSvc : this.adminSvc;

    if (this.filterTypeId !== null) {
      // Only mark filtered type as read locally
      svc.notifications.update(list =>
        list.map(n =>
          n.typeId === this.filterTypeId ? { ...n, isRead: true } : n
        )
      );
      svc.unreadCount.set(
        svc.notifications().filter(n => !n.isRead).length
      );
    } else {
      svc.markAllRead().subscribe(() => {
        svc.unreadCount.set(0);
        svc.notifications.update(list =>
          list.map(n => ({ ...n, isRead: true }))
        );
      });
    }

    this.isOpen = false;
  }

  iconForType(typeId: number): string {
    const map: Record<number, string> = {
      1: 'bi-calendar-check-fill',
      2: 'bi-calendar-x-fill',
      3: 'bi-bell-fill'
    };
    return map[typeId] ?? 'bi-bell-fill';
  }

  colorClass(typeId: number): string {
    const map: Record<number, string> = {
      1: 'nbell-success',
      2: 'nbell-danger',
      3: 'nbell-info'
    };
    return map[typeId] ?? 'nbell-info';
  }
}