import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { ICustomerProfile } from '../../models/profile/ICustomerProfile';
import { IUpdateMobileRequest } from '../../models/profile/IUpdateMobileRequest';
import { IVerifyEmailChangeRequest } from '../../models/profile/IVerifyEmailChangeRequest';
import { IEmailChangeOtpResponse } from '../../models/profile/IEmailChangeOtpResponse';
import { IEmailChangeRequest } from '../../models/profile/IEmailChangeRequest';
import { ApiResponse } from '../../models/service-partner/service-partner';
import { Observable } from 'rxjs';
import { IAddressRequest } from '../../models/profile/IAddressRequest';
import { IAddRecentSearch } from '../../models/profile/IAddRecentSearch';
import { IRecentSearch } from '../../models/profile/IRecentSearch';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  constructor(private http: HttpClient) {}

  private api_url = `${API_BASE_URL}/api/customer/profile`;

  getProfile(): Observable<ApiResponse<ICustomerProfile>> {
    return this.http.get<ApiResponse<ICustomerProfile>>(this.api_url);
  }

  updateMobile(mobileNumber: string): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.api_url}/mobile`, { mobileNumber });
  }

  requestEmailChange(req: IEmailChangeRequest) {
    return this.http.post<IApiResponse<IEmailChangeOtpResponse>>(
      `${this.api_url}/email/request-change`,
      req
    );
  }

  verifyEmailChange(req: IVerifyEmailChangeRequest) {
    return this.http.post<IApiResponse<string>>(`${this.api_url}/email/verify-otp`, req);
  }

  addAddress(dto: IAddressRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.api_url}/address`, dto);
  }

  editAddress(addressId: number, dto: IAddressRequest): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.api_url}/address/${addressId}`, dto);
  }

  deleteAddress(addressId: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.api_url}/address/${addressId}`);
  }

  getAddressLabels(): Observable<ApiResponse<string[]>> {
    return this.http.get<ApiResponse<string[]>>(`${this.api_url}/address/labels`);
  }

  addRecentSearch(dto: IAddRecentSearch): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.api_url}/address/add-recent-search`, dto);
  }

  getRecentSearches(): Observable<ApiResponse<IRecentSearch[]>> {
    return this.http.get<ApiResponse<IRecentSearch[]>>(`${this.api_url}/address/recent-searches`);
  }
  shareReferral(recipientEmail: string): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${API_BASE_URL}/api/referral/share`, {
      recipientEmail,
    });
  }
}
