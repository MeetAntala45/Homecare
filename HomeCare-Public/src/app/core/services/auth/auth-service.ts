import { Injectable } from '@angular/core';
import { ISendOtpRequest } from '../../models/auth/ISendOtpRequest';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { IVerifyOtpRequest } from '../../models/auth/IVerifyOtpRequest';
import { IVerifyOtpResponse } from '../../models/auth/IVerifyResponse';
import { ISendOtpResponse } from '../../models/auth/ISendOtpResponse';
import { BehaviorSubject, Observable } from 'rxjs';
import { API_BASE_URL, CUSTOMER_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private http: HttpClient) {}

  api_url = `${API_BASE_URL}/api/customer-auth`;

  private isLoggedInSubject = new BehaviorSubject<boolean>(
    !!localStorage.getItem(CUSTOMER_ACCESS_TOKEN_KEY)
  );
  isLoggedIn$ = this.isLoggedInSubject.asObservable();

  sendOtp(req: ISendOtpRequest) {
    return this.http.post<IApiResponse<ISendOtpResponse>>(`${this.api_url}/send-otp`, req, {
      withCredentials: true,
    });
  }

  verifyOtp(req: IVerifyOtpRequest) {
    return this.http.post<IApiResponse<IVerifyOtpResponse>>(`${this.api_url}/verify-otp`, req, {
      withCredentials: true,
    });
  }

  logout() {
    return this.http.post<IApiResponse<string>>(
      `
      ${this.api_url}/logout`,
      {},
      { withCredentials: true }
    );
  }

  refreshToken() {
    return this.http.post<IApiResponse<IVerifyOtpResponse>>(
      `${this.api_url}/refresh`,
      {},
      { withCredentials: true }
    );
  }

  isLoggedIn() {
    return !!localStorage.getItem(CUSTOMER_ACCESS_TOKEN_KEY);
  }

  setLoggedIn(value: boolean) {
    this.isLoggedInSubject.next(value);
  }
  validateReferralCode(
    referralCode: string
  ): Observable<IApiResponse<{ isValid: boolean; errorMessage: string | null }>> {
    return this.http.post<IApiResponse<{ isValid: boolean; errorMessage: string | null }>>(
      `${this.api_url}/validate-referral-code`,
      { referralCode }
    );
  }
}
