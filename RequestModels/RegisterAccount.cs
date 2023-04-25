using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace RequestModels
{
	public sealed class RegisterAccount
	{
		[Required]
		[MaxLength(256)]
		[MinLength(3)]
		public string Username { set; get; }

		[Required]
		[EmailAddress]
		[MaxLength(256)]
		public string Email { set; get; }
	}
}
