import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
// Services
import { ToastService } from '../../services/toast-info';

@Component({
  selector: 'app-toast',
  imports: [CommonModule],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
})
export class ToastComponent {
  toastService = inject(ToastService);
}
