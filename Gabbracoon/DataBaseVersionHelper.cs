using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Cassandra;

using IdGen;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Gabbracoon
{
	public static class DataBaseVersionHelper
	{
		public const string DATABASE_VERSIONS_TABLE = "database_versions";

		public static void LoadVersioningTable(this ISession database) {
			database.Execute($@"CREATE TABLE IF NOT EXISTS {DATABASE_VERSIONS_TABLE} (name TEXT PRIMARY KEY, version INT);");
		}

		public static void UpdateVersion(this ISession database, int version, string target = "main") {
			var preparedStatement = database.Prepare($@"INSERT INTO {DATABASE_VERSIONS_TABLE} (name, version) VALUES (?, ?); ");
			var boundStatement = preparedStatement.Bind(target, version);
			database.Execute(boundStatement);
		}

		public static int GetVersion(this ISession database, string target = "main") {
			var preparedStatement = database.Prepare($"SELECT * FROM {DATABASE_VERSIONS_TABLE} WHERE name = ?;");
			var boundStatement = preparedStatement.Bind(target);
			var result = database.Execute(boundStatement);
			var row = result.FirstOrDefault();
			return row is null ? -1 : row.GetValue<int>("version");
		}

	}
}
