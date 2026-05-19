import {
  Component, OnInit, OnDestroy,
  AfterViewInit, ViewChild, ElementRef, Input
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import * as L from 'leaflet';
import { CustomerSignalRService, IPartnerLocationUpdate } from '../../../../core/services/signalR/customer-signalr-service';
import { CustomerTrackingService } from '../../../../core/services/signalR/tracking-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { MatTooltipModule } from '@angular/material/tooltip';


@Component({
  selector: 'app-track-partner',
  standalone: true,
  imports: [CommonModule, MatTooltipModule],
  templateUrl: './track-partner.html',
  styleUrl: './track-partner.css'
})
export class TrackPartner implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef;
  isLocationStopped = false;
  isLiveUpdate = false;

  @Input() bookingId!: number;
  @Input() customerLat!: number;  
  @Input() customerLng!: number;
  @Input() partnerName!: string;
  @Input() partnerMobileNumber! : string;

  private map!: L.Map;
  private partnerMarker!: L.Marker;
  private customerMarker!: L.Marker;
  private locationSub!: Subscription;

  isConnecting = true;
  isPartnerOnline = false;
  lastUpdated: string = '';

  private readonly partnerIcon = L.divIcon({
    className: '',
    html: `<div class="partner-pin">
              <svg xmlns="http://www.w3.org/2000/svg" width="34" height="44" viewBox="0 0 36 48">
                <ellipse cx="18" cy="44" rx="7" ry="3" fill="rgba(124,92,191,0.25)"/>
                <path d="M18 0C8.06 0 0 8.06 0 18c0 13.5 18 30 18 30S36 31.5 36 18C36 8.06 27.94 0 18 0z" fill="#7c5cbf"/>
                <circle cx="18" cy="18" r="7" fill="white"/>
              </svg>
              <div class="pin-label">Partner</div>
           </div>`,
    iconSize: [36, 48],
    iconAnchor: [18, 48]
  });

  private readonly customerIcon = L.divIcon({
    className: '',
    html: `<div class="customer-pin">
              <svg xmlns="http://www.w3.org/2000/svg" width="34" height="44" viewBox="0 0 36 48">
                <ellipse cx="18" cy="44" rx="7" ry="3" fill="rgba(26,115,232,0.2)"/>
                <path d="M18 0C8.06 0 0 8.06 0 18c0 13.5 18 30 18 30S36 31.5 36 18C36 8.06 27.94 0 18 0z" fill="#4540e1"/>
                <circle cx="18" cy="18" r="7" fill="white"/>
              </svg>
              <div class="pin-label">You</div>
           </div>`,
    iconSize: [36, 48],
    iconAnchor: [18, 48]
  });

  constructor(
    private signalR: CustomerSignalRService,
    private trackingService: CustomerTrackingService,
    private toaster: Toaster
  ) { }

  async ngOnInit(): Promise<void> {
    await this.signalR.startConnection();
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.initMap(), 300);
  }

  private initMap(): void {
    if (!this.mapContainer?.nativeElement || this.map) return;

    delete (L.Icon.Default.prototype as any)._getIconUrl;
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
      iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
      shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    });


    this.map = L.map(this.mapContainer.nativeElement, {
      center: [this.customerLat ?? 23.0225, this.customerLng ?? 72.5714],
      zoom: 14,
      zoomControl: false
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(this.map);
    L.control.zoom({ position: 'bottomright' }).addTo(this.map);

    this.customerMarker = L.marker(
      [this.customerLat, this.customerLng],
      { icon: this.customerIcon }
    ).addTo(this.map);

    this.loadLastLocation();
    this.subscribeToLocation();

    setTimeout(() => {
      this.map.invalidateSize();
    }, 400);
  }

  private loadLastLocation(): void {
    this.trackingService.getLastLocation(this.bookingId).subscribe({
      next: (res) => {
        this.isConnecting = false;
        if (res.success && res.data) {
          this.placeOrMovePartnerMarker(res.data.latitude, res.data.longitude);
          this.isPartnerOnline = true;
          this.isLiveUpdate = false;
          this.startStaleTimer(); 
        } else {
          this.isPartnerOnline = false;
        }
      },
      error: () => { this.isConnecting = false; }
    });
  }

  private async subscribeToLocation(): Promise<void> {
    await this.signalR.joinTrackingGroup(this.bookingId);

    this.locationSub = this.signalR.partnerLocation$.subscribe(
      (data: IPartnerLocationUpdate) => {
        if (data.bookingId !== this.bookingId) return;
        this.placeOrMovePartnerMarker(data.latitude, data.longitude);
        this.lastUpdated = data.updatedAt;
        this.isLiveUpdate = true;
        this.isPartnerOnline = true;
        this.isLocationStopped = false;
        this.isConnecting = false;
        this.startStaleTimer();
      }
    );
  }

  private staleTimer: any;

  private startStaleTimer(): void {
    clearTimeout(this.staleTimer);
    this.staleTimer = setTimeout(() => {
      console.log('Stale timer fired — marking partner offline');
      this.isPartnerOnline = false;
      this.isLocationStopped = true;
    }, 12000);
  }

  private placeOrMovePartnerMarker(lat: number, lng: number): void {
    if (!this.partnerMarker) {
      this.partnerMarker = L.marker([lat, lng], { icon: this.partnerIcon })
        .addTo(this.map);

      const bounds = L.latLngBounds(
        [this.customerLat, this.customerLng],
        [lat, lng]
      );
      this.map.fitBounds(bounds, { padding: [60, 60] });
    } else {
      this.partnerMarker.setLatLng([lat, lng]);
    }
  }

  getTimeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const secs = Math.floor(diff / 1000);
    if (secs < 10) return 'just now';
    if (secs < 60) return `${secs}s ago`;
    return `${Math.floor(secs / 60)}m ago`;
  }

  copiedIndex: number  = 0;

  onCallClick(mobileNumber: string, tooltip: any): void {
    navigator.clipboard.writeText(mobileNumber);

    this.copiedIndex = 1;

    tooltip.show();

    setTimeout(() => {
      this.copiedIndex = 0;
      tooltip.hide();
    }, 1500);
  }

  ngOnDestroy(): void {
    this.locationSub?.unsubscribe();
    this.signalR.leaveTrackingGroup(this.bookingId);
    clearTimeout(this.staleTimer);
    if (this.map) {
      this.map.remove();
    }
  }
}
