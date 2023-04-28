using System;
using System.Text.Json.Serialization;

namespace RequestModels
{
	public sealed class AuthRequest
	{
		public string TargetProvider { get; set; }
		public string TargetTokenAsString { get; set; }

		[JsonIgnore]
		public long TargetToken
		{
			get => BitConverter.ToInt64(Convert.FromBase64String(TargetTokenAsString), 0);
			set => TargetTokenAsString = Convert.ToBase64String(BitConverter.GetBytes(value));
		}

		public string AuthCode { get; set; }
	}
}
