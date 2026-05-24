import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../features/auth/services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  menuItems = [
    {
      icon: 'dashboard',
      label: 'Dashboard',
      route: '/dashboard'
    },
    {
      icon: 'person',
      label: 'Bệnh nhân',
      route: '/patients'
    },
    {
      icon: 'event',
      label: 'Lịch hẹn',
      route: '/appointments'
    },
    {
      icon: 'medical_services',
      label: 'Nhân viên y tế',
      route: '/staff'
    }
  ];

  private authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
