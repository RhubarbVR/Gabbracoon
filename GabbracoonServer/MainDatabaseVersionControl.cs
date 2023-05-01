using Gabbracoon;

using ISession = Cassandra.ISession;

namespace GabbracoonServer
{
	public static class MainDatabaseVersionControl
	{
		public const int MAIN_DATABASE_VERSION = 0;

		public static void InitMainDatabase(this ISession database) {
			var currentVersion = database.GetVersion();
			for (var i = currentVersion; i < MAIN_DATABASE_VERSION; i++) {
				database.UpdateDataBase(i + 1);
				database.UpdateVersion(i + 1);
			}
		}

		private static void UpdateDataBase(this ISession database, int version) {
			Console.WriteLine($"Running Main Database Update {version}");
			switch (version) {
				case 0:
					database.LoadVersionOne();
					break;
				default:
					throw new Exception("Version Not known");
			}
		}

		private static void LoadVersionOne(this ISession database) {

			database.Execute($@"CREATE TABLE IF NOT EXISTS users (
					id BIGINT PRIMARY KEY,
					username TEXT,
					email TEXT,
					player_color INT,
					total_bytes BIGINT,
					total_used_bytes BIGINT,
					has_login BOOLEAN
				);");

			database.Execute("CREATE INDEX IF NOT EXISTS users_emails ON users ( email );");
			database.Execute("CREATE INDEX IF NOT EXISTS users_usernames ON users ( username );");

			//target_user_index == target_user   should always be the fucking same
			database.Execute($@"CREATE TABLE IF NOT EXISTS auth_providers_names (
					auth_group INT,
					target_user BIGINT,
					target_user_index BIGINT,
					name TEXT,
					primary key (auth_group, target_user)
				);");

			database.Execute("CREATE INDEX IF NOT EXISTS auth_providers_names_target_users ON auth_providers_names ( target_user_index );");

			database.Execute($@"CREATE TABLE IF NOT EXISTS auth_providers (
					id BIGINT PRIMARY KEY,
					auth_group INT,
					target_user BIGINT,
					session_key BOOLEAN,
					provider_type TEXT,
					life_time BIGINT,
					private_data TEXT
				);");

			database.Execute("CREATE INDEX IF NOT EXISTS auth_providers_target_users ON auth_providers ( target_user );");

			database.Execute($@"CREATE TABLE IF NOT EXISTS auth_state (
					id BIGINT PRIMARY KEY,
					target_user BIGINT,
					target_provider BIGINT,
					roll_key BIGINT,
					last_used TIMESTAMP
				);");

			database.Execute("CREATE INDEX IF NOT EXISTS auth_state_target_users ON auth_state ( target_user );");


			//target_state_index == target_state   should always be the fucking same
			database.Execute($@"CREATE TABLE IF NOT EXISTS auth_info (
					target_state_index BIGINT,
					target_state BIGINT,
					user_agent TEXT,
					region_code TEXT,
					ip TEXT,
					primary key (target_state, user_agent, region_code, ip)
				);");

			database.Execute("CREATE INDEX IF NOT EXISTS auth_info_target_state ON auth_info ( target_state_index );");



		}
	}
}
