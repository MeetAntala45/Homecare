import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { AdminSystemNotif, ApiResponse } from '../../models/servicepartner_leave/leave.js';
import { API_BASE_URL } from '../../constants/environment-config.js';

@Injectable({ providedIn: 'root' })
export class AdminSystemNotification {

  private hub!: HubConnection;

  private readonly base = `${API_BASE_URL}/api/admin/system-notifications`;
  private readonly hubUrl = `${API_BASE_URL}/hubs/booking`;

  unreadCount = signal<number>(0);
  notifications = signal<AdminSystemNotif[]>([]);

  constructor(private http: HttpClient) { }
  async connectSignalR(token: string): Promise<void> {
    this.hub = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('ReceiveAdminSystemNotification', (notif: AdminSystemNotif) => {
      this.notifications.update(prev => [notif, ...prev]);
      this.unreadCount.update(c => c + 1);
    });

    await this.hub.start();
  }


  loadAll(): void {
    this.http.get<ApiResponse<AdminSystemNotif[]>>(this.base)
      .subscribe(r => this.notifications.set(r.data));

    this.http.get<ApiResponse<number>>(`${this.base}/unread-count`)
      .subscribe(r => this.unreadCount.set(r.data));
  }

  markRead(id: number) {
    return this.http.patch(`${this.base}/${id}/read`, {});
  }

  markAllRead() {
    return this.http.patch(`${this.base}/mark-all-read`, {});
  }

  async disconnect(): Promise<void> {
    await this.hub?.stop();
  }
}