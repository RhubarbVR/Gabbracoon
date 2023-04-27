using RequestModels;

namespace Gabbracoon
{
	public interface IGabbracoonAuthProvider
	{
		public string Name { get; }

		public Task<bool> Authenticate(AuthRequest request, CancellationToken cancellationToken);
		public Task RequestAuthenticate(long TargetToken, CancellationToken cancellationToken);
		public Task<long> AddAuthenticate(long TargetUser, string extraData, CancellationToken cancellationToken);
		public Task RemoveAuthenticate(long TargetAuth, CancellationToken cancellationToken);


	}
}
