namespace MediCare.API.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage message);
        Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
    }

    public class EmailMessage
    {
        public string ToEmail { get; set; } = default!;
        public string ToName { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string HtmlBody { get; set; } = default!;
        public string? PlainText { get; set; }
    }
}
