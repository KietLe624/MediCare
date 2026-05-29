// Định nghĩa Interface ngay trong file này để dùng chung
export interface StatusConfig {
  badge: string;
  dot: string;
  label: string;
}

export class AppointmentStatusHelper {
  // Đặt private để ngăn các nơi khác sửa đổi trực tiếp cục dữ liệu này (Bảo mật data)
  private static readonly STATUS_MAP: Record<string, StatusConfig> = {
    'Confirmed': {
      badge: 'bg-green-100 text-green-700',
      dot: 'bg-green-500',
      label: 'Đã xác nhận'
    },
    'Scheduled': {
      badge: 'bg-yellow-100 text-yellow-800',
      dot: 'bg-yellow-500',
      label: 'Đã lên lịch'
    },
    'Cancelled': {
      badge: 'bg-red-100 text-red-700',
      dot: 'bg-red-500',
      label: 'Đã hủy'
    }
  };

  // Hàm static cho phép gọi trực tiếp: AppointmentStatusHelper.getConfig(...)
  static getConfig(status: string): StatusConfig {
    return this.STATUS_MAP[status] || {
      badge: 'bg-slate-100 text-slate-700',
      dot: 'bg-slate-500',
      label: status // Giữ nguyên tiếng Anh nếu là trạng thái lạ
    };
  }
}
