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
	public class VerbsModule : MyModuleBase<SocketCommandContext>
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
			return ActAsync($"{Attacks.PickRandom()} {target} with {Weapons.PickRandom()}*");
		}

		// !kill Bob -> a random phong message directed against Bob.
		[Command("phong")]
		[Summary("Does horrible, unspeakable things to someone,..")]
		public Task PhongAsync([Remainder] [Summary("Who to phong")] string target)
		{
			return ActAsync($"{Phongs.PickRandom()} {target}");
		}

		private string[] Attacks { get; } = File.ReadAllLines("Verbs/Attacks.txt");

		private string[] Weapons { get; } = File.ReadAllLines("Verbs/Weapons.txt");

		private string[] Phongs { get; } = File.ReadAllLines("Verbs/Phongs.txt");

		private IDictionary<string, string> Immortals { get; } = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("Verbs/Immortals.json"));


	}
}
