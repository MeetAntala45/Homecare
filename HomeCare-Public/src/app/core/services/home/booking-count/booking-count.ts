import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../../models/apiResponse/IApiResponse';
import { API_BASE_URL } from '../../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class BookingCount {
  api_url = `${API_BASE_URL}/api/customer`

  constructor(private http: HttpClient) { }

  getAllServiceBookingCounts() {
    return this.http.get<IApiResponse<Record<number, number>>>(
      `${API_BASE_URL}/api/booking/counts`
    );
  }
}
