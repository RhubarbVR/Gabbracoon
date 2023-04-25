using Cassandra;

using Gabbracoon;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RequestModels;

namespace GabbracoonServer.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AuthenticationController : ControllerBase
	{
		private readonly Gabbracoon.IGabbracoonAuthProvider[] _authProviders = Array.Empty<Gabbracoon.IGabbracoonAuthProvider>();
		private readonly IUserAndAuthService _userAndAuthService;

		public AuthenticationController(IEnumerable<Gabbracoon.IGabbracoonAuthProvider> authProviders, IUserAndAuthService userAndAuthService) {
			_authProviders = authProviders.ToArray();
			_userAndAuthService = userAndAuthService;
		}

		[HttpPost(nameof(Authinticate))]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> Authinticate(AuthRequest request, CancellationToken cancellationToken) {
			foreach (var provider in _authProviders) {
				if (provider.Name == request.TargetProvider) {
					if (await provider.Authenticate(request, cancellationToken)) {

					}
				}
			}
			return BadRequest(new LocalText("Server.Authinticate.Failed"));
		}

		[HttpGet(nameof(Login))]
		[ProducesResponseType(typeof(PrivateUserData), StatusCodes.Status202Accepted)]
		[ProducesResponseType(typeof(MissingAuth), StatusCodes.Status100Continue)]
		public IActionResult Login([FromForm(Name = "Email")] string email) {
			return Accepted();
		}

		[HttpGet(nameof(RefreshTokens))]
		[ProducesResponseType(typeof(NullData), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(DataInHeaders), StatusCodes.Status200OK)]
		public IActionResult RefreshTokens() {
			return Accepted();
		}

		[HttpPost(nameof(Register))]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register(RegisterAccount registerAccount, CancellationToken cancellationToken) {
			var isClaimed = await _userAndAuthService.CheckIfEmailClamed(registerAccount.Email, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			if (isClaimed) {
				return BadRequest(new LocalText("Server.Register.EmailClaimed"));
			}
			cancellationToken.ThrowIfCancellationRequested();
			var userToken = await _userAndAuthService.CreateAccount(registerAccount.Email, registerAccount.Username, cancellationToken);
			return Ok(new LocalText("Server.Register.AccountCreated", userToken.ToString()));
		}

		[HttpGet(nameof(LogOut))]
		[ProducesResponseType(typeof(NullData), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status200OK)]
		public IActionResult LogOut() {
			return BadRequest();
		}

		[HttpGet(nameof(ClearAllTokens))]
		[ProducesResponseType(typeof(NullData), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status200OK)]
		public IActionResult ClearAllTokens() {
			return BadRequest();
		}
	}
}
