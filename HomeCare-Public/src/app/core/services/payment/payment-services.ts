import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { ICheckoutSession } from '../../models/payments/ICheckoutSession';
import { ICheckoutRequest } from '../../models/payments/ICheckoutRequest';
import { IBookingSuccess } from '../../models/payments/IBookingSuccess';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {

  private api_url = `${API_BASE_URL}/api/payment`;

  constructor(private http: HttpClient) {}

  createCheckoutSession(dto: ICheckoutRequest): Observable<IApiResponse<ICheckoutSession>> {
    return this.http.post<IApiResponse<ICheckoutSession>>(
      `${this.api_url}/create-checkout-session`, dto
    );
  }

  getBookingSuccessDetails(bookingId: number): Observable<IApiResponse<IBookingSuccess>> {
    return this.http.get<IApiResponse<IBookingSuccess>>(
        `${this.api_url}/booking/${bookingId}/success-details`
    );
  }

  downloadInvoice(bookingId: number): Observable<Blob> {
    return this.http.get(
      `${this.api_url}/booking/${bookingId}/invoice`,
      { responseType: 'blob' }
    );
  }

}