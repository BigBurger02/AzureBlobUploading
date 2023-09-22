using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TestTask.Data;
using TestTask.Interfaces;
using TestTask.Model;

namespace TestTask.Api;

[Route("api/[controller]")]
[ApiController]
public class TriggeredController : ControllerBase
{
    private readonly ILogger<TriggeredController> _logger;
    private readonly IConfiguration _configuration;
    private readonly BlobContext _context;
    private readonly IEmailSender _emailSender;

    public TriggeredController(ILogger<TriggeredController> logger, IConfiguration configuration, BlobContext context,
        IEmailSender emailSender)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> BlobTriggered(string fileName)
    {
        _logger.LogInformation($"Triggered(get) api controller started. FileName: {fileName}");

        var blobData = _context.Uploads
            .AsNoTracking()
            .FirstOrDefault(n => n.FileName == fileName);
        if (blobData == null)
        {
            _logger.LogInformation(
                $"Triggered(get) api controller. FileName not found in the db. FileName: {fileName}");
            return new ObjectResult("File with that name not found.") { StatusCode = 404 };
        }

        var mailData = new MailData(new List<string>() { blobData.Email }, "File Uploaded", $"Hi! Your file was successfully uploaded to the BLOB storage: <a href=\"{blobData.Uri}\">{blobData.Uri}</a>", "autosender@example.com", "AzureBlobUploading");

        if (await _emailSender.SendAsync(mailData))
        {
            return new ObjectResult("Mail sent.") { StatusCode = 200 };
        }
        else
        {
            return new ObjectResult("There was an error sending the email.") { StatusCode = 500 };
        }
    }
}