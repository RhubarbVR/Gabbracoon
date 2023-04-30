using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using RequestModels;

namespace Gabbracoon
{
	public interface IUserAndAuthService
	{

		public Task<bool?> GetAuthProvidersIsSessionToken(long targetProvider, CancellationToken cancellationToken);

		public Task<long?> GetAuthProvidersUser(long targetProvider, CancellationToken cancellationToken);

		public Task SetAuthProvidersPrivateData(long targetProvider, string newValue, CancellationToken cancellationToken);
		
		public Task<string> GetAuthProvidersPrivateData(long targetProvider, CancellationToken cancellationToken);

		public Task MarkUserHasLogin(long id, CancellationToken cancellationToken);

		public Task<(PrivateUserData user, bool hasLogin)> GetUser(long id, CancellationToken cancellationToken);

		public Task<long?> GetUserIDFromEmail(string email, CancellationToken cancellationToken);

		public IAsyncEnumerable<(MissingAuth auth, int group)> GetAuths(long targetUser, CancellationToken cancellationToken);

		public Task<bool> CheckIfEmailClamed(string email, CancellationToken cancellationToken);

		public Task<long> CreateAccount(string email, string username, CancellationToken cancellationToken);
		public Task<string> GetNewAuthToken(long targetToken, CancellationToken cancellationToken);
		Task<TimeSpan> GetAuthProvidersTimeSpan(long targetProvider, CancellationToken cancellationToken);
		Task<(TimeSpan, long?)> GetAuthProvidersTimeSpanAndUser(long targetProvider, CancellationToken cancellationToken);
		Task<string[]> GetAuthProviderNames(long userId, CancellationToken cancellationToken);
	}
}
