using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon.Email
{
	public sealed class SmtpEmailer : IEmailer
	{
		public SmtpClient smtpClient;

		public string fromEmail;

		public SmtpEmailer(SmtpClient smtpClient, string fromEmail) {
			this.smtpClient = smtpClient;
			this.fromEmail = fromEmail;
		}

		public async Task SendEmail(string targetEmail, string subject, string httpBody, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var mailMessage = new MailMessage {
				From = new MailAddress(fromEmail)
			};
			cancellationToken.ThrowIfCancellationRequested();
			mailMessage.To.Add(new MailAddress(targetEmail));
			mailMessage.Subject = subject;
			mailMessage.IsBodyHtml = true;
			mailMessage.Body = httpBody;
			cancellationToken.ThrowIfCancellationRequested();
			await smtpClient.SendMailAsync(mailMessage, cancellationToken);
		}
	}
}
