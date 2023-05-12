using Android.App;
using Android.Runtime;

namespace Whispertail.Platforms.Android
{
	[Application]
	public class MainApplication : MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership) {
		}

		protected override MauiApp CreateMauiApp() {
			return MauiProgram.CreateMauiApp();
		}
	}
}