using Foundation;

namespace Whispertail.Platforms.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() {
			return MauiProgram.CreateMauiApp();
		}
	}
}