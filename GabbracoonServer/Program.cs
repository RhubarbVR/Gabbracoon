using System;

using Cassandra;

using Gabbracoon;

using GabbracoonServer.Controllers;

using IdGen;
using IdGen.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using RhubarbServerNode.Database;

namespace GabbracoonServer
{
	public class Program
	{
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);

			//Config
			var generatorID = 0;
			var targetCassandraServices = new List<string> {
				"localhost"
			};
			var cassandraSSl = false;

			builder.Services.AddIdGen(generatorID, () => new IdGeneratorOptions(IdStructure.Default, new DefaultTimeSource(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)), SequenceOverflowStrategy.SpinWait));
			var database = new CassandraService(null, cassandraSSl, targetCassandraServices);
			database.DatabaseSession.CreateKeyspaceIfNotExists("GabbracoonDB");
			database.DatabaseSession.ChangeKeyspace("GabbracoonDB");
			builder.Services.AddSingleton<ICassandraService>(database);
			builder.Services.AddScoped<IUserAndAuthService,UserAndAuthService>();

			//Load Versioning table
			database.DatabaseSession.LoadVersioningTable();
			database.DatabaseSession.InitMainDatabase();

			var features = new Type[] {
				typeof(PasswordAuthFeature),
				typeof(OAuthFeature)
			}.Select(x => (IGabbracoonFeature)Activator.CreateInstance(x));
			builder.Services.AddMvc().AddApplicationPart(typeof(IGabbracoonFeature).Assembly).AddControllersAsServices();

			foreach (var item in features) {
				var startingVersion = database.DatabaseSession.GetVersion(item.Name);
				for (var i = startingVersion; i < item.MaxDatabaseVersion; i++) {
					Console.WriteLine($"Running {item.Name} Database Update {i + 1}");
					item.UpdateDatabse(database.DatabaseSession, i + 1);
					database.DatabaseSession.UpdateVersion(i + 1, item.Name);
				}
				builder.Services.AddMvc().AddApplicationPart(item.GetType().Assembly).AddControllersAsServices();
			}

			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			foreach (var feature in features) {
				feature.LoadBuilder(builder);
			}

			var health = builder.Services.AddHealthChecks()
				.AddCheck("Database", database);

			foreach (var feature in features) {
				feature.AddHealthCheck(health);
			}

			var app = builder.Build();

			app.UseSwagger();
			app.UseSwaggerUI();
			app.MapHealthChecks("/health");
			app.MapControllers();

			foreach (var feature in features) {
				feature.BuildApp(app);
			}

			GabbracoonController._featuresList = features.Select(x => x.Name).ToArray();

			app.Run();
		}
	}
}