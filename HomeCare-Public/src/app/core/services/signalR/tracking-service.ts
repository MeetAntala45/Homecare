// core/services/tracking/tracking.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { API_BASE_URL } from '../../constants/environment-config';

export interface ILocationResponse {
  latitude: number;
  longitude: number;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerTrackingService {
  private base = `${API_BASE_URL}/api/tracking`;

  constructor(private http: HttpClient) {}

  getLastLocation(bookingId: number): Observable<IApiResponse<ILocationResponse>> {
    return this.http.get<IApiResponse<ILocationResponse>>(
      `${this.base}/last-location/${bookingId}`
    );
  }

}