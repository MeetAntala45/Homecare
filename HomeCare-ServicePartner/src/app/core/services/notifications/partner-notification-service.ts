import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { IPartnerNotification, IPartnerNotificationPaged } from '../../models/notifications/partner-notification';
import { IApiResponse } from '../../models/api-response/api-response';
import { API_BASE_URL } from '../../constants/environment-config';


@Injectable({ providedIn: 'root' })
export class PartnerNotificationService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${API_BASE_URL}/api/partnernotification`;

    private _notifications = signal<IPartnerNotification[]>([]);
    private _unreadCount = signal<number>(0);

    notifications = this._notifications;
    readonly unreadCount = this._unreadCount.asReadonly();

    load(): void {
        this.http.get<IApiResponse<IPartnerNotificationPaged>>(this.apiUrl).subscribe({
            next: (res) => {
                if (!res.success) return;
                this._notifications.set(res.data!.items);
                this._unreadCount.set(res.data!.unreadCount);
            }
        });
    }

    prepend(notification: IPartnerNotification): void {
        this._notifications.update(list =>
            [{ ...notification, isRead: false }, ...list].slice(0, 20)
        );
        this._unreadCount.update(c => c + 1);

        setTimeout(() => this.load(), 800);
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

    markOneRead(notificationId: number): void {
        this.http.patch<IApiResponse<boolean>>(`${this.apiUrl}/${notificationId}/mark-read`, {}).subscribe({
            next: (res) => {
                if(!res.success) return;
                this.notifications.update(list => 
                    list.map(n => n.id === notificationId ? {...n, isRead: true} : n)
                );
                this._unreadCount.update(c => Math.max(0, c - 1));
            }
        })
    }
}