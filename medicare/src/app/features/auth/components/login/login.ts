import { Component } from '@angular/core';
import { LoginRequest } from '../../models/auth.model';
import { AuthService } from '../../services/auth.service';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  constructor(private authService: AuthService) { }

  credentials: LoginRequest = {
    userName: '',
    password: '',
  };

  rememberMe = false;
  isSubmitting = false;
  errorMessage = '';

  onSubmit(): void {
    this.errorMessage = '';

    if (!this.credentials.userName || !this.credentials.password) {
      this.errorMessage = 'Vui lòng nhập đầy đủ tên người dùng và mật khẩu.';
      return;
    }

    this.isSubmitting = true;
    this.authService.login(this.credentials)
      .pipe(finalize(() => { this.isSubmitting = false; }))
      .subscribe({
        next: () => {
          this.errorMessage = '';
        },
        error: (err) => {
          const apiMessage = err?.error?.message;
          this.errorMessage = apiMessage || 'Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.';
        }
      });
  }
}
