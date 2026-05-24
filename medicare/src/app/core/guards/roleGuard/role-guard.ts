// src/app/core/guards/role.guard.ts
import { CanActivateFn, ActivatedRouteSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../../../features/auth/services/auth.service';
import { Router } from '@angular/router';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const allowed: string[] = route.data['roles'] ?? [];

  if (allowed.length === 0 || allowed.includes(auth.role() ?? '')) return true;

  // Không đủ quyền về dashboard
  const router = inject(Router);
  return router.createUrlTree(['/login']);
};
