import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { IMyBooking } from '../../models/my-bookings/IMyBookings';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class MyBookings {
  constructor(private http: HttpClient) { }

  private getCustomerIdFromToken(): number | null {
    const token = localStorage.getItem('customer_access_token');
    if (!token) return null;

    const payload = JSON.parse(atob(token.split('.')[1]));

    return Number(
      payload?.customerId ||
      payload?.CustomerId ||
      payload?.nameid ||
      payload?.sub ||
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    ) || null;
  }

  bookingsByCustomerId() {
    const customerId = this.getCustomerIdFromToken();

    if (customerId === null) {
      throw new Error('Customer ID not found in token');
    }

    return this.http.get<IApiResponse<IMyBooking[]>>(
      `${API_BASE_URL}/api/my-bookings/${customerId}`
    );
  }
}