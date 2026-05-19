import { Injectable } from '@angular/core';
import { ISendOtpRequest } from '../../models/auth/ISendOtpRequest';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/api-response/api-response';
import { IVerifyOtpRequest } from '../../models/auth/IVerifyOtpRequest';
import { IVerifyOtpResponse } from '../../models/auth/IVerifyResponse';
import { ISendOtpResponse } from '../../models/auth/ISendOtpResponse';
import { BehaviorSubject } from 'rxjs';
import { API_BASE_URL, SERVICE_PARTNER_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private http: HttpClient) { }

  api_url = `${API_BASE_URL}/api/service-partner-auth`;

  private isLoggedInSubject = new BehaviorSubject<boolean>(
    !!localStorage.getItem(SERVICE_PARTNER_ACCESS_TOKEN_KEY)
  );
  isLoggedIn$ = this.isLoggedInSubject.asObservable();

  sendOtp(req: ISendOtpRequest) {
    return this.http.post<IApiResponse<ISendOtpResponse>>(
      `${this.api_url}/send-otp`, req,
      { withCredentials: true }
    );
  }

  verifyOtp(req: IVerifyOtpRequest) {
    return this.http.post<IApiResponse<IVerifyOtpResponse>>(
      `${this.api_url}/verify-otp`, req,
      { withCredentials: true }
    );
  }

  getPartnerId(): number {
    return Number(localStorage.getItem('partner_id'));
  }

  logout() {
    return this.http.post<IApiResponse<string>>(
      `${this.api_url}/logout`,
      {},
      { withCredentials: true }
    );
  }

  refreshToken() {
    return this.http.post<IApiResponse<IVerifyOtpResponse>>(
      `${this.api_url}/refresh`, {},
      { withCredentials: true }
    );
  }

  isLoggedIn() {
    return !!localStorage.getItem(SERVICE_PARTNER_ACCESS_TOKEN_KEY);
    this.setLoggedIn(true);
  }

  setLoggedIn(value: boolean) {
    this.isLoggedInSubject.next(value);
  }
  
}
