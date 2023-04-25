using System.Runtime.Intrinsics.X86;

using Cassandra;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RhubarbServerNode.Database
{

	public class CassandraService : ICassandraService, IDisposable, IHealthCheck
	{
		private readonly Cluster _databaseCluster;
		public Cassandra.ISession DatabaseSession { get; private set; }

		public CassandraService(IAuthProvider authProvider, string contactPoint) {
			var builder = new Builder();
			builder = builder.AddContactPoint(contactPoint);
			if (authProvider is not null) {
				builder = builder.WithAuthProvider(authProvider);
			}
			_databaseCluster = builder.Build();
			DatabaseSession = _databaseCluster.Connect();
		}

		public CassandraService(IAuthProvider authProvider, bool ssl, IEnumerable<string> contactPoints) {
			var builder = new Builder();
			builder = builder.AddContactPoints(contactPoints);
			if (ssl) {
				builder = builder.WithSSL();
			}
			if (authProvider is not null) {
				builder = builder.WithAuthProvider(authProvider);
			}
			_databaseCluster = builder.Build();
			DatabaseSession = _databaseCluster.Connect();
		}

		public CassandraService(IAuthProvider authProvider, params string[] contactPoints) {
			var builder = new Builder();
			builder = builder.AddContactPoints(contactPoints);
			if (authProvider is not null) {
				builder = builder.WithAuthProvider(authProvider);
			}
			_databaseCluster = builder.Build();
			DatabaseSession = _databaseCluster.Connect();
		}

		public void Dispose() {
			_databaseCluster.Dispose();
			GC.SuppressFinalize(this);
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
			try {
				var rowSet = await DatabaseSession.ExecuteAsync(new SimpleStatement("SELECT now() FROM system.local;"));
				return HealthCheckResult.Healthy();
			}
			catch (Exception ex) {
				return HealthCheckResult.Unhealthy("Cassandra health check failed.", ex);
			}
		}
	}

}
