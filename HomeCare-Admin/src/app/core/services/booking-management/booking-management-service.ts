import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/auth/api.response';
import { IDropdownOption } from '../../models/booking-management/booking-managment-filter';
import { ICustomerBookingDetailPaged } from '../../models/booking-management/customer-booking-detail';
import { ICustomerBookingSummaryPaged } from '../../models/booking-management/customer-booking-summary';
import { ICustomerPageRequest } from '../../models/booking-management/customer-page-request';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class CustomerBookingService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/api/admin/bookings`;

  getServiceTypes(): Observable<IApiResponse<IDropdownOption[]>> {
    return this.http.get<IApiResponse<IDropdownOption[]>>(`${this.apiUrl}/service-types`);
  }

  getCustomerSummaries(params: HttpParams): Observable<IApiResponse<ICustomerBookingSummaryPaged>> {
    return this.http.get<IApiResponse<ICustomerBookingSummaryPaged>>(
      `${this.apiUrl}/customer-summaries`,
      { params }
    );
  }

  deleteCustomerBookings(
    customerId: number,
    params: HttpParams
  ): Observable<IApiResponse<boolean>> {
    return this.http.delete<IApiResponse<boolean>>(`${this.apiUrl}/customers/${customerId}`, {
      params,
    });
  }
  getCustomerBookingDetails(
    customerId: number,
    params: HttpParams
  ): Observable<IApiResponse<ICustomerBookingDetailPaged>> {
    return this.http.get<IApiResponse<ICustomerBookingDetailPaged>>(
      `${this.apiUrl}/customers/${customerId}/details`,
      { params }
    );
  }

  getCustomerPage(request: ICustomerPageRequest): Observable<IApiResponse<number>> {
    return this.http.get<IApiResponse<number>>(
      `${this.apiUrl}/page-of/${request.customerId}/${request.paymentMethod}`,
      { params: new HttpParams().set('pageSize', request.pageSize.toString()) }
    );
  }
}
