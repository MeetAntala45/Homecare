import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom, Observable, tap } from 'rxjs';
import { ILoginResponse } from '../../models/auth/login.response';
import { ILoginRequest } from '../../models/auth/login.request';
import { IForgotPasswordRequest } from '../../models/auth/forgot-password.request';
import { IApiResponse } from '../../models/auth/api.response';
import { IResetPasswordRequest } from '../../models/auth/reset-password.request';
import { ADMIN_USER_ROLE, API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly baseUrl = `${API_BASE_URL}/api/auth`;
  private readonly ROLE_KEY = ADMIN_USER_ROLE;

  role = signal<string>(localStorage.getItem(this.ROLE_KEY) ?? '');
  isRefreshing = false;
  refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private http: HttpClient) { }

  login(request: ILoginRequest): Observable<IApiResponse<ILoginResponse>> {
    return this.http.post<IApiResponse<ILoginResponse>>(
      `${this.baseUrl}/login`,
      request,
      { withCredentials: true }
    );
  }

  refresh(): Observable<IApiResponse<ILoginResponse>> {
    return this.http.post<IApiResponse<ILoginResponse>>(
      `${this.baseUrl}/refresh`,
      {},
      { withCredentials: true }
    );
  }

  logout(): Observable<IApiResponse<string>> {
    return this.http.post<IApiResponse<string>>(
      `${this.baseUrl}/logout`,
      {},
      { withCredentials: true }
    );
  }

  forgotPassword(request: IForgotPasswordRequest): Observable<IApiResponse<string>> {
    return this.http.post<IApiResponse<string>>(
      `${this.baseUrl}/forgot-password`,
      request
    );
  }

  validateResetToken(token: string): Observable<IApiResponse<string>> {
    return this.http.get<IApiResponse<string>>(
      `${this.baseUrl}/validate-reset-token?token=${token}`
    );
  }

  resetPassword(request: IResetPasswordRequest): Observable<IApiResponse<string>> {
    return this.http.post<IApiResponse<string>>(
      `${this.baseUrl}/reset-password`,
      request
    );
  }
}