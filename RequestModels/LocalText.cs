using System;
using System.Collections.Generic;
using System.Text;

namespace RequestModels
{
	public sealed class LocalText
	{
		public string Key { get; set; }
		public string[] Prams { get; set; }

		public LocalText(string key,params string[] prams) {
			Key = key;
			Prams = prams;
		}

		public LocalText() { }
	}
}
