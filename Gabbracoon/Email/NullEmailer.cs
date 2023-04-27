using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon.Email
{
	public sealed class NullEmailer : IEmailer
	{
		public Task SendEmail(string targetEmail, string subject, string httpBody, CancellationToken cancellationToken) {
			Console.WriteLine($"Null Emailer Subject:{subject} HttpBody:{httpBody}");
			return Task.CompletedTask;
		}
	}
}
