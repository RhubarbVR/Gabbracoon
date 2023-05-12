using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;

using Whispertail_Shared;

using System.Net.Http;
using System.Reflection;

namespace Whispertail
{
	public static class MauiProgram
	{
		public static void UpdateTheme(bool isLight) {
			Console.WriteLine("Theme Change");
			LightModeManager.IsLightMode = isLight;
			LightModeManager.Update();
		}

		public static MauiApp CreateMauiApp() {
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));
			
			LightModeManager.UseWeb = false;
			builder.Services.AddScoped<LightModeManager>();

			UpdateTheme(Application.Current.RequestedTheme == AppTheme.Light);
			Application.Current.RequestedThemeChanged += (e, a) => UpdateTheme(a.RequestedTheme == AppTheme.Light);

			builder.Services.AddMauiBlazorWebView();
#if DEBUG
			builder.Services.AddBlazorWebViewDeveloperTools();
#endif		
			return builder.Build();
		}
	}
}