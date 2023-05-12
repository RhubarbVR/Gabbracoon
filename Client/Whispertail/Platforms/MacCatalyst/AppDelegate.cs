using Foundation;

namespace Whispertail.Platforms.MacCatalyst
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() {
			return MauiProgram.CreateMauiApp();
		}
	}
}