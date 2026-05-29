import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

// Cấu trúc của một cục Toast
export interface ToastInfo {
  id: number;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
}

@Injectable({
  providedIn: 'root' // Khai báo root để dùng chung toàn app
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<ToastInfo[]>([]);
  toasts$ = this.toastsSubject.asObservable();
  private idCounter = 0;

  // Các hàm gọi nhanh (Sử dụng ở các Component khác)
  success(title: string, message: string) {
    this.show('success', title, message);
  }

  error(title: string, message: string) {
    this.show('error', title, message);
  }

  warning(title: string, message: string) {
    this.show('warning', title, message);
  }

  // Hàm core xử lý logic
  private show(type: 'success' | 'error' | 'warning' | 'info', title: string, message: string) {
    const newToast: ToastInfo = { id: this.idCounter++, type, title, message };

    // Thêm Toast mới vào mảng
    this.toastsSubject.next([...this.toastsSubject.value, newToast]);

    // Tự động xóa sau 3.5 giây
    setTimeout(() => {
      this.remove(newToast.id);
    }, 3500);
  }

  // Hàm tắt Toast khi bấm nút X
  remove(id: number) {
    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next(currentToasts.filter(t => t.id !== id));
  }
}
