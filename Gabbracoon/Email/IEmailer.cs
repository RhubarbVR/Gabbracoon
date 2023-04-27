using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon.Email
{
	public interface IEmailer
	{
		public Task SendEmail(string targetEmail, string subject, string httpBody, CancellationToken cancellationToken);
	}
}
