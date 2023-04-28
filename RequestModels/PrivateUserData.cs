using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace RequestModels
{
	public sealed class PrivateUserData
	{
		public string UserIdAsString { get; set; }

		[JsonIgnore]
		public long UserId
		{
			get => BitConverter.ToInt64(Convert.FromBase64String(UserIdAsString), 0);
			set => UserIdAsString = Convert.ToBase64String(BitConverter.GetBytes(value));
		}

		public string Username { get; set; }
		public int PlayerColor { get; set; }
		public long TotalBytes { get; set; }
		public long TotalUsedBytes { get; set; }
	}
}
