using BitirmeTezi.Auth;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BitirmeTezi.Service
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Kullanıcıya email doğrulama maili gönderir.
        /// Link: {FrontendBaseUrl}/verify-email?email=...&token=...
        /// </summary>
        public async Task SendEmailVerificationAsync(string toEmail, string toName, string token)
        {
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(toEmail);
            var verifyUrl = $"{_settings.FrontendBaseUrl}/verify-email?email={encodedEmail}&token={encodedToken}";

            var subject = "E-posta Adresinizi Doğrulayın — Türkçe Öğren";

            var htmlBody = $"""
                <!DOCTYPE html>
                <html lang="tr">
                <body style="font-family: Arial, sans-serif; background: #f5f6fa; margin: 0; padding: 0;">
                  <div style="max-width: 480px; margin: 40px auto; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08);">
                    <div style="background: #1CB0F6; padding: 32px; text-align: center;">
                      <h1 style="color: #fff; margin: 0; font-size: 24px;">Türkçe Öğren</h1>
                    </div>
                    <div style="padding: 32px;">
                      <h2 style="color: #1a1a1a; margin-top: 0;">Merhaba, {toName}!</h2>
                      <p style="color: #555; line-height: 1.6;">
                        Hesabınızı oluşturduğunuz için teşekkürler. Hesabınızı aktifleştirmek için
                        aşağıdaki butona tıklayarak e-posta adresinizi doğrulayın.
                      </p>
                      <div style="text-align: center; margin: 32px 0;">
                        <a href="{verifyUrl}"
                           style="background: #1CB0F6; color: #fff; padding: 14px 32px; border-radius: 12px;
                                  text-decoration: none; font-weight: bold; font-size: 16px; display: inline-block;">
                          E-postamı Doğrula
                        </a>
                      </div>
                      <p style="color: #888; font-size: 13px; line-height: 1.6;">
                        Bu link <strong>24 saat</strong> geçerlidir. Eğer hesap oluşturmadıysanız bu maili görmezden gelebilirsiniz.
                      </p>
                      <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;" />
                      <p style="color: #bbb; font-size: 12px; word-break: break-all;">
                        Link çalışmıyorsa şu URL'yi tarayıcınıza kopyalayın:<br/>{verifyUrl}
                      </p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await SendMailAsync(toEmail, subject, htmlBody);
        }

        /// <summary>
        /// Kullanıcıya şifre sıfırlama maili gönderir.
        /// Link: {FrontendBaseUrl}/reset-password?email=...&token=...
        /// </summary>
        public async Task SendPasswordResetAsync(string toEmail, string toName, string token)
        {
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(toEmail);
            var resetUrl = $"{_settings.FrontendBaseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

            var subject = "Şifre Sıfırlama İsteği — Türkçe Öğren";

            var htmlBody = $"""
                <!DOCTYPE html>
                <html lang="tr">
                <body style="font-family: Arial, sans-serif; background: #f5f6fa; margin: 0; padding: 0;">
                  <div style="max-width: 480px; margin: 40px auto; background: #fff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08);">
                    <div style="background: #1CB0F6; padding: 32px; text-align: center;">
                      <h1 style="color: #fff; margin: 0; font-size: 24px;">Türkçe Öğren</h1>
                    </div>
                    <div style="padding: 32px;">
                      <h2 style="color: #1a1a1a; margin-top: 0;">Şifre Sıfırlama</h2>
                      <p style="color: #555; line-height: 1.6;">
                        Merhaba, {toName}! Hesabınız için bir şifre sıfırlama isteği alındı.
                        Yeni şifrenizi belirlemek için aşağıdaki butona tıklayın.
                      </p>
                      <div style="text-align: center; margin: 32px 0;">
                        <a href="{resetUrl}"
                           style="background: #FF9600; color: #fff; padding: 14px 32px; border-radius: 12px;
                                  text-decoration: none; font-weight: bold; font-size: 16px; display: inline-block;">
                          Şifremi Sıfırla
                        </a>
                      </div>
                      <p style="color: #888; font-size: 13px; line-height: 1.6;">
                        Bu link <strong>1 saat</strong> geçerlidir. Eğer bu isteği siz yapmadıysanız
                        bu maili görmezden gelebilirsiniz — şifreniz değişmeyecektir.
                      </p>
                      <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;" />
                      <p style="color: #bbb; font-size: 12px; word-break: break-all;">
                        Link çalışmıyorsa şu URL'yi tarayıcınıza kopyalayın:<br/>{resetUrl}
                      </p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await SendMailAsync(toEmail, subject, htmlBody);
        }

        /// <summary>
        /// Ortak SMTP gönderim metodu.
        /// Port 587 → STARTTLS, Port 465 → SSL/TLS
        /// </summary>
        private async Task SendMailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                // Gmail port 587 → STARTTLS, port 465 → SslOnConnect
                var secureOption = _settings.SmtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, secureOption);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Mail gönderildi: {ToEmail} — {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gönderilemedi: {ToEmail} — {Subject}", toEmail, subject);
                throw; // Controller'a iletilir, 500 döner
            }
        }
    }
}
