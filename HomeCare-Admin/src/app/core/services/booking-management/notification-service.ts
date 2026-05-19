import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/auth/api.response';
import { IAdminNotification, IAdminNotificationPaged } from '../../models/booking-management/admin-notification';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/api/admin/notification`;

  private _notifications = signal<IAdminNotification[]>([]);
  private _unreadCount = signal<number>(0);

  notifications = this._notifications;
  readonly unreadCount = this._unreadCount.asReadonly();

  load(): void {
    this.http.get<IApiResponse<IAdminNotificationPaged>>(this.apiUrl).subscribe({
      next: (res) => {
        if (!res.success) return;
        this._notifications.set(res.data!.items);
        this._unreadCount.set(res.data!.unreadCount);
      }
    });
  }

  prepend(notification: IAdminNotification): void {
    const temp: IAdminNotification = { ...notification, isRead: false, id: 0 };
    this._notifications.update(list => [temp, ...list].slice(0, 20));
    this._unreadCount.update(c => c + 1);
  
    setTimeout(() => {
      this.load();
    }, 1000); 
  }

  markAllRead(): void {
    this.http.patch<IApiResponse<boolean>>(`${this.apiUrl}/mark-read`, {}).subscribe({
      next: (res) => {
        if (!res.success) return;
        this._unreadCount.set(0);
        this._notifications.update(list =>
          list.map(n => ({ ...n, isRead: true }))
        );
      }
    });
  }

  markOneRead(notificationId: number): void{
    this.http.patch<IApiResponse<boolean>>(`${this.apiUrl}/${notificationId}/mark-read`,{}).subscribe({
        next: (res) => {
            if(!res.success) return;
            this.notifications.update(list => 
                list.map(n => n.id === notificationId ? {...n, isRead: true} : n)
            );
            this._unreadCount.update(c => Math.max(0, c - 1));
        }
    })
  }

  ViewAll(): Observable<IApiResponse<IAdminNotificationPaged>>
  {
    return this.http.get<IApiResponse<IAdminNotificationPaged>>(`${this.apiUrl}/view-all`);
  }
}