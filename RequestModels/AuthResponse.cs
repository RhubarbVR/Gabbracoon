using System.Text.Json.Serialization;

namespace RequestModels
{
	public sealed class AuthResponse
	{
		public string TargetTokenAsString { get; set; }
	
		[JsonIgnore]
		public long TargetToken
		{
			get => long.Parse(TargetTokenAsString);
			set => TargetTokenAsString = value.ToString();
		}

		public bool SessionToken { get; set; }
		public string AuthToken { get; set; }
	}
}
