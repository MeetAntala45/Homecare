import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ICreateBookingRequest, ICreateBookingResponse } from '../../models/checkout/booking';
import { API_BASE_URL } from '../../constants/environment-config';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';

@Injectable({
  providedIn: 'root',
})
export class Booking {
  private readonly apiUrl = `${API_BASE_URL}/api/booking`;

  constructor(private http: HttpClient) {}

  createBooking(dto: ICreateBookingRequest): Observable<IApiResponse<ICreateBookingResponse>> {
    return this.http.post<IApiResponse<ICreateBookingResponse>>(`${this.apiUrl}/create`, dto);
  }
}
