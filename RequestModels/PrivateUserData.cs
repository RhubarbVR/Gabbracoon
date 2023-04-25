using System;
using System.Collections.Generic;
using System.Text;

namespace RequestModels
{
	public sealed class PrivateUserData
	{
		public long UserId { get; set; }
		public string Username { get; set; }
		public int PlayerColor { get; set; }
		public long TotalBytes { get; set; }
		public long TotalUsedBytes { get; set; }
		public DateTimeOffset CreationDate { get; set; }
	}
}
