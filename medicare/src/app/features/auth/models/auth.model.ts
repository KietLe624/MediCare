export interface LoginRequest {
  userName: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
  phoneNumber?: string;
  role?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  email: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// ─── RESPONSE MODELS ──────────────────────────────────────────

export interface AuthResponse {
  fullName: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  expiration: string;
}

export interface UserInfoResponse {
  id: number;
  userName: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  roles: string[];
}

// ─── LOCAL STORAGE KEYS ───────────────────────────────────────
export const STORAGE_KEYS = {
  ACCESS_TOKEN: 'mc_access_token',
  REFRESH_TOKEN: 'mc_refresh_token',
  USER: 'mc_user'
} as const;
