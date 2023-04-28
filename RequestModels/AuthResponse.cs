using System;
using System.Text.Json.Serialization;

namespace RequestModels
{
	public sealed class AuthResponse
	{
		public string TargetTokenAsString { get; set; }
	
		[JsonIgnore]
		public long TargetToken
		{
			get => BitConverter.ToInt64(Convert.FromBase64String(TargetTokenAsString), 0);
			set => TargetTokenAsString = Convert.ToBase64String(BitConverter.GetBytes(value));
		}

		public bool SessionToken { get; set; }
		public string AuthToken { get; set; }
	}
}
