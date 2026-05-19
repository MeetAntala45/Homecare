// core/services/tracking/tracking.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/api-response/api-response';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class TrackingService {
  private base = `${API_BASE_URL}/api/tracking`;

  constructor(private http: HttpClient) {}

  updateLocation(bookingId: number, latitude: number, longitude: number) {
    return this.http.post<IApiResponse<string>>(`${this.base}/update-location`, {
      bookingId,
      latitude,
      longitude
    });
  }
}