using System.ComponentModel.DataAnnotations;

namespace RequestModels
{
	public sealed class MissingAuth
	{
		public long TargetToken { get; set; }

		public bool SessionToken { get; set; }

		public string AuthProviderType { get; set; }
	}
}
