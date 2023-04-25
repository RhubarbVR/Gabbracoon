using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cassandra;

using IdGen;

using RhubarbServerNode.Database;

namespace Gabbracoon
{
	public sealed class UserAndAuthService : IUserAndAuthService
	{
		private readonly ICassandraService _cassandraService;
		private readonly IIdGenerator<long> _idGen;

		public UserAndAuthService(ICassandraService cassandraService, IIdGenerator<long> idGen) {
			_cassandraService = cassandraService;
			_idGen = idGen;
		}


		private static int GenerateRandomColorHue() {
			var hue = (float)Random.Shared.NextDouble();
			var saturation = 1f;
			var value = 1f;
			var (R, G, B) = HSVToRGB(hue, saturation, value);
			return (255 << 24) | (R << 16) | (G << 8) | B;
		}

		private static (byte R, byte G, byte B) HSVToRGB(float h, float s, float v) {
			var hi = Convert.ToInt32(Math.Floor(h * 6)) % 6;
			var f = (h * 6) - (float)Math.Floor(h * 6);
			var p = (byte)(v * 255 * (1 - s));
			var q = (byte)(v * 255 * (1 - (f * s)));
			var t = (byte)(v * 255 * (1 - ((1 - f) * s)));
			var vByte = (byte)(v * 255);
			return hi switch {
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
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($"SELECT * FROM users WHERE email = ?;");
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(email);
			cancellationToken.ThrowIfCancellationRequested();
			var result = await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);
			return result.FirstOrDefault() is not null;
		}

		public async Task<long> CreateAccount(string email, string username, CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();
			var preparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO users (id, username, email, player_color, total_bytes, total_used_bytes) VALUES (?, ?, ?, ?, ?, ?); ");
			cancellationToken.ThrowIfCancellationRequested();
			var newUserID = _idGen.CreateId();
			cancellationToken.ThrowIfCancellationRequested();
			var randomColor = GenerateRandomColorHue();
			cancellationToken.ThrowIfCancellationRequested();
			var boundStatement = preparedStatement.Bind(newUserID, username, email, randomColor, 0L,0L);
			await _cassandraService.DatabaseSession.ExecuteAsync(boundStatement);

			var newAuthID = _idGen.CreateId();
			var authPreparedStatement = await _cassandraService.DatabaseSession.PrepareAsync($@"INSERT INTO auth_providers (id, auth_group, target_user, session_key, provider_type) VALUES (?, ?, ?, ?, ?); ");
			var authBoundStatement = authPreparedStatement.Bind(newAuthID, 0, newUserID, true, "EmailAuth");
			await _cassandraService.DatabaseSession.ExecuteAsync(authBoundStatement);

			return newUserID;
		}
	}
}
