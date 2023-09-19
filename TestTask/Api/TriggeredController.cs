using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using TestTask.Data;

namespace TestTask.Api;

[Route("api/[controller]")]
[ApiController]
public class TriggeredController : ControllerBase
{
    private readonly ILogger<TriggeredController> _logger;
    private readonly IConfiguration _configuration;
    private readonly BlobContext _context;
    
    public TriggeredController(ILogger<TriggeredController> logger, IConfiguration configuration, BlobContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
    }
    
    [HttpGet("{fileName}")]
    public IActionResult BlobTriggered(string fileName)
    {
        _logger.LogInformation($"Triggered(get) api controller started. FileName: {fileName}");
        
        var blobData = _context.Uploads
            .AsNoTracking()
            .FirstOrDefault(n => n.FileName == fileName);
        if (blobData == null)
        {
            _logger.LogInformation($"Triggered(get) api controller. FileName not found in the db. FileName: {fileName}");
            return new ObjectResult("File with that name not found.") { StatusCode = 404 };
        }
	
        CancellationToken ct = default;
		
        try
        {
            var mail = new MimeMessage();
		
            #region Mail Data
		
            mail.From.Add(new MailboxAddress("AzureBlobUploading", "autosender@example.com"));
            mail.Sender = new MailboxAddress("AzureBlobUploading", "autosender@example.com");
            mail.To.Add(MailboxAddress.Parse(blobData.Email));

            _logger.LogInformation("Sender / Receiver formed at {time} by {email}.", DateTime.UtcNow.ToString(), blobData.Email);
		
            var body = new BodyBuilder();
            mail.Subject = "File Uploaded";
            body.HtmlBody = $"Hi! Your file was successfully uploaded to the BLOB storage: <a href=\"{blobData.Uri}\">{blobData.Uri}</a>";
            mail.Body = body.ToMessageBody();

            _logger.LogInformation("Mail formed at {time} by {email}.", DateTime.UtcNow.ToString(), blobData.Email);

            #endregion

            #region Send Mail
		
            using var smtp = new SmtpClient();
		
            smtp.Connect("smtp-relay.sendinblue.com", 587, SecureSocketOptions.StartTls, ct);
            smtp.Authenticate(_configuration.GetValue<string>("SMTP:UserName"), _configuration.GetValue<string>("SMTP:PassWord"), ct);
            smtp.Send(mail, ct);
            smtp.Disconnect(true, ct);

            _logger.LogInformation("Mail sent at {time} by {email}.", DateTime.UtcNow.ToString(), blobData.Email);

            #endregion
        }
        catch (Exception)
        {
            _logger.LogError("Send Mail thrown exception at {time} by {email}.", DateTime.UtcNow.ToString(), blobData.Email);
            return new ObjectResult("Exception thrown while sending mail.") { StatusCode = 500 };
        }

        return new ObjectResult("Mail sent.") { StatusCode = 200 };
    }
}