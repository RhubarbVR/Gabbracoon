using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon
{
	public interface IUserAndAuthService
	{
		public Task<bool> CheckIfEmailClamed(string email, CancellationToken cancellationToken);

		public Task<long> CreateAccount(string email, string username, CancellationToken cancellationToken);
	}
}
