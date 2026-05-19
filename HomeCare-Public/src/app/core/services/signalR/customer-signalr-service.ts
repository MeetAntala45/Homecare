// core/services/signalr/customer-signalr.service.ts
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { API_BASE_URL, CUSTOMER_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

export interface IPartnerLocationUpdate {
  bookingId: number;
  latitude: number;
  longitude: number;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerSignalRService {
  private hub!: signalR.HubConnection;
  readonly partnerLocation$ = new Subject<IPartnerLocationUpdate>();

  startConnection(): Promise<void> {
    return new Promise((resolve) => {
      if (
        this.hub &&
        (this.hub.state === signalR.HubConnectionState.Connected ||
         this.hub.state === signalR.HubConnectionState.Connecting)
      ) {
        resolve();
        return;
      }
  
      this.hub = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/hubs/booking`, {
          accessTokenFactory: () =>
            localStorage.getItem(CUSTOMER_ACCESS_TOKEN_KEY) ?? ''
        })
        .withAutomaticReconnect()
        .build();
  
      this.hub.on('PartnerLocationUpdated', (data: IPartnerLocationUpdate) => {
        this.partnerLocation$.next(data);
      });
  
      this.hub.start()
        .then(() => resolve())   
        .catch(() => resolve());
    });
  }

  async joinTrackingGroup(bookingId: number): Promise<void> {
    if (this.hub?.state !== signalR.HubConnectionState.Connected) {
      await this.startConnection();
    }
    await this.hub.invoke('JoinBookingTrackingGroup', bookingId);
  }

  async leaveTrackingGroup(bookingId: number): Promise<void> {
    if (this.hub?.state === signalR.HubConnectionState.Connected) {
      await this.hub.invoke('LeaveBookingTrackingGroup', bookingId);
    }
  }

  stopConnection(): void {
    this.hub?.stop();
  }
}