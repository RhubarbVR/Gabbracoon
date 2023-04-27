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
			get => long.Parse(TargetTokenAsString);
			set => TargetTokenAsString = value.ToString();
		}

		public string AuthCode { get; set; }
	}
}
