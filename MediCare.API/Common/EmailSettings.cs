namespace MediCare.API.Common
{
    public class EmailSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = false;   // true nếu port 465
        public bool StartTls { get; set; } = true;    // true cho port 587
        public string Username { get; set; } = default!; // email đăng nhập SMTP
        public string Password { get; set; } = default!; // app password (không phải pass thường)
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = "MediCare";
    }
}
