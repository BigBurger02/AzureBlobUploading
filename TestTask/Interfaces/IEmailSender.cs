using TestTask.Model;

namespace TestTask.Interfaces;

public interface IEmailSender
{
    Task<bool> SendAsync(MailData mailData, CancellationToken ct = default);
}