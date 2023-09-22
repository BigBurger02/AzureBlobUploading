using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using TestTask.Interfaces;
using TestTask.Model;

namespace TestTask.Services;

public class BrevoEmailSender : IEmailSender 
{
    private readonly ILogger<BrevoEmailSender> _logger;
    private readonly IConfiguration _configuration;
    
    public BrevoEmailSender(ILogger<BrevoEmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task<bool> SendAsync(MailData mailData, CancellationToken ct = default)
    {
        _logger.LogInformation("BrevoEmailSender.SendAsync invoked at {time} by {email}.", DateTime.UtcNow.ToString(), mailData.To.FirstOrDefault());
        
        try
        {
            var mail = new MimeMessage();
            var email = mailData.To.FirstOrDefault();
		
            #region Mail Data
		
            mail.From.Add(new MailboxAddress(mailData.DisplayName, mailData.From));
            mail.Sender = new MailboxAddress(mailData.DisplayName, mailData.From);
            mail.To.Add(MailboxAddress.Parse(email));

            _logger.LogInformation("Sender / Receiver formed at {time} by {email}.", DateTime.UtcNow.ToString(), email);
		
            mail.Subject = mailData.Subject;
            var body = new BodyBuilder();
            body.HtmlBody = mailData.Body;
            mail.Body = body.ToMessageBody();

            _logger.LogInformation("Mail formed at {time} by {email}.", DateTime.UtcNow.ToString(), email);

            #endregion

            #region Send Mail
		
            using var smtp = new SmtpClient();
		
            await smtp.ConnectAsync("smtp-relay.sendinblue.com", 587, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_configuration.GetValue<string>("SMTP:UserName"), _configuration.GetValue<string>("SMTP:PassWord"), ct);
            var a = await smtp.SendAsync(mail, ct);
            await smtp.DisconnectAsync(true, ct);

            _logger.LogInformation("Mail sent at {time} by {email}.", DateTime.UtcNow.ToString(), email);

            #endregion

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Send Mail thrown exception at {time} by {email}. Ex: {ex}", DateTime.UtcNow.ToString(), mailData.To.FirstOrDefault(), ex);
            return false;
        }
    }
}