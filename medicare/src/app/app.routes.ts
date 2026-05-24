import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/components/login/login';
import { ForgotPassword } from './features/auth/components/forgot-password/forgot-password';
import { roleGuard } from './core/guards/roleGuard/role-guard';
import { noAuthGuard } from './core/guards/noAuthGuard/no-auth-guard';
import { authGuard } from './core/guards/authGuard/auth-guard';
import { OverviewComponent } from './features/dashboard/components/overview/overview';
import { AdminLayout } from './layouts/pages/admin-layout/admin-layout';

export const routes: Routes = [
  {
    path: '',
    canActivate: [noAuthGuard],
    component: LoginComponent
  },
  {
    path: 'login',
    canActivate: [noAuthGuard],
    component: LoginComponent
  },
  {
    path: 'forgot-password',
    canActivate: [noAuthGuard],
    component: ForgotPassword
  },
  // {
  //   path: '/login',
  //   canActivate: [noAuthGuard],
  //   component: LoginComponent
  // }

  // auth routes
  {
    path: 'dashboard',
    canActivate: [authGuard, roleGuard],
    component: AdminLayout,
    children: [
      {
        path: 'overview',
        component: OverviewComponent,
      },
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'overview',
      },
    ],
  }

];
