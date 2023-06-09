using System;
using System.Net.Mail;
using System.Net;

using Cassandra;

using Gabbracoon;
using Gabbracoon.Email;

using GabbracoonServer.Controllers;

using IdGen;
using IdGen.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using RhubarbServerNode.Database;
using Gabbracoon.Certificate;

namespace GabbracoonServer
{
	public class Program
	{
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);
			var configuration = builder.Configuration;

			//Config
			var serverID = configuration.GetValue<int?>("serverID") ?? 0;
			var targetCassandraNodes = configuration.GetValue<string[]>("targetCassandraNodes") ?? new string[] {
				"localhost"
			};
			var cassandraSSl = configuration.GetValue<bool?>("cassandraSSl") ?? false;
			var emailerName = configuration.GetValue<string>("emailerName");
			var emailerPassword = configuration.GetValue<string>("emailerPassword");
			var emailerHost = configuration.GetValue<string>("emailerHost");
			var emailerPort = configuration.GetValue<int?>("emailerPort") ?? 587;
			var emailerSsl = configuration.GetValue<bool?>("emailerSsl") ?? true;
			var certificateLocation = configuration.GetValue<string>("certificateLocation") ?? "Gabbracoon.pem";
			var privateKeyLocation = configuration.GetValue<string>("privateKeyLocation") ?? "Gabbracoon.key";

			var certificate = new X509CertificateManager {
				CertificateLocation = certificateLocation,
				PrivateKeyLocation = privateKeyLocation
			};
			certificate.UpdateCertificate();
			builder.Services.AddSingleton<IX509CertificateManager>(certificate);
			builder.Services.AddIdGen(serverID, () => new IdGeneratorOptions(IdStructure.Default, new DefaultTimeSource(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)), SequenceOverflowStrategy.SpinWait));
			var database = new CassandraService(null, cassandraSSl, targetCassandraNodes);
			database.DatabaseSession.CreateKeyspaceIfNotExists("GabbracoonDB");
			database.DatabaseSession.ChangeKeyspace("GabbracoonDB");
			builder.Services.AddSingleton<ICassandraService>(database);
			builder.Services.AddScoped<IUserAndAuthService, UserAndAuthService>();

			IEmailer emailer;
			try {
				emailer = new SmtpEmailer(new SmtpClient() {
					Credentials = new NetworkCredential(emailerName, emailerPassword),
					Host = emailerHost,
					Port = emailerPort,
					EnableSsl = emailerSsl,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
				}, emailerName);
			}
			catch (Exception e) {
				Console.WriteLine($"Emailer Failed Now using nullemailer Error:{e}");
				emailer = new NullEmailer();
			}
			builder.Services.AddSingleton(emailer);
			builder.Services.AddTransient<IGabbracoonAuthProvider, EmailAuthProvider>();

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

			builder.Services.AddCors();
			var health = builder.Services.AddHealthChecks()
				.AddCheck("Database", database);

			foreach (var feature in features) {
				feature.AddHealthCheck(health);
			}

			var app = builder.Build();

			app.UseSwagger();
			app.UseSwaggerUI();
			app.MapHealthChecks("/gabbracoon_health");
			app.MapControllers();

			foreach (var feature in features) {
				feature.BuildApp(app);
			}

			GabbracoonController._featuresList = features.Select(x => x.Name).ToArray();


			app.UseCors(x => x
					.AllowAnyHeader()
					.AllowAnyMethod()
					.WithOrigins(
						"http://whispertail.net",
						"https://whispertail.net",
						"http://whispertail.com",
						"https://whispertail.com",
						"https://0.0.0.0",
						"http://0.0.0.0",
						"http://localhost",
						"https://localhost",
						"http://localhost:59345",
						"https://localhost:59345",
						"http://localhost:5269",
						"https://localhost:5269",
						"http://127.0.0.1:5500",
						"https://127.0.0.1:5500"
					));

			app.Run();
		}
	}
}