import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { IApiResponse } from '../../models/api-response/api-response';
import { IMyJobCalendarItem, IMyJobCalendarRequest, IMyJobRequest, IMyJobs } from '../../models/my-jobs/my-jobs';
import { Observable } from 'rxjs';
import { IPaymentPagedResult } from '../../models/payments-and-transations/IPaymentPagedResult';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class MyJobsService {
  private apiUrl = `${API_BASE_URL}/api/service-partner/bookings`;

  constructor(private http: HttpClient) { }

  getBookingsByPartnerId(payload: IMyJobRequest) {
    let bookingDate = '';

    if (payload.bookingDate) {
      const d = new Date(payload.bookingDate);

      bookingDate = `${d.getFullYear()}-${(d.getMonth() + 1)
        .toString()
        .padStart(2, '0')}-${d.getDate().toString().padStart(2, '0')}`;
    }
    return this.http.get<IApiResponse<IPaymentPagedResult<IMyJobs>>>(
      this.apiUrl,
      {
        params: {
          pageNumber: payload.pageNumber,
          pageSize: payload.pageSize,
          sortBy: payload.sortBy || '',
          sortOrder: payload.sortOrder || '',
          status: payload.status || 'Pending',
          serviceName: payload.serviceName || '',
          customerName: payload.customerName || '',
          bookingDate: bookingDate,
          paymentMethod: payload.paymentMethod || '',
          minAmount: payload.minAmount ?? '',
          maxAmount: payload.maxAmount ?? ''
        }
      }
    );
  }

  getCalendarJobs(req: IMyJobCalendarRequest): Observable<IApiResponse<IMyJobCalendarItem[]>> {
    return this.http.get<IApiResponse<IMyJobCalendarItem[]>>(
      `${this.apiUrl}/calendar`,
      { params: { year: req.year, month: req.month } }
    );
  }

  getBookingPage(bookingId: number, pageSize: number, status: string):
    Observable<IApiResponse<number>> {
    return this.http.get<IApiResponse<number>>(
      `${this.apiUrl}/booking-page/${bookingId}`, { params : {pageSize: pageSize, status: status} });
  }

  getAllServices() {
    return this.http.get<any>(`${this.apiUrl}/services`);
  }
}
