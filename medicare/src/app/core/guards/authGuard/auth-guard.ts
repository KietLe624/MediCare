import { CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../../../features/auth/services/auth.service';

export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const auth = inject(AuthService);

  // Nếu đã đăng nhập, cho phép đi tiếp
  if (auth.isLoggedIn()) {
    return true;
  }

  // Nếu chưa đăng nhập, trả về UrlTree chuyển hướng ngay lập tức về /login
  const router = inject(Router);
  return router.createUrlTree(['/login']);
};
