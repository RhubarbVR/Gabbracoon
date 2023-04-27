using Gabbracoon;

using RequestModels;

namespace OAuth
{
	public sealed class OAuthProvider : IGabbracoonAuthProvider
	{
		public string Name => "OAuth";

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
