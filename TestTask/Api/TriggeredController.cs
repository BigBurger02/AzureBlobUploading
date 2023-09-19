using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace TestTask.Api;

[Route("api/[controller]")]
[ApiController]
public class TriggeredController : ControllerBase
{
    private readonly ILogger<TriggeredController> _logger;
    private readonly IConfiguration _configuration;
    
    public TriggeredController(ILogger<TriggeredController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    [HttpGet("{fileName}")]
    public IActionResult BlobTriggered(string fileName)
    {
        string email = "";
	
        CancellationToken ct = default;
		
        try
        {
            var mail = new MimeMessage();
		
            #region Mail Data
		
            mail.From.Add(new MailboxAddress("AzureBlobUploading", "autosender@example.com"));
            mail.Sender = new MailboxAddress("AzureBlobUploading", "autosender@example.com");
            mail.To.Add(MailboxAddress.Parse(email));

            _logger.LogInformation("Sender / Receiver formed at {time} by {email}.", DateTime.UtcNow.ToString(), email);
		
            var body = new BodyBuilder();
            mail.Subject = "File Uploaded";
            body.HtmlBody = "Hi! Your file was successfully uploaded to the BLOB storage!";
            mail.Body = body.ToMessageBody();

            _logger.LogInformation("Mail formed at {time} by {email}.", DateTime.UtcNow.ToString(), email);

            #endregion

            #region Send Mail
		
            using var smtp = new SmtpClient();
		
            smtp.Connect("smtp-relay.sendinblue.com", 587, SecureSocketOptions.StartTls, ct);
            smtp.Authenticate(_configuration.GetValue<string>("SMTP:UserName"), _configuration.GetValue<string>("SMTP:PassWord"), ct);
            smtp.Send(mail, ct);
            smtp.Disconnect(true, ct);

            _logger.LogInformation("Mail sent at {time} by {email}.", DateTime.UtcNow.ToString(), email);

            #endregion
        }
        catch (Exception)
        {
            _logger.LogError("Send Mail thrown exception at {time} by {email}.", DateTime.UtcNow.ToString(), email);
            return new ObjectResult("Exception thrown while sending mail.") { StatusCode = 500 };
        }

        return new ObjectResult("Mail sent.") { StatusCode = 200 };
    }
}