// core/services/tracking/location-sender.service.ts
import { Injectable, OnDestroy } from '@angular/core';
import { TrackingService } from './tracking-service';
@Injectable({ providedIn: 'root' })
export class LocationSenderService implements OnDestroy {
    private watchId: number | null = null;
    private intervalId: any = null;
    private currentBookingId: number | null = null;
    private lastLat: number | null = null;
    private lastLng: number | null = null;

    constructor(private trackingService: TrackingService) { }

    startTracking(bookingId: number): void {
        if (this.watchId !== null) return; 

        this.currentBookingId = bookingId;

        if (!navigator.geolocation) {
            console.error('Geolocation not supported');
            return;
        }

        this.watchId = navigator.geolocation.watchPosition(
            (pos) => {
                this.lastLat = pos.coords.latitude;
                this.lastLng = pos.coords.longitude;
            },
            (err) => console.error('Geolocation error:', err),
            { enableHighAccuracy: true, maximumAge: 5000 }
        );
        this.intervalId = setInterval(() => {
            if (this.lastLat !== null && this.lastLng !== null && this.currentBookingId) {
                this.trackingService.updateLocation(
                    this.currentBookingId,
                    this.lastLat,
                    this.lastLng
                ).subscribe({
                    error: (err) => console.error('Location update failed:', err)
                });
            }
        }, 5000);
    }

    stopTracking(): void {
        if (this.watchId !== null) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
        }

        if (this.intervalId !== null) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }

        this.currentBookingId = null;
        this.lastLat = null;
        this.lastLng = null;
    }

    isTracking(): boolean {
        return this.watchId !== null || this.intervalId !== null;
    }

    ngOnDestroy(): void {
        this.stopTracking();
    }
}