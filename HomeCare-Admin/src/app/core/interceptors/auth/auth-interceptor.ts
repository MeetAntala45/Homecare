import { HttpInterceptorFn, HttpErrorResponse, HttpEvent } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, Observable, BehaviorSubject, filter, take } from 'rxjs';
import { AuthService } from '../../services/auth/auth-service';
import { ADMIN_ACCESS_TOKEN_KEY } from '../../constants/environment-config';

const ACCESS_TOKEN_KEY = ADMIN_ACCESS_TOKEN_KEY;

export const authInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<any>> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = localStorage.getItem(ACCESS_TOKEN_KEY);

  const isAuthApi = req.url.includes('/auth/login') || req.url.includes('/auth/refresh');

  let authReq = req.clone({ withCredentials: true });

  if (token && !isAuthApi) {
    authReq = req.clone({
      withCredentials: true,
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if ((error.status !== 401 && error.status !== 403) || isAuthApi) {
        return handleGeneralError(error);
      }

      if (authService.isRefreshing) {
        return authService.refreshTokenSubject.pipe(
          filter((token) => token !== null),
          take(1),
          switchMap((newToken) =>
            next(
              req.clone({
                withCredentials: true,
                setHeaders: {
                  Authorization: `Bearer ${newToken!}`,
                },
              })
            )
          ),
          catchError((err) => {
            return handleLogout(authService, router, err);
          })
        );
      }

      authService.isRefreshing = true;
      authService.refreshTokenSubject.next(null);

      return authService.refresh().pipe(
        switchMap((res) => {
          authService.isRefreshing = false;

          if (!res?.success || !res?.data?.accessToken) {
            return throwError(() => new Error('Invalid refresh response'));
          }

          const newToken = res.data.accessToken;

          localStorage.setItem(ACCESS_TOKEN_KEY, newToken);
          authService.refreshTokenSubject.next(newToken);

          return next(
            req.clone({
              withCredentials: true,
              setHeaders: { Authorization: `Bearer ${newToken}` },
            })
          );
        }),
        catchError((err: HttpErrorResponse) => {
          authService.isRefreshing = false;
          return handleLogout(authService, router, err);
        })
      );
    })
  );
};

function handleGeneralError(error: HttpErrorResponse) {
  let message = 'Something went wrong. Please try again.';

  if (error.error instanceof ErrorEvent) {
    message = error.error.message;
  } else {
    switch (error.status) {
      case 0:
        message = 'Server unreachable. Check network.';
        break;
      case 400:
        message = error.error?.message || 'Bad request';
        break;
      case 403:
        message = 'Access denied';
        break;
      case 404:
        message = 'Resource not found';
        break;
      case 500:
        message = 'Internal server error';
        break;
      default:
        message = error.error?.message || `Error ${error.status}`;
    }
  }

  return throwError(() => new Error(message));
}

function handleLogout(authService: AuthService, router: Router, error: any) {
  localStorage.removeItem(ADMIN_ACCESS_TOKEN_KEY);
  authService.isRefreshing = false;
  authService.refreshTokenSubject.next(null);
  router.navigate(['/login']);
  return throwError(() => error);
}
