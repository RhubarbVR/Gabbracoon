using System.Security.Cryptography;

using Gabbracoon.Email;

using IdGen;

using RequestModels;

using RhubarbServerNode.Database;

namespace Gabbracoon
{
	public sealed class EmailAuthProvider : IGabbracoonAuthProvider
	{
		private readonly IEmailer _emailer;
		private readonly ICassandraService _cassandraService;
		private readonly IUserAndAuthService _userAndAuthService;

		public EmailAuthProvider(IEmailer emailer, ICassandraService cassandraService, IUserAndAuthService userAndAuthService) {
			_emailer = emailer;
			_cassandraService = cassandraService;
			_userAndAuthService = userAndAuthService;
		}

		public string Name => "EmailAuth";

		public Task<long> AddAuthenticate(long TargetUser, string extraData, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public async Task<bool> Authenticate(AuthRequest request, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			try {
				var token = await _userAndAuthService.GetAuthProvidersPrivateData(request.TargetToken, cancellationToken);
				if (token is null) {
					return false;
				}
				var seprateLocation = token.IndexOf('_');
				if (seprateLocation <= -1) {
					return false;
				}
				var loginCode = token.Remove(seprateLocation);
				var dateStamp = token.Substring(seprateLocation);
				return long.TryParse(dateStamp, out var ticks)
										&&
					DateTime.UtcNow.AddMinutes(3) < new DateTime(ticks, DateTimeKind.Utc) 
										&&
					loginCode == request.AuthCode;
			}
			catch {
				return false;
			}
		}

		public Task RemoveAuthenticate(long TargetAuth, CancellationToken cancellationToken) {
			throw new NotImplementedException();
		}

		public async Task RequestAuthenticate(long TargetToken, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT email FROM users WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(TargetToken);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			var targetEmail = result.FirstOrDefault()?.GetValue<string>("email");
			var loginCode = BitConverter.ToString(RandomNumberGenerator.GetBytes(4));
			var encodeString = $"{loginCode}_{DateTime.UtcNow.Ticks}";
			cancellationToken.ThrowIfCancellationRequested();
			await _userAndAuthService.SetAuthProvidersPrivateData(TargetToken, encodeString, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			//Todo format html body nicer
#if DEBUG
			await _emailer.SendEmail(targetEmail, "Login Code", loginCode, cancellationToken);
#else
			await _emailer.SendEmail(targetEmail, "Login Code", loginCode, cancellationToken);
#endif
		}
	}
}
