import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ForgotPasswordRequest } from '../../models/auth.model';

@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
  constructor(private authService: AuthService) { }

  form: ForgotPasswordRequest = {
    email: ''
  };

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.form.email) {
      this.errorMessage = 'Vui lòng nhập email.';
      return;
    }

    this.isSubmitting = true;
    this.authService.forgotPassword(this.form)
      .pipe(finalize(() => { this.isSubmitting = false; }))
      .subscribe({
        next: () => {
          this.successMessage = 'Liên kết đặt lại mật khẩu đã được gửi.';
        },
        error: (err) => {
          const apiMessage = err?.error?.message;
          this.errorMessage = apiMessage || 'Không thể gửi yêu cầu. Vui lòng thử lại.';
        }
      });
  }

}
