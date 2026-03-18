using DevOpsAiHub.Application.Common.Interfaces.Services;
using DevOpsAiHub.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DevOpsAiHub.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;

    public EmailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendOtpAsync(
        string email,
        string otp,
        string subject,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.FromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2>DevOpsAiHub OTP Verification</h2>
                    <p>Your OTP code is:</p>
                    <h1 style='letter-spacing: 4px; color: #2563eb;'>{otp}</h1>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you did not request this, please ignore this email.</p>
                </div>"
        };

        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        var socketOption = _emailOptions.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await smtp.ConnectAsync(
            _emailOptions.SmtpHost,
            _emailOptions.SmtpPort,
            socketOption,
            cancellationToken);

        await smtp.AuthenticateAsync(
            _emailOptions.SmtpUsername,
            _emailOptions.SmtpPassword,
            cancellationToken);

        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}