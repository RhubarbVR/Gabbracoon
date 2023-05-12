using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Whispertail_Shared;


using System.Net.Http;
using System.Reflection;

namespace Whispertail_Web.Client
{
	public class Program
	{
		public class CookieHandler : DelegatingHandler
		{
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

				return await base.SendAsync(request, cancellationToken);
			}
		}

		public static async Task Main(string[] args) {
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");
			builder.RootComponents.Add<HeadOutlet>("head::after");

			builder.Services.AddScoped<LightModeManager>();

			builder.Services
				.AddTransient<CookieHandler>()
				.AddScoped(sp => sp
					.GetRequiredService<IHttpClientFactory>()
					.CreateClient("API"));
				

			await builder.Build().RunAsync();
		}
	}
}