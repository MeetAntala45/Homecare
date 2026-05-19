import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IRecentSearch } from '../../../../core/models/profile/IRecentSearch';
import { ProfileService } from '../../../../core/services/profile/profile-service';
import { IAddRecentSearch } from '../../../../core/models/profile/IAddRecentSearch';
import { PROFILE_MESSAGES } from '../../../../core/constants/profile-messages';
import { Toaster } from '../../../../core/services/toaster/toaster';


export interface INominatimResult {
  display_name: string;
  lat: string;
  lon: string;
}
@Component({
  selector: 'app-location-search-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './location-search-modal.html',
  styleUrl: './location-search-modal.css'
})
export class LocationSearchModal implements OnInit {

  @Output() locationSelected = new EventEmitter<{ lat: number; lng: number; display_name: string }>();
  @Output() closeModal = new EventEmitter<void>();

  searchQuery = '';
  searchResults: INominatimResult[] = [];
  isSearching = false;
  searchFocused = false;
  isLoadingRecents = false;
  private searchTimeout: any;

  recentSearches: IRecentSearch[] = [];

  constructor(
    private profileService: ProfileService, 
    private toaster: Toaster
  ) {}

  ngOnInit() {
    this.loadRecentSearches();
  }

  loadRecentSearches() {
    this.isLoadingRecents = true;
    this.profileService.getRecentSearches().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.recentSearches = res.data;
        }
        this.isLoadingRecents = false;
      },
      error: () => this.isLoadingRecents = false
    });
  }

  private saveRecentSearch(lat: number, lng: number, displayName: string) {
    const dto: IAddRecentSearch = {
      displayName: displayName,
      latitude: lat,
      longitude: lng
    };
    this.profileService.addRecentSearch(dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.loadRecentSearches();
        }
      }
    });
  }

  onSearchInput() {
    clearTimeout(this.searchTimeout);
    if (!this.searchQuery.trim()) {
      this.searchResults = [];
      return;
    }
    this.isSearching = true;
    this.searchTimeout = setTimeout(() => {
      this.fetchSearchResults(this.searchQuery);
    }, 400);
  }

  async fetchSearchResults(query: string) {
    try {
      const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=5&addressdetails=1`;
      const res = await fetch(url, { headers: { 'Accept-Language': 'en' } });
      const data = await res.json();
      this.searchResults = data;
    } catch {
      this.searchResults = [];
    } finally {
      this.isSearching = false;
    }
  }

  clearSearch() {
    this.searchQuery = '';
    this.searchResults = [];
  }

  selectSearchResult(result: INominatimResult) {
    const lat = parseFloat(result.lat);
    const lng = parseFloat(result.lon);
    this.saveRecentSearch(lat, lng, result.display_name);

    this.locationSelected.emit({ lat, lng, display_name: result.display_name });
  }

  selectRecent(recent: IRecentSearch) {
    this.saveRecentSearch(recent.latitude, recent.longitude, recent.displayName);

    this.locationSelected.emit({
      lat: recent.latitude,
      lng: recent.longitude,
      display_name: recent.displayName
    });
  }

  useCurrentLocation() {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      async (pos) => {
        const { latitude, longitude } = pos.coords;
        const display_name = await this.reverseGeocode(latitude, longitude);
        this.locationSelected.emit({ lat: latitude, lng: longitude, display_name });
      },
      (err) => {
        this.toaster.error(PROFILE_MESSAGES.ADDRESS.GEOLOGICAL_ERROR || err?.message)
      }
    );
  }

  async reverseGeocode(lat: number, lng: number): Promise<string> {
    try {
      const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`;
      const res = await fetch(url, { headers: { 'Accept-Language': 'en' } });
      const data = await res.json();
      return data.display_name || `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
    } catch {
      return `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
    }
  }

  close() {
    this.closeModal.emit();
  }
}