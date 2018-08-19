using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath.Verbs
{
	/// <summary>
	/// Allows the bot to say or do things in response to commands.
	/// </summary>
	public class VerbsModule : ModuleBase<SocketCommandContext>
	{
		// !kill Bob -> a random attack message directed against Bob, unless Bob is on the immortals list.
		[Command("kill")]
		[Summary("Attacks someone fatally.")]
		public Task KillAsync([Remainder] [Summary("Who to kill")] string target)
		{
			foreach (var immortal in Immortals.Keys)
			{
				if (target.Contains(immortal))
					return ReplyAsync(Immortals[immortal]);
			}
			return ReplyAsync($"*{Attacks.PickRandom()} {target} with {Weapons.PickRandom()}*");
		}

		private string[] Attacks = File.ReadAllLines("Verbs/Attacks.txt");

		private string[] Weapons = File.ReadAllLines("Verbs/Weapons.txt");

		private IDictionary<string, string> Immortals = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("Verbs/Immortals.json"));
	}
}
