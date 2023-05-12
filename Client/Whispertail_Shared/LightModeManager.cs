using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.JSInterop;

namespace Whispertail_Shared
{
	public class LightModeManager
	{
		public static event Action LightModeChange;
		public static bool IsLightMode = false;

		public static bool UseWeb = true;

		private readonly IJSRuntime _js;

		public LightModeManager(IJSRuntime js) {
			_js = js;
			LightModeChange += LightModeManager_LightModeChange;
			LightModeManager_LightModeChange();
		}

		private void LightModeManager_LightModeChange() {
			if (!UseWeb) {
				_js.InvokeVoidAsync("UpdateTheme", IsLightMode);
			}
		}

		public static void Update() {
			LightModeChange?.Invoke();
		}
	}
}
