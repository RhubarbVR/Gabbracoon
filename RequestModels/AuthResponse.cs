using System;
using System.Collections.Generic;
using System.Text;

namespace RequestModels
{
	public sealed class AuthResponse
	{
		public ulong TargetToken { get; set; }
		public bool SessionToken { get; set; }
		public string AuthToken { get; set; }
	}
}
