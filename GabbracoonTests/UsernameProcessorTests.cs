using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gabbracoon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon.Tests
{
	[TestClass()]
	public class UsernameProcessorTests
	{
		[TestMethod()]
		public void TryProcessTest() {
			Console.WriteLine(TestUsername("pokémon"));
			Console.WriteLine(TestUsername("H3ll0, W0r1d!"));
			Console.WriteLine(TestUsername("𝔧𝔢𝔞𝔫𝔞"));
			Console.WriteLine(TestUsername("𝖏𝖊𝖆𝖓𝖆"));
			Console.WriteLine(TestUsername("𝓳𝓮𝓪𝓷𝓪"));
			Console.WriteLine(TestUsername("𝕛𝕖𝕒𝕟𝕒"));
			Console.WriteLine(TestUsername("j҉e҉a҉n҉a҉"));
			Console.WriteLine(TestUsername("ʝҽąղą"));
			Console.WriteLine(TestUsername("ᒍEᗩᑎᗩ"));
			Console.WriteLine(TestUsername("ᒚᘿᗩᘉᗩ"));
			Console.WriteLine(TestUsername("j̶e̶a̶n̶a̶"));
			Console.WriteLine(TestUsername("ʝɛąŋą"));
			Console.WriteLine(TestUsername("Hëllö, çôñvêrsïóñ!"));
			Console.WriteLine(TestUsername("綿３"));
			Console.WriteLine(TestUsername("えすぺら"));
			Console.WriteLine(TestUsername("猫囃子"));
			Console.WriteLine(TestUsername("アエトリズ"));
			Console.WriteLine(TestUsername("そのへんのめりーさん"));
			Console.WriteLine(TestUsername("七篠ナギサ"));
			Console.WriteLine(TestUsername("たてがみ"));
			Console.WriteLine(TestUsername("横たう"));
			Console.WriteLine(TestUsername("羅人"));
			Console.WriteLine(TestUsername("コン助"));
			Console.WriteLine(TestUsername("ポケットモンスター"));
			Console.WriteLine(TestUsername("遊戯王"));
			Console.WriteLine(TestUsername("ハンバーグステーキ"));
			Console.WriteLine(TestUsername("​​​"));
			Console.WriteLine(TestUsername("ꜰᴀᴏʟᴀɴ"));
			Console.WriteLine(TestUsername("uɐloɐℲ"));
			Console.WriteLine(TestUsername("ඐ"));
			Console.WriteLine(TestUsername("එ"));
			Console.WriteLine(TestUsername("ඒ"));
			Console.WriteLine(TestUsername("ඓ"));
			Console.WriteLine(TestUsername("ඔ"));
			Console.WriteLine(TestUsername("ඕ"));
			Console.WriteLine(TestUsername("ඖ"));
			Console.WriteLine(TestUsername("ක"));
			Console.WriteLine(TestUsername("ඛ"));
			Console.WriteLine(TestUsername("ග"));
			Console.WriteLine(TestUsername("ඝ"));
			Console.WriteLine(TestUsername("ඞ"));
			Console.WriteLine(TestUsername("ඟ"));
			Console.WriteLine(TestUsername("ච"));
			Console.WriteLine(TestUsername("ඡ"));
			Console.WriteLine(TestUsername("ජ"));
			Console.WriteLine(TestUsername("ඣ"));
			Console.WriteLine(TestUsername("ඤ"));
			Console.WriteLine(TestUsername("ඥ"));
			Console.WriteLine(TestUsername("ඦ"));
			Console.WriteLine(TestUsername("ට"));
			Console.WriteLine(TestUsername("ඨ"));
			Console.WriteLine(TestUsername("ඩ"));
			Console.WriteLine(TestUsername("ඪ"));
			Console.WriteLine(TestUsername("ණ"));
			Console.WriteLine(TestUsername("ඬ"));
			Console.WriteLine(TestUsername("ත"));
			Console.WriteLine(TestUsername("ථ"));
			Console.WriteLine(TestUsername("ද"));
			Console.WriteLine(TestUsername("ධ"));
			Console.WriteLine(TestUsername("න"));
			Console.WriteLine(TestUsername("ඳ"));
			Console.WriteLine(TestUsername("ප"));
			Console.WriteLine(TestUsername("ඵ"));
			Console.WriteLine(TestUsername("බ"));
			Console.WriteLine(TestUsername("භ"));
			Console.WriteLine(TestUsername("ම"));
			Console.WriteLine(TestUsername("ඹ"));
			Console.WriteLine(TestUsername("ය"));
			Console.WriteLine(TestUsername("ර"));
			Console.WriteLine(TestUsername("ල"));
			Console.WriteLine(TestUsername("ඞ"));
			Console.WriteLine(TestUsername("‮0123456789fa‮nalo"));
			Console.WriteLine(TestUsername("​"));
			Console.WriteLine(TestUsername("​"));
			Console.WriteLine(TestUsername("​"));
			Console.WriteLine(TestUsername("​"));
			Console.WriteLine(TestUsername("$$b"));
		}


		public static string TestUsername(string username) {
			Console.WriteLine($"Testing {username}");
			if (!UsernameProcessor.TryProcess(username, out var result)) {
				result = "Illegal Username";
			}
			return result;
		}
    }
}