using Microsoft.AspNetCore.Mvc;

namespace GabbracoonServer.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class GabbracoonController : ControllerBase
	{
		internal static string[] _featuresList = Array.Empty<string>();

		[HttpGet(nameof(Features))]
		[ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
		public IActionResult Features() {
			return Ok(_featuresList);
		}
	}
}
