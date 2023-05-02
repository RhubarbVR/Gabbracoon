using System.Linq;

using Cassandra;

using Gabbracoon;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RequestModels;

namespace GabbracoonServer.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AuthenticationController : ControllerBase
	{
		private readonly Dictionary<string, IGabbracoonAuthProvider> _authProviders = new();
		private readonly IUserAndAuthService _userAndAuthService;

		public AuthenticationController(IEnumerable<IGabbracoonAuthProvider> authProviders, IUserAndAuthService userAndAuthService) {
			_authProviders = authProviders.ToDictionary((x) => x.Name);
			_userAndAuthService = userAndAuthService;
		}

		[HttpPost(nameof(Authinticate))]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> Authinticate(AuthRequest request, CancellationToken cancellationToken) {
			if (_authProviders.TryGetValue(request.TargetProvider, out var provider)) {
				if (await provider.Authenticate(request, cancellationToken)) {
					return Ok(new AuthResponse {
						TargetToken = request.TargetToken,
						SessionToken = await _userAndAuthService.GetAuthProvidersIsSessionToken(request.TargetToken, cancellationToken) ?? true,
						AuthToken = await _userAndAuthService.GetNewAuthToken(request.TargetToken, cancellationToken),
					});
				}
			}
			return BadRequest(new LocalText("Server.Authinticate.Failed"));
		}

		[HttpPost(nameof(Login))]
		[ProducesResponseType(typeof(LocalText), StatusCodes.Status409Conflict)]
		[ProducesResponseType(typeof(PrivateUserData), StatusCodes.Status202Accepted)]
		[ProducesResponseType(typeof(MissingAuth), StatusCodes.Status100Continue)]
		[ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
		public async Task<IActionResult> Login([FromForm(Name = "Email")] string email, [FromForm(Name = "AuthGroup")] int? authGroup, CancellationToken cancellationToken) {
			if (email is null || authGroup is null) {
				return Conflict(new LocalText("Server.Error"));
			}
			var findUser = await _userAndAuthService.GetUserIDFromEmail(email, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			if (findUser is null) {
				return Conflict(new LocalText("Server.Error"));
			}
			if (authGroup < 0) {
				return new ObjectResult(await _userAndAuthService.GetAuthProviderNames(findUser ?? 0, cancellationToken)) {
					StatusCode = StatusCodes.Status200OK
				};
			}
			var authTokens = new Dictionary<long, string>();
			if (Request.Headers.TryGetValue("RhubarbAuths", out var value)) {
				authTokens = value.ToDictionary((inputString) => long.Parse(inputString.Remove(inputString.IndexOf(' '))));
			}
			MissingAuth authProviders = null;
			var login = false;
			await foreach (var auth in _userAndAuthService.GetAuths(findUser ?? 0, cancellationToken)) {
				if (auth.group != authGroup) {
					if (authTokens.ContainsKey(auth.auth.TargetToken)) {
						return Conflict(new LocalText("Server.Error.DullLogin"));
					}
					continue;
				}
				else {
					if (authTokens.TryGetValue(auth.auth.TargetToken, out var token)) {
						if ((await _userAndAuthService.ValidateAuth(auth.auth.TargetToken, findUser, token, Request, cancellationToken)).Item1) {
							login = true;
							continue;
						}
					}
				}
				authProviders ??= auth.auth;
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (authProviders is null) {
				if (login) {
					var (user, hasLogin) = await _userAndAuthService.GetUser(findUser ?? 0, cancellationToken);
					if (!hasLogin) {
						//Todo call userData load on fetures
						await _userAndAuthService.MarkUserHasLogin(findUser ?? 0, cancellationToken);
					}
					return Accepted(user);
				}
				return Conflict(new LocalText("Server.Error"));
			}
			if (_authProviders.TryGetValue(authProviders.AuthProviderType, out var provider)) {
				await provider.RequestAuthenticate(authProviders.TargetToken, cancellationToken);
				Console.WriteLine($"Code {authProviders.TargetToken}");
				return new ObjectResult(authProviders) { StatusCode = StatusCodes.Status100Continue };
			}
			else {
				return Conflict(new LocalText("Server.Error"));
			}

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
