import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import {
  LoginRequest, RegisterRequest, AuthResponse,
  UserInfoResponse, ForgotPasswordRequest, ChangePasswordRequest,
  STORAGE_KEYS
} from '../models/auth.model';
import { Router } from '@angular/router';


@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private authUrl = 'http://localhost:5034/api/Auth';
  private router = inject(Router);

  // ── State (Signals) ─────────────────────────────────────────
  private _user = signal<UserInfoResponse | null>(this.#loadUser());
  private _hasToken = signal(this.#loadHasToken());
  private _loading = signal(false);

  readonly user = this._user.asReadonly();
  readonly loading = this._loading.asReadonly();

  readonly isLoggedIn = computed(() => this._hasToken() || !!this._user());

  readonly role = computed(() => this._user()?.roles[0] ?? null);
  readonly fullName = computed(() => this._user()?.fullName ?? '');

  // Login method
  login(body: LoginRequest): Observable<AuthResponse> {
    this._loading.set(true);
    return this.http.post<AuthResponse>(`${this.authUrl}/login`, body).pipe(
      tap(res => {
        localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, res.accessToken);
        localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, res.refreshToken);
        this._hasToken.set(true);
        this.#fetchMe();
      })
    );
  }
  // Logout method
  logout(): void {
    this.#clear();
    this.router.navigate(['/']);
  }

  // fogot password
  forgotPassword(body: ForgotPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.authUrl}/forgot-password`, body);
  }

  // Helper
  #fetchMe() {
    this.http.get<UserInfoResponse>(`${this.authUrl}/me`).subscribe({
      next: u => {
        this._user.set(u);
        this._loading.set(false);
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(u));
        this.#redirectByRole(u.roles[0]);
      },
      error: () => { this.#clear(); this._loading.set(false); }
    });
  }

  #redirectByRole(role: string): void {
    const map: Record<string, string> = {
      Admin: '/dashboard/overview',
      Doctor: '/dashboard/me',
      Nurse: '/dashboard/overview',
      Receptionist: '/dashboard/overview',
      Patient: '/patient/me'
    };
    this.router.navigate([map[role] ?? '/dashboard/overview']);
  }

  #clear(): void {
    this._user.set(null);
    this._hasToken.set(false);
    Object.values(STORAGE_KEYS).forEach(k => {
      localStorage.removeItem(k);
      sessionStorage.removeItem(k);
    });
  }

  #loadUser(): UserInfoResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEYS.USER);
      return raw ? JSON.parse(raw) : null;
    } catch { return null; }
  }

  #loadHasToken(): boolean {
    const hasSessionToken = !!sessionStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    const hasLocalToken = !!localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    return hasSessionToken || hasLocalToken;
  }
}
