using Cassandra;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using OAuth;

namespace Gabbracoon
{
	public sealed class OAuthFeature : IGabbracoonFeature
	{
		public string Name => nameof(OAuthFeature);

		public int MaxDatabaseVersion => -1;

		public void AddHealthCheck(IHealthChecksBuilder health) {
		}

		public void BuildApp(WebApplication builder) {
		}

		public void LoadBuilder(WebApplicationBuilder webApplicationBuilder) {
			webApplicationBuilder.Services.AddTransient<IGabbracoonAuthProvider, OAuthProvider>();
		}

		public void UpdateDatabse(ISession session, int version) {
		}
	}
}