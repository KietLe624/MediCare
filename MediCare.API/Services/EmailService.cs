using MailKit.Net.Smtp;
using MailKit.Security;
using MediCare.API.Common;
using MediCare.API.Interfaces;
using MediCare.API.Middlewares;
using Microsoft.Extensions.Options;
using MimeKit;
using static MediCare.API.Middlewares.ExceptionMiddleware;

namespace MediCare.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value; // Lấy giá trị cấu hình từ IOptions 
            _logger = logger;
        }

        // SEND MAIL 
        public async Task SendAsync(EmailMessage message)
        {
            var email = BuildMimeMessage(message);

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("your-email@gmail.com", "your-password");

                await client.SendAsync(email);

                Console.WriteLine("Email sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Send mail failed");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        // SEND MAIL RESET PASSWORD
        public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
        {
            var html = BuildPasswordResetHtml(toName, resetLink);

            await SendAsync(new EmailMessage
            {
                ToEmail = toEmail,
                ToName = toName,
                Subject = "[MediCare] Đặt lại mật khẩu của bạn",
                HtmlBody = html,
                PlainText = $"Truy cập link sau để đặt lại mật khẩu (hiệu lực 15 phút):\n{resetLink}"
            });
        }

        // HELPER BUILD MIME MESSAGE
        private MimeMessage BuildMimeMessage(EmailMessage message)
        {
            var mime = new MimeMessage();

            mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            mime.To.Add(new MailboxAddress(message.ToName, message.ToEmail));
            mime.Subject = message.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody,
                TextBody = message.PlainText
            };
            mime.Body = builder.ToMessageBody();

            return mime;
        }

        private static string BuildPasswordResetHtml(string name, string resetLink)
        {
            return $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body style="margin:0;padding:0;background:#f4f6f9;font-family:Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0"
                     style="background:#f4f6f9;padding:40px 0;">
                <tr>
                  <td align="center">
                    <table width="560" cellpadding="0" cellspacing="0"
                           style="background:#ffffff;border-radius:8px;
                                  box-shadow:0 2px 8px rgba(0,0,0,.08);
                                  overflow:hidden;">
 
                      <!-- Header -->
                      <tr>
                        <td style="background:#0f6e56;padding:28px 40px;text-align:center;">
                          <h1 style="margin:0;color:#ffffff;font-size:22px;
                                     font-weight:700;letter-spacing:1px;">
                            MediCare
                          </h1>
                        </td>
                      </tr>
 
                      <!-- Body -->
                      <tr>
                        <td style="padding:36px 40px;">
                          <p style="margin:0 0 12px;font-size:16px;color:#1a1a1a;">
                            Xin chào <strong>{name}</strong>,
                          </p>
                          <p style="margin:0 0 24px;font-size:15px;color:#444;line-height:1.6;">
                            Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                            Nhấn vào nút bên dưới để tiến hành đặt lại.
                          </p>
 
                          <!-- CTA Button -->
                          <table cellpadding="0" cellspacing="0" width="100%">
                            <tr>
                              <td align="center" style="padding:8px 0 28px;">
                                <a href="{resetLink}"
                                   style="display:inline-block;background:#0f6e56;
                                          color:#ffffff;font-size:15px;font-weight:600;
                                          text-decoration:none;padding:14px 36px;
                                          border-radius:6px;">
                                  Đặt lại mật khẩu
                                </a>
                              </td>
                            </tr>
                          </table>
 
                          <!-- Warning -->
                          <table cellpadding="0" cellspacing="0" width="100%">
                            <tr>
                              <td style="background:#fff8e1;border-left:4px solid #f59e0b;
                                         padding:14px 16px;border-radius:0 4px 4px 0;">
                                <p style="margin:0;font-size:13px;color:#92400e;">
                                  ⚠️ Link chỉ có hiệu lực trong <strong>15 phút</strong>.
                                  Nếu bạn không yêu cầu, hãy bỏ qua email này.
                                </p>
                              </td>
                            </tr>
                          </table>
 
                          <!-- Fallback link -->
                          <p style="margin:24px 0 0;font-size:12px;color:#888;">
                            Nếu nút không hoạt động, copy link sau vào trình duyệt:<br>
                            //<a href="{resetLink}" style="color:#0f6e56;word-break:break-all;">
                            //  {resetLink}
                            //</a>
                          </p>
                        </td>
                      </tr>
 
                      <!-- Footer -->
                      <tr>
                        <td style="background:#f9fafb;padding:20px 40px;
                                   border-top:1px solid #e5e7eb;text-align:center;">
                          <p style="margin:0;font-size:12px;color:#9ca3af;">
                            © {DateTime.UtcNow.Year} MediCare. Email này được gửi tự động,
                            vui lòng không reply.
                          </p>
                        </td>
                      </tr>
 
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
        }
    }
}

