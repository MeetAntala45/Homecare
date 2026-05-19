import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../models/paged-result';
import { ApiResponse } from '../../models/profile/profile.model';
import { ICustomerFilterRequest } from '../../models/customer-user/ICustomerFilterRequest';
import { ICustomerList } from '../../models/customer-user/ICustomerList';
import { ICustomerRequest } from '../../models/customer-user/ICustomerRequest';
import { IDropdownOption } from '../../models/customer-user/IDropdownOption';
import { IFilterPagedResult } from '../../models/shared-components/IFilterPagedResult';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class CustomerManagementService {
  api_url = `${API_BASE_URL}/api/customer`;
  booking_url = `${API_BASE_URL}/api/booking-management`;

  constructor(private http: HttpClient) {}

  getCustomerList(filter: ICustomerFilterRequest) {
    return this.http.get<ApiResponse<IFilterPagedResult<ICustomerList>>>(`${this.api_url}/customer-list`,
      { params: filter as any }
    );
  }

  blockCustomer(id: number) {
    return this.http.patch<ApiResponse<string>>(`${this.api_url}/block/${id}`, {});
  }

  deleteCustomer(id: number) {
    return this.http.delete<ApiResponse<string>>(`${this.api_url}/delete/${id}`);
  }

  addCustomer(data: ICustomerRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.api_url}/add-customer`, data);
  }

  getCustomerById(id: number) {
    return this.http.get<ApiResponse<ICustomerList>>(`${this.api_url}/${id}`);
  }

  // addCustomer(data: ICustomerRequest): Observable<ApiResponse<string>> {
  //   return this.http.post<ApiResponse<string>>(`${this.api_url}/add-customer`, data);
  // }

  // blockCustomer(id: number) {
  //   return this.http.patch<ApiResponse<string>>(`${this.api_url}/block/${id}`, {});
  // }

  unblockCustomer(id: number) {
    return this.http.patch<ApiResponse<string>>(`${this.api_url}/unblock/${id}`, {});
  }

  // deleteCustomer(id: number) {
  //   return this.http.delete<ApiResponse<string>>(`${this.api_url}/delete/${id}`);
  // }

  activateCustomer(id:number)
  {
    return this.http.patch<ApiResponse<string>>(`${this.api_url}/active/${id}`, {});
  }

  getServiceTypes(): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(`${this.api_url}/service-types`);
  }

  getCustomerBookings(id: number, params: any) {
    return this.http.get<ApiResponse<PagedResult<any>>>(`${this.api_url}/${id}/bookings`, {
      params,
    });
  }

  getAvailablePartners(bookingId: number) {
    return this.http.get<ApiResponse<any[]>>(`${this.booking_url}/${bookingId}/available-partners`);
  }

  changeExpert(bookingId: number, newPartnerId: number): Observable<ApiResponse<string>> {
    return this.http.patch<ApiResponse<string>>(`${this.booking_url}/${bookingId}/change-expert`, {
      newPartnerId,
    });
  }

  completeBooking(bookingId: number): Observable<ApiResponse<string>> {
    return this.http.patch<ApiResponse<string>>(`${this.booking_url}/${bookingId}/complete`, {});
  }

  cancelBooking(bookingId: number, reason: string | null): Observable<ApiResponse<string>> {
    return this.http.patch<ApiResponse<string>>(`${this.booking_url}/${bookingId}/cancel`, {
      reason,
    });
  }

  deleteBooking(bookingId: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.booking_url}/${bookingId}/delete`);
  }
}
