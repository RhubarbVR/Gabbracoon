using Gabbracoon;

using RequestModels;

namespace PasswordAuth
{
	public sealed class PasswordAuthProvider : IGabbracoonAuthProvider
	{
		public string Name => "Password";

		public Task<long> AddAuthenticate(long TargetUser, string extraData, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public Task<bool> Authenticate(AuthRequest request, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public Task RemoveAuthenticate(long TargetAuth, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public Task RequestAuthenticate(long TargetToken, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}
	}
}
