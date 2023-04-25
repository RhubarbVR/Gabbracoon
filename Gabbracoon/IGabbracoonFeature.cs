using Cassandra;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using RhubarbServerNode.Database;

namespace Gabbracoon
{
	public interface IGabbracoonFeature
	{
		public string Name { get; }

		public int MaxDatabaseVersion { get; }
		public void UpdateDatabse(ISession session, int version);

		public void AddHealthCheck(IHealthChecksBuilder health);
		public void LoadBuilder(WebApplicationBuilder builder);
		public void BuildApp(WebApplication app);
	}
}