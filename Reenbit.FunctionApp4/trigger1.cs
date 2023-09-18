using System;
using System.IO;
// using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Org.BouncyCastle.Math.EC;

namespace Reenbit.FunctionApp4;

public class Trigger1
{
    [FunctionName("Trigger1")]
    public void Run([BlobTrigger("container1/{name}")] Stream myBlob, string name, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext context)
    {
        log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

        string email = "vlad.polovoy@gmail.com";
        
        var config = new ConfigurationBuilder()
	        .SetBasePath(context.FunctionAppDirectory)
	        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
	        .AddEnvironmentVariables()
	        .Build();
        
        log.LogInformation("SmtpEmailSender.SendAsync visited at {time} by {email}.", DateTime.UtcNow.ToString(), email);
		
		CancellationToken ct = default;
			
		try
		{
			var mail = new MimeMessage();

			#region Sender / Receiver
			// Sender
			// mail.From.Add(new MailboxAddress(_settings.DisplayName, mailData.From ?? _settings.From));
			// mail.Sender = new MailboxAddress(mailData.DisplayName ?? _settings.DisplayName, mailData.From ?? _settings.From);
			mail.From.Add(new MailboxAddress("AzureBlobUploading", "autosender@example.com"));
			mail.Sender = new MailboxAddress("AzureBlobUploading", "autosender@example.com");

			// Receiver
			// foreach (string mailAddress in mailData.To)
			// 	mail.To.Add(MailboxAddress.Parse(mailAddress));
			mail.To.Add(MailboxAddress.Parse(email));

			// Set Reply to if specified in mail data
			// if (!string.IsNullOrEmpty(mailData.ReplyTo))
			// 	mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

			// BCC
			// Check if a BCC was supplied in the request
			// if (mailData.Bcc != null)
			// {
			// 	// Get only addresses where value is not null or with whitespace. x = value of address
			// 	foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
			// 		mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
			// }

			// CC
			// Check if a CC address was supplied in the request
			// if (mailData.Cc != null)
			// {
			// 	foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
			// 		mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
			// }

			log.LogInformation("SmtpEmailSender.SendAsync: Sender / Receiver formed at {time} by {email}.", DateTime.UtcNow.ToString(), email);
			#endregion

			#region Content
			
			var body = new BodyBuilder();
			mail.Subject = "File Uploaded";
			body.HtmlBody = "Hi! Your file was successfully uploaded to the BLOB storage!";
			mail.Body = body.ToMessageBody();

			log.LogInformation("SmtpEmailSender.SendAsync: Content formed at {time} by {email}.", DateTime.UtcNow.ToString(), email);

			#endregion

			#region Send Mail

			var a = config["SMTP:UserName"];
			var b = config["SMTP:PassWord"];
			using var smtp = new SmtpClient();
			
			smtp.Connect("smtp-relay.sendinblue.com", 587, SecureSocketOptions.StartTls, ct);
			
			smtp.Authenticate(config["SMTP:UserName"], config["SMTP:PassWord"], ct);
			smtp.Send(mail, ct);
			smtp.Disconnect(true, ct);

			log.LogInformation("SmtpEmailSender.SendAsync: mail sent at {time} by {email}.", DateTime.UtcNow.ToString(), email);

			#endregion
		}
		catch (Exception)
		{
			log.LogError("SmtpEmailSender.SendAsync: Thrown exception at {time} by {email}.", DateTime.UtcNow.ToString(), email);
		}
    }
}