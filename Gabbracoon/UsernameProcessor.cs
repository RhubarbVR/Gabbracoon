using System.Globalization;
using System.Text;

using AnyAscii;

namespace Gabbracoon
{
	public static class UsernameProcessor
	{
		public static bool TryProcess(string inputUsername, out string result) {
			if(inputUsername.Length >= 128) {
				result = null;
				return false;
			}
			result = ConvertToAscii(inputUsername);
			result = result.ToLower();
			result = ReplaceSimilar(result);
			result = RemoveIgored(result);
			return !string.IsNullOrWhiteSpace(result);
		}


		static string ReplaceSimilar(string value) {
			var imput = new StringBuilder();
			foreach (var item in value) {
				if (item == 'i') {
					imput.Append('l');
				}
				else if (item == 'w') {
					imput.Append("vv");
				}
				else if (item == '0') {
					imput.Append('o');
				}
				else if (item == '1') {
					imput.Append('l');
				}
				else if (item == '3') {
					imput.Append('e');
				}
				else {
					imput.Append(item);
				}
			}
			return imput.ToString();
		}


		private static readonly char[] _allowedAscii = new char[] {
			'a','b','c','d','e','f','g','h','j','k','l','m','n','o','p','q','r','s','t','u','v','x','y','z','2','4','5','6','7','8','9'
		};

		static string RemoveIgored(string inputUsername) {
			var vailedChars = "";
			for (var i = 0; i < inputUsername.Length; i++) {
				if (Array.Find(_allowedAscii, (x) => x == inputUsername[i]) > 0) {
					vailedChars += inputUsername[i];
				}
			}
			return vailedChars;
		}

		static unsafe string ConvertToAscii(string input) {
			var normalized = input.Normalize(NormalizationForm.FormD);
			var newString = new StringBuilder();
			foreach (var c in normalized) {
				var uc = CharUnicodeInfo.GetUnicodeCategory(c);
				if (uc == UnicodeCategory.Format) {
					return "";
				}
				if (uc is not UnicodeCategory.ModifierLetter and not UnicodeCategory.ModifierLetter and not UnicodeCategory.NonSpacingMark and not UnicodeCategory.Control and not UnicodeCategory.EnclosingMark and not UnicodeCategory.SpacingCombiningMark) {
					newString.Append(c);
				}
			}
			return Transliteration.Transliterate(newString.ToString().Normalize(NormalizationForm.FormC));
		}

	}
}
