using System;
using System.Collections.Generic;
using System.Text;

namespace RequestModels
{
	public sealed class AuthRequest
	{
		public string TargetProvider { get; set; }
		public long TargetToken { get; set; }
		public string AuthCode { get; set; }
	}
}
