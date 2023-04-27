using System.ComponentModel.DataAnnotations;

using System.Text.Json.Serialization;

namespace RequestModels
{
	public sealed class MissingAuth
	{
		public string TargetTokenAsString { get; set; }

		[JsonIgnore]
		public long TargetToken
		{
			get => long.Parse(TargetTokenAsString);
			set => TargetTokenAsString = value.ToString();
		}

		public bool SessionToken { get; set; }

		public string AuthProviderType { get; set; }
	}
}
