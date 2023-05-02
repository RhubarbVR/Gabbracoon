using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Cassandra;

using Gabbracoon.Certificate;

using IdGen;

using JWT;
using JWT.Algorithms;
using JWT.Builder;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json.Linq;

using RequestModels;

using RhubarbServerNode.Database;

namespace Gabbracoon
{
	public sealed class UserAndAuthService : IUserAndAuthService
	{
		private readonly ICassandraService _cassandraService;
		private readonly IIdGenerator<long> _idGen;
		private readonly IX509CertificateManager _x509CertificateManager;

		public UserAndAuthService(ICassandraService cassandraService, IIdGenerator<long> idGen, IX509CertificateManager x509CertificateManager) {
			_cassandraService = cassandraService;
			_idGen = idGen;
			_x509CertificateManager = x509CertificateManager;
		}


		private static int GenerateRandomColorHue() {
			var hue = (float)Random.Shared.NextDouble();
			var saturation = 1f;
			var value = 1f;
			var (R, G, B) = HSVToRGB(hue, saturation, value);
			return (255 << 24) | (R << 16) | (G << 8) | B;
		}

		private static (byte R, byte G, byte B) HSVToRGB(float h, float s, float v) {
			var f = (h * 6) - (float)Math.Floor(h * 6);
			var p = (byte)(v * 255 * (1 - s));
			var q = (byte)(v * 255 * (1 - (f * s)));
			var t = (byte)(v * 255 * (1 - ((1 - f) * s)));
			var vByte = (byte)(v * 255);
			return (Convert.ToInt32(Math.Floor(h * 6)) % 6) switch {
				0 => (vByte, t, p),
				1 => (q, vByte, p),
				2 => (p, vByte, t),
				3 => (p, q, vByte),
				4 => (t, p, vByte),
				_ => (vByte, p, q),
			};
		}

		public async Task<bool> CheckIfEmailClamed(string email, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT username FROM users WHERE email = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(email);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.FirstOrDefault() is not null;
		}

		public async Task<long?> GetUserIDFromEmail(string email, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT id FROM users WHERE email = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(email);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.FirstOrDefault()?.GetValue<long>("id");
		}

		public async IAsyncEnumerable<(MissingAuth auth, int group)> GetAuths(long targetUser, [EnumeratorCancellation] CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT * FROM auth_providers WHERE target_user = ?;");
			var boundStatement = preparedStatement.Bind(targetUser);
			cancellationToken.ThrowIfCancellationRequested();
			var rows = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			cancellationToken.ThrowIfCancellationRequested();
			foreach (var row in rows) {
				yield return (new MissingAuth {
					AuthProviderType = row.GetValue<string>("provider_type"),
					SessionToken = row.GetValue<bool>("session_key"),
					TargetToken = row.GetValue<long>("id")
				}, row.GetValue<int>("auth_group"));
			}
		}

		public async Task SetAuthProvidersPrivateData(long targetProvider, string newValue, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO auth_providers (id, private_data) VALUES (?, ?); ");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(targetProvider, newValue);
			cancellationToken.ThrowIfCancellationRequested();
			await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
		}

		public async Task<(TimeSpan, long?)> GetAuthProvidersTimeSpanAndUser(long targetProvider, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT life_time, target_user FROM auth_providers WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(targetProvider);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return (new TimeSpan(result.FirstOrDefault()?.GetValue<long>("life_time") ?? 0), result.FirstOrDefault()?.GetValue<long>("target_user"));
		}

		public async Task<TimeSpan> GetAuthProvidersTimeSpan(long targetProvider, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT life_time FROM auth_providers WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(targetProvider);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return new TimeSpan(result.FirstOrDefault()?.GetValue<long>("life_time") ?? 0);
		}

		public async Task<long?> GetAuthProvidersUser(long targetProvider, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT target_user FROM auth_providers WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(targetProvider);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.FirstOrDefault()?.GetValue<long>("target_user");
		}


		public async Task<string> GetAuthProvidersPrivateData(long targetProvider, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT private_data FROM auth_providers WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(targetProvider);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.FirstOrDefault()?.GetValue<string>("private_data");
		}

		public async Task<string[]> GetAuthProviderNames(long userId, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT name FROM auth_providers_names WHERE target_user_index = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(userId);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.Select(x => x?.GetValue<string>("name")).ToArray();
		}


		public async Task<long> CreateAccount(string email, string username, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO users (id, username, email, player_color, total_bytes, total_used_bytes, has_login) VALUES (?, ?, ?, ?, ?, ?, ?); ");
			cancellationToken.ThrowIfCancellationRequested();
			var newUserID = _idGen.CreateId();
			cancellationToken.ThrowIfCancellationRequested();
			var randomColor = GenerateRandomColorHue();
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(newUserID, username, email, randomColor, 0L, 0L, false);
			await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);

			var newAuthID = _idGen.CreateId();
			var authPreparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO auth_providers (id, auth_group, target_user, session_key, provider_type, life_time) VALUES (?, ?, ?, ?, ?, ?); ");
			var authBoundStatement = authPreparedStatement.Bind(newAuthID, 0, newUserID, true, "EmailAuth", TimeSpan.FromHours(6).Ticks);
			await _cassandraService.DatabaseSession.ExecuteAsync(authBoundStatement);

