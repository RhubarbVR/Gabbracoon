using Cassandra;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using PasswordAuth;

namespace Gabbracoon
{
	public sealed class PasswordAuthFeature : IGabbracoonFeature
	{
		public string Name => nameof(PasswordAuthFeature);

		public int MaxDatabaseVersion => -1;

		public void AddHealthCheck(IHealthChecksBuilder health) {
		}

		public void BuildApp(WebApplication builder) {
		}

		public void LoadBuilder(WebApplicationBuilder webApplicationBuilder) {
			webApplicationBuilder.Services.AddTransient<IGabbracoonAuthProvider, PasswordAuthProvider>();
		}

		public void UpdateDatabse(ISession session, int version) {
		}
	}
}