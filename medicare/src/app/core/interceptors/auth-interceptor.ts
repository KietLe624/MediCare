import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthResponse, RefreshTokenRequest, STORAGE_KEYS } from '../../features/auth/models/auth.model';
import { HttpClient } from '@angular/common/http';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);
const REFRESH_URL = '/api/auth/refresh';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const http = inject(HttpClient);
  const router = inject(Router);

  const hasSessionTokens = !!sessionStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
    || !!sessionStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
  const storage = hasSessionTokens ? sessionStorage : localStorage;

  // Lấy access token từ storage
  const accessToken = storage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  const authReq = accessToken
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        return throwError(() => error);
      }
      //
      if (req.url.includes(REFRESH_URL)) {
        localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        sessionStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
        sessionStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        router.navigate(['/login']);
        return throwError(() => error);
      }
      // Lỗi 401, gọi refresh token
      const refreshToken = storage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
      if (!refreshToken) {
        localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
        localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        sessionStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
        sessionStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
        router.navigate(['/login']);
        return throwError(() => error);
      }
      // Tránh gọi đồng thời nhiều request refresh token
      if (!isRefreshing) {
        isRefreshing = true;
        refreshTokenSubject.next(null);

        const body: RefreshTokenRequest = { refreshToken };
        return http.post<AuthResponse>(REFRESH_URL, body).pipe(
          switchMap((res) => {
            storage.setItem(STORAGE_KEYS.ACCESS_TOKEN, res.accessToken);
            storage.setItem(STORAGE_KEYS.REFRESH_TOKEN, res.refreshToken);
            isRefreshing = false;
            refreshTokenSubject.next(res.accessToken);
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${res.accessToken}` }
            });
            return next(retryReq);
          }),
          catchError((refreshErr: HttpErrorResponse) => {
            isRefreshing = false;
            // Xoá token cũ và điều hướng về login
            localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
            localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
            sessionStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
            sessionStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
            router.navigate(['/login']);
            return throwError(() => refreshErr);
          })
        );
      }
      // Refresh token
      return refreshTokenSubject.pipe(
        filter((token): token is string => token !== null),
        take(1),
        switchMap((token) => {
          const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
          return next(retryReq);
        })
      );
    })
  );
};
