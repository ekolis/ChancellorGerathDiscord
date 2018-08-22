using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChancellorGerath.Verbs
{
	/// <summary>
	/// Allows the bot to say or do things in response to commands.
	/// </summary>
	public class VerbsModule : MyModuleBase<SocketCommandContext>
	{
		private string[] AvgnAdjectives { get; } = File.ReadAllLines("Verbs/Avgn/Adjectives.txt");

		private string[] AvgnBodyParts { get; } = File.ReadAllLines("Verbs/Avgn/BodyParts.txt");

		private string[] AvgnCreatures { get; } = File.ReadAllLines("Verbs/Avgn/Creatures.txt");

		private string[] AvgnGames { get; } = File.ReadAllLines("Verbs/Avgn/Games.txt");

		private string[] AvgnNouns { get; } = File.ReadAllLines("Verbs/Avgn/Nouns.txt");

		private string[] AvgnPlaces { get; } = File.ReadAllLines("Verbs/Avgn/Places.txt");

		private string[] AvgnQuotes { get; } = File.ReadAllLines("Verbs/Avgn/Quotes.txt");

		private string[] AvgnStuffs { get; } = File.ReadAllLines("Verbs/Avgn/Stuffs.txt");

		private string[] AvgnVerbs { get; } = File.ReadAllLines("Verbs/Avgn/Verbs.txt");

		private string[] KillAttacks { get; } = File.ReadAllLines("Verbs/Kill/Attacks.txt");

		private IDictionary<string, string> KillImmortals { get; } = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("Verbs/Kill/Immortals.json"));

		private string[] KillWeapons { get; } = File.ReadAllLines("Verbs/Kill/Weapons.txt");

		private string[] PhongPhongs { get; } = File.ReadAllLines("Verbs/Phong/Phongs.txt");

		private string[] SummonAdjectives { get; } = File.ReadAllLines("Verbs/Summon/Adjectives.txt");

		private string[] SummonBodyParts { get; } = File.ReadAllLines("Verbs/Summon/BodyParts.txt");

		private string[] SummonCreatures { get; } = File.ReadAllLines("Verbs/Summon/Creatures.txt");

		private string[] SummonLatinWords { get; } = File.ReadAllLines("Verbs/Summon/LatinWords.txt");

		private string[] SummonNouns { get; } = File.ReadAllLines("Verbs/Summon/Nouns.txt");

		private string[] SummonPlaces { get; } = File.ReadAllLines("Verbs/Summon/Places.txt");

		private string[] SummonSpells { get; } = File.ReadAllLines("Verbs/Summon/Spells.txt");

		private string[] SummonStuffs { get; } = File.ReadAllLines("Verbs/Summon/Stuffs.txt");

		private string[] SummonVerbs { get; } = File.ReadAllLines("Verbs/Summon/Verbs.txt");

		// !avgn -> a randomly generated AVGN quote.
		[Command("avgn")]
		[Summary("Channels the wrath of the Angry Video Game Nerd against a randomly selected shitty game.")]
		public Task AvgnAsync()
		{
			return AvgnAsync(AvgnGames.PickRandom());
		}

		// !avgn game -> a randomly generated AVGN quote about a specific game.
		[Command("avgn")]
		[Summary("Channels the wrath of the Angry Video Game Nerd against a game.")]
		public Task AvgnAsync([Remainder] [Summary("A game to lambast")] string game)
		{
			var quote = AvgnQuotes.PickRandom();
			for (var i = 0; i < 10; i++) // HACK - shouldn't have more than 10 of the same tag
			{
				quote = quote.ReplaceSingle("{adjective}", AvgnAdjectives.PickRandom());
				quote = quote.ReplaceSingle("{bodypart}", AvgnBodyParts.PickRandom());
				quote = quote.ReplaceSingle("{creature}", AvgnCreatures.PickRandom());
				quote = quote.ReplaceSingle("{game}", game);
				quote = quote.ReplaceSingle("{noun}", AvgnNouns.PickRandom());
				quote = quote.ReplaceSingle("{place}", AvgnPlaces.PickRandom());
				quote = quote.ReplaceSingle("{stuff}", AvgnStuffs.PickRandom());
				quote = quote.ReplaceSingle("{verb}", AvgnVerbs.PickRandom());
			}

			return ReplyAsync(quote);
		}

		// !kill Bob -> a random attack message directed against Bob, unless Bob is on the immortals list.
		[Command("kill")]
		[Summary("Attacks someone fatally.")]
		public Task KillAsync([Remainder] [Summary("Who to kill")] string target)
		{
			foreach (var immortal in KillImmortals.Keys)
			{
				if (target.Split(' ').Select(x => x.ToLower()).Contains(immortal))
					return ReplyAsync(KillImmortals[immortal]);
			}
			return ActAsync($"{KillAttacks.PickRandom()} {target} with {KillWeapons.PickRandom()}");
		}

		// !kill Bob -> a random phong message directed against Bob.
		[Command("phong")]
		[Summary("Does horrible, unspeakable things to someone.")]
		public Task PhongAsync([Remainder] [Summary("Who to phong")] string target)
		{
			return ActAsync($"{PhongPhongs.PickRandom()} {target}");
		}

		// !summon what -> summons someone, if it's a user they'll be pinged.
		[Command("summon")]
		[Summary("Summons someone or something. If it's a user, they'll be pinged.")]
		public Task SummonAsync([Remainder] [Summary("What (or who) to summon")] string what)
		{
			var quote = SummonSpells.PickRandom();
			var users = Context.Guild.Users.Where(u => u.GetNicknameOrUsername() == what);
			if (users.Count() == 1)
				what = users.Single().Mention;
			for (var i = 0; i < 10; i++) // HACK - shouldn't have more than 10 of the same tag
			{
				quote = quote.ReplaceSingle("{adjective}", SummonAdjectives.PickRandom());
				quote = quote.ReplaceSingle("{bodypart}", SummonBodyParts.PickRandom());
				quote = quote.ReplaceSingle("{creature}", SummonCreatures.PickRandom());
				quote = quote.ReplaceSingle("{latin}", SummonLatinWords.PickRandom());
				quote = quote.ReplaceSingle("{noun}", SummonNouns.PickRandom());
				quote = quote.ReplaceSingle("{place}", SummonPlaces.PickRandom());
				quote = quote.ReplaceSingle("{stuff}", SummonStuffs.PickRandom());
				quote = quote.ReplaceSingle("{verb}", SummonVerbs.PickRandom());
				quote = quote.ReplaceSingle("{what}", what);
			}

			return ActAsync(quote);
		}
	}
}