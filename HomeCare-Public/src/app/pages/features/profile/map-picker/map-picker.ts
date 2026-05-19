import {
  Component, OnInit, AfterViewInit, OnDestroy, OnChanges, SimpleChanges,
  ViewChild, ElementRef, Output, Input, EventEmitter
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

import * as L from 'leaflet'
import { ISavedAddress } from '../../../../core/models/profile/ISavedAddress';
import { InputComponent } from '../../../../shared/components/input-component/input-component';
import { ProfileService } from '../../../../core/services/profile/profile-service';
import { PROFILE_MESSAGES } from '../../../../core/constants/profile-messages';
import { Toaster } from '../../../../core/services/toaster/toaster';

@Component({
  selector: 'app-map-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, InputComponent],
  templateUrl: './map-picker.html',
  styleUrl: './map-picker.css'
})
export class MapPicker implements OnInit, AfterViewInit, OnDestroy, OnChanges {

  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef;

  @Input() initialLat: number = 20.5937;
  @Input() initialLng: number = 78.9629;
  @Input() initialAddress: string = '';
  @Input() editMode: boolean = false;
  @Input() editData: ISavedAddress | null = null;
  @Input() mode: 'add' | 'edit' | 'select' = 'add';

  @Output() addressSelected = new EventEmitter<ISavedAddress>();
  @Output() closeModal = new EventEmitter<void>();
  @Output() changeLocation = new EventEmitter<void>(); 

  private map: any;
  private marker: any;
  private reverseTimeout: any;
  labels: string[] = [];
  selectedLabel: string | null = null;
  showCustomLabelInput = false;

  selectedAddress = '';
  houseNumberControl = new FormControl('', [Validators.required, Validators.maxLength(100)]);
  landmarkControl = new FormControl('', [Validators.required, Validators.maxLength(150)]);
  nicknameControl = new FormControl('', Validators.maxLength(100));
  
  isLoadingLabels = false;

  constructor(
    private profileService: ProfileService,
    private toaster: Toaster
  ) { }

  ngOnInit() {
    if (this.initialAddress) {
      this.selectedAddress = this.initialAddress;
    }
    
    this.loadLabels();

    if(this.editMode && this.editData){
      this.houseNumberControl.setValue(this.editData.houseFlatNo);
      this.landmarkControl.setValue(this.editData.landmark);
      this.selectedAddress = this.editData.displayName ?? '';
    }
  }

  loadLabels() {
    this.isLoadingLabels = true;
    this.profileService.getAddressLabels().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.labels = res.data;
          if (this.editMode && this.editData) {
            this.selectedLabel = this.editData.label;
            this.showCustomLabelInput = false;
          } else {
            this.selectedLabel = this.labels.length > 0 ? this.labels[0] : null;
          }
        }
        this.isLoadingLabels = false;
      },
      error: () => this.isLoadingLabels = false
    });
  }

  onLabelSelect(label: string | null) {
    if (label === null) {
      this.selectedLabel = null;
      this.showCustomLabelInput = true;
      this.nicknameControl.setValidators(Validators.required);
    } else {
      this.selectedLabel = label;
      this.showCustomLabelInput = false;
      this.nicknameControl.clearValidators();
      this.nicknameControl.setValue('');
    }
    this.nicknameControl.updateValueAndValidity();
  }

  ngOnChanges(changes: SimpleChanges) {
    if ((changes['initialLat'] || changes['initialLng']) && this.map) {
      this.flyToLocation(this.initialLat, this.initialLng);
    }
    if (changes['initialAddress'] && this.initialAddress) {
      this.selectedAddress = this.initialAddress;
    }
  }

  ngAfterViewInit() {
    setTimeout(() => this.initMap(), 100);
  }

  ngOnDestroy() {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
    clearTimeout(this.reverseTimeout);
  }

  private initMap() {
    if (!this.mapContainer?.nativeElement || this.map) return;

    this.map = L.map(this.mapContainer.nativeElement, {
      center: [this.initialLat, this.initialLng],
      zoom: 15,
      zoomControl: false
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(this.map);

    L.control.zoom({ position: 'bottomright' }).addTo(this.map);

    const pinIcon = L.divIcon({
      className: '',
      html: `<svg xmlns="http://www.w3.org/2000/svg" width="34" height="44" viewBox="0 0 36 48" class="custom-pin">
        <ellipse cx="18" cy="44" rx="7" ry="3" fill="rgba(26,115,232,0.2)"/>
        <path d="M18 0C8.06 0 0 8.06 0 18c0 13.5 18 30 18 30S36 31.5 36 18C36 8.06 27.94 0 18 0z" fill="#4540e1"/>
        <circle cx="18" cy="18" r="7" fill="white"/>
      </svg>`,
      iconSize: [36, 48],
      iconAnchor: [18, 48]
    });

    this.marker = L.marker([this.initialLat, this.initialLng], {
      icon: pinIcon,
      draggable: true
    }).addTo(this.map);

    this.marker.on('dragend', () => {
      const pos = this.marker.getLatLng();
      this.scheduleReverseGeocode(pos.lat, pos.lng);
    });

    this.map.on('click', (e: any) => {
      this.marker.setLatLng(e.latlng);
      this.scheduleReverseGeocode(e.latlng.lat, e.latlng.lng);
    });

    if (this.initialAddress) {
      this.selectedAddress = this.initialAddress;
    } else {
      this.scheduleReverseGeocode(this.initialLat, this.initialLng);
    }

    setTimeout(() => this.map.invalidateSize(), 200);
  }

  private flyToLocation(lat: number, lng: number) {
    this.map.flyTo([lat, lng], 16, { animate: true, duration: 0.8 });
    this.marker.setLatLng([lat, lng]);
  }

  private scheduleReverseGeocode(lat: number, lng: number) {
    clearTimeout(this.reverseTimeout);
    this.reverseTimeout = setTimeout(() => this.reverseGeocode(lat, lng), 500);
  }

  private async reverseGeocode(lat: number, lng: number) {
    try {
      const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`;
      const res = await fetch(url, { headers: { 'Accept-Language': 'en' } });
      const data = await res.json();
      this.selectedAddress = data.display_name || `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
    } catch {
      this.selectedAddress = `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
    }
  }

  getCurrentLocation() {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const { latitude, longitude } = pos.coords;
        this.flyToLocation(latitude, longitude);
        this.scheduleReverseGeocode(latitude, longitude);
      },
      (err) => {
        this.toaster.error(PROFILE_MESSAGES.ADDRESS.GEOLOGICAL_ERROR || err?.message);
      }
    );
  }

  saveAddress() {
    if (!this.houseNumberControl.value?.trim() || !this.landmarkControl.value?.trim()) {
      this.houseNumberControl.markAsTouched();
      this.landmarkControl.markAsTouched();
      return;
    }

    if (this.showCustomLabelInput && !this.nicknameControl.value?.trim()) {
      this.nicknameControl.markAsTouched();
      return;
    }

    const label = this.showCustomLabelInput
      ? this.nicknameControl.value!.trim()
      : this.selectedLabel ?? '';

    const pos = this.marker.getLatLng();

    this.addressSelected.emit({
      id: this.editData?.id,
      houseFlatNo: this.houseNumberControl.value,
      landmark: this.landmarkControl.value ?? '',
      label: label,
      latitude: pos.lat,
      longitude: pos.lng,
      displayName: this.selectedAddress
    });
  }

  onChangeLocation() {
    this.changeLocation.emit(); 
  }

  onClose() {
    this.closeModal.emit();
  }
}