			var auth_providers_names_PreparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO auth_providers_names (auth_group, target_user, target_user_index, name) VALUES (?, ?, ?, ?); ");
			var auth_providers_names_BoundStatement = auth_providers_names_PreparedStatement.Bind(0, newUserID, newUserID, "Email Back Up");
			await _cassandraService.DatabaseSession.ExecuteAsync(auth_providers_names_BoundStatement);

			return newUserID;
		}

		public async Task MarkUserHasLogin(long id, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO users (id, has_login) VALUES (?, ?); ");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(id, true);
			await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
		}

		public async Task<(PrivateUserData user, bool hasLogin)> GetUser(long id, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT * FROM users WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(id);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			var resultData = result.FirstOrDefault();
			var user = resultData is null
				? null
				: new PrivateUserData {
					PlayerColor = resultData.GetValue<int>("player_color"),
					Username = resultData.GetValue<string>("username"),
					TotalBytes = resultData.GetValue<long>("total_bytes"),
					TotalUsedBytes = resultData.GetValue<long>("total_used_bytes"),
				};
			return (user, resultData?.GetValue<bool>("has_login") ?? false);
		}

		public async Task<bool?> GetAuthProvidersIsSessionToken(long targetProvider, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT session_key FROM auth_providers WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(targetProvider);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.FirstOrDefault()?.GetValue<bool>("session_key");
		}

		public async Task<string> GetNewAuthToken(long targetToken, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var (timeSpan, userID) = await GetAuthProvidersTimeSpanAndUser(targetToken, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO auth_state (id, target_user, target_provider, roll_key, last_used) VALUES (?, ?, ?, ?, ?); ");
			cancellationToken.ThrowIfCancellationRequested();
			var authState = _idGen.CreateId();
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(authState, userID, targetToken, 0L, DateTime.UtcNow);
			await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);

			return JwtBuilder.Create()
					  .WithAlgorithm(new RS256Algorithm(_x509CertificateManager.Certificate))
					  .AddClaim("exp", Convert.ToBase64String(BitConverter.GetBytes((DateTime.UtcNow + timeSpan).Ticks)))
					  .AddClaim("userID", Convert.ToBase64String(BitConverter.GetBytes(userID ?? 0)))
					  .AddClaim("rollingID", Convert.ToBase64String(BitConverter.GetBytes(0L)))
					  .AddClaim("authToken", Convert.ToBase64String(BitConverter.GetBytes(targetToken)))
					  .AddClaim("authState", Convert.ToBase64String(BitConverter.GetBytes(authState)))
					  .Encode();
		}

		public async Task<(bool, long)> ValidateAuth(long targetToken, long? findUser, string token, HttpRequest request, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var info = JObject.Parse(JwtBuilder.Create()
						.WithAlgorithm(new RS256Algorithm(_x509CertificateManager.Certificate))
						.Decode(token));
			cancellationToken.ThrowIfCancellationRequested();
			DateTime? exp = null;
			long? authToken = null;
			long? authState = null;
			long? userID = null;
			long? rollingID = null;
			if (info.TryGetValue("exp", out var expToken)) {
				exp = new DateTime(BitConverter.ToInt64(Convert.FromBase64String((string)expToken)));
			}
			if (info.TryGetValue("authToken", out var authTokenToken)) {
				authToken = BitConverter.ToInt64(Convert.FromBase64String((string)authTokenToken));
			}
			if (info.TryGetValue("authState", out var authStateToken)) {
				authState = BitConverter.ToInt64(Convert.FromBase64String((string)authStateToken));
			}
			if (info.TryGetValue("userID", out var userIDToken)) {
				userID = BitConverter.ToInt64(Convert.FromBase64String((string)userIDToken));
			}
			if (info.TryGetValue("rollingID", out var rollingIDToken)) {
				rollingID = BitConverter.ToInt64(Convert.FromBase64String((string)rollingIDToken));
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (exp is null || authToken is null || authState is null || userID is null || rollingID is null) {
				return (false, 0);
			}
			if (DateTime.UtcNow > exp.Value) {
				return (false, 0);
			}
			if (findUser is not null) {
				if (findUser != userID) {
					return (false, 0);
				}
			}
			if(targetToken != authToken) {
				return (false, 0);
			}
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT roll_key FROM auth_state WHERE id = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(authToken);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			var rollId = result.FirstOrDefault()?.GetValue<long>("roll_key");
			if (rollId is null) {
				return (false, 0);
			}

			var auth_info_PreparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"INSERT INTO auth_info (target_state_index, target_state, user_agent, region_code, ip) VALUES (?, ?, ?, ?, ?);");
			var auth_info_BoundStatement = auth_info_PreparedStatement.Bind(authState ?? 0L, authState ?? 0L, request.Headers["User-Agent"].ToString(), request.Headers["User-Agent"].ToString(), request.HttpContext.Connection.RemoteIpAddress.ToString());
			await _cassandraService.DatabaseSession.ExecuteAsync(auth_info_BoundStatement);

			if (rollId != rollingID) {
				// Set roll_key to -1 to lock all users out
				if (rollingID != -1) {
					var roll_keyPreparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"INSERT INTO auth_state (id, roll_key) VALUES (?, ?);");
					var roll_keyBoundStatement = roll_keyPreparedStatement.Bind(authToken, -1L);
					await _cassandraService.DatabaseSession.ExecuteAsync(roll_keyBoundStatement);
				}

				return (false, 0);
			}



			return (true, 0);

		}
	}
}
