import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse, HttpEvent } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, Observable, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../../services/auth/auth-service';

import { Toaster } from '../../services/toaster/toaster';
import { SERVICE_PARTNER_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

const ACCESS_TOKEN_KEY = SERVICE_PARTNER_ACCESS_TOKEN_KEY;

const PUBLIC_URLS = [
  '/api/service-partner-auth/send-otp',
  '/api/service-partner-auth/verify-otp',
  '/api/service-partner-auth/refresh',
];

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toaster = inject(Toaster);

  const isPublicUrl = PUBLIC_URLS.some(url => req.url.includes(url));
  const token = localStorage.getItem(ACCESS_TOKEN_KEY);

  let authReq = req;
  if (token && !isPublicUrl) {
    authReq = req.clone({
      withCredentials: true,
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {

      if (error.status !== 401 || isPublicUrl) {
        return throwError(() => error);
      }

      if (error.error?.message === 'Your account has been blocked.') {
        isRefreshing = false;
        localStorage.removeItem(ACCESS_TOKEN_KEY);
        authService.setLoggedIn(false);
        toaster.error(error.error.message);
        router.navigate(['/service-partner/dashboard']);
        return throwError(() => error);
      }

      if (isRefreshing) {
        return refreshTokenSubject.pipe(
          filter((token) => token !== null),
          take(1),
          switchMap(newToken => next(authReq.clone({
            setHeaders: { Authorization: `Bearer ${newToken!}` }
          })))
        );
      }

      isRefreshing = true;
      refreshTokenSubject.next(null);

      return authService.refreshToken().pipe(
        switchMap(res => {
          isRefreshing = false;

          if (!res.success || !res.data) {
            throw new Error('Sign in required');
          }

          const newToken = res.data.accessToken;
          localStorage.setItem(ACCESS_TOKEN_KEY, newToken);
          refreshTokenSubject.next(newToken);

          return next(authReq.clone({
            setHeaders: { Authorization: `Bearer ${newToken}` }
          }));
        }),

        catchError(err => {
          isRefreshing = false;
          refreshTokenSubject.next(null);

          localStorage.removeItem(ACCESS_TOKEN_KEY);
          router.navigate(['/service-partner/dashboard']);

          return throwError(() => err);
        })
      );
    })
  );
};