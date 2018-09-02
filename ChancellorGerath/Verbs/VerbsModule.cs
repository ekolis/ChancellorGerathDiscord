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
		private Cache<string[]> AvgnAdjectives { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Adjectives.txt"));

		private Cache<string[]> AvgnBodyParts { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/BodyParts.txt"));

		private Cache<string[]> AvgnCreatures { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Creatures.txt"));

		private Cache<string[]> AvgnGames { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Games.txt"));

		private Cache<string[]> AvgnNouns { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Nouns.txt"));

		private Cache<string[]> AvgnPlaces { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Places.txt"));

		private Cache<string[]> AvgnQuotes { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Quotes.txt"));

		private Cache<string[]> AvgnStuffs { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Stuffs.txt"));

		private Cache<string[]> AvgnVerbs { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Avgn/Verbs.txt"));

		private Cache<string[]> KillAttacks { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Kill/Attacks.txt"));

		private Cache<IDictionary<string, string>> KillImmortals { get; } = new Cache<IDictionary<string, string>>(() => JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("Verbs/Kill/Immortals.json")));

		private Cache<string[]> KillWeapons { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Kill/Weapons.txt"));

		private Cache<string[]> PhongPhongs { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Phong/Phongs.txt"));

		private Cache<string[]> SmashAttacks { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Smash/Attacks.txt"));

		private Cache<string[]> SummonAdjectives { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Summon/Adjectives.txt"));

		private Cache<string[]> SummonBodyParts { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Summon/BodyParts.txt"));

		private Cache<string[]> SummonCreatures { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Summon/Creatures.txt"));

		private Cache<string[]> SummonLatinWords { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Summon/LatinWords.txt"));

		private Cache<string[]> SummonNouns { get; } = new Cache<string[]>(() => File.ReadAllLines("Verbs/Summon/Nouns.txt"));

		private Cache<string[]> SummonPlaces { get; } = File.ReadAllLines("Verbs/Summon/Places.txt");

		private Cache<string[]> SummonSpells { get; } = File.ReadAllLines("Verbs/Summon/Spells.txt");

		private Cache<string[]> SummonStuffs { get; } = File.ReadAllLines("Verbs/Summon/Stuffs.txt");

		private Cache<string[]> SummonVerbs { get; } = File.ReadAllLines("Verbs/Summon/Verbs.txt");

		// !avgn -> a randomly generated AVGN quote.
		[Command("avgn")]
		[Summary("Channels the wrath of the Angry Video Game Nerd against a randomly selected shitty game.")]
		public Task AvgnAsync()
		{
			return AvgnAsync(AvgnGames.Data.PickRandom());
		}

		// !avgn game -> a randomly generated AVGN quote about a specific game.
		[Command("avgn")]
		[Summary("Channels the wrath of the Angry Video Game Nerd against a game.")]
		public Task AvgnAsync([Remainder] [Summary("A game to lambast")] string game)
		{
			var quote = AvgnQuotes.Data.PickRandom();
			for (var i = 0; i < 10; i++) // HACK - shouldn't have more than 10 of the same tag
			{
				quote = quote.ReplaceSingle("{adjective}", AvgnAdjectives.Data.PickRandom());
				quote = quote.ReplaceSingle("{bodypart}", AvgnBodyParts.Data.PickRandom());
				quote = quote.ReplaceSingle("{creature}", AvgnCreatures.Data.PickRandom());
				quote = quote.ReplaceSingle("{game}", game);
				quote = quote.ReplaceSingle("{noun}", AvgnNouns.Data.PickRandom());
				quote = quote.ReplaceSingle("{place}", AvgnPlaces.Data.PickRandom());
				quote = quote.ReplaceSingle("{stuff}", AvgnStuffs.Data.PickRandom());
				quote = quote.ReplaceSingle("{verb}", AvgnVerbs.Data.PickRandom());
			}

			return ReplyAsync(quote);
		}

		// !kill Bob -> a random attack message directed against Bob, unless Bob is on the immortals list.
		[Command("kill")]
		[Summary("Attacks someone fatally.")]
		public Task KillAsync([Remainder] [Summary("Who to kill")] string target)
		{
			foreach (var immortal in KillImmortals.Data.Keys)
			{
				if (target.Split(' ').Select(x => x.ToLower()).Contains(immortal))
					return ReplyAsync(KillImmortals.Data[immortal]);
			}
			return ActAsync($"{KillAttacks.Data.PickRandom()} {target} with {KillWeapons.Data.PickRandom()}");
		}

		// !kill Bob -> a random phong message directed against Bob.
		[Command("phong")]
		[Summary("Does horrible, unspeakable things to someone.")]
		public Task PhongAsync([Remainder] [Summary("Who to phong")] string target)
		{
			return ActAsync($"{PhongPhongs.Data.PickRandom()} {target}");
		}

		// !smash Bob -> a random Super Smash Bros attack message directed against Bob.
		[Command("smash")]
		[Summary("Attacks someone like in Super Smash Bros.")]
		public Task SmashAsync([Remainder] [Summary("Who to smash")] string target)
		{
			return ActAsync($"{SmashAttacks.Data.PickRandom()} {target}");
		}

		// !summon what -> summons someone, if it's a user they'll be pinged.
		[Command("summon")]
		[Summary("Summons someone or something. If it's a user, they'll be pinged.")]
		public Task SummonAsync([Remainder] [Summary("What (or who) to summon")] string what)
		{
			var quote = SummonSpells.Data.PickRandom();
			var users = Context.Guild.Users.Where(u => u.GetNicknameOrUsername() == what);
			if (users.Count() == 1)
				what = users.Single().Mention;
			for (var i = 0; i < 10; i++) // HACK - shouldn't have more than 10 of the same tag
			{
				quote = quote.ReplaceSingle("{adjective}", SummonAdjectives.Data.PickRandom());
				quote = quote.ReplaceSingle("{bodypart}", SummonBodyParts.Data.PickRandom());
				quote = quote.ReplaceSingle("{creature}", SummonCreatures.Data.PickRandom());
				quote = quote.ReplaceSingle("{latin}", SummonLatinWords.Data.PickRandom());
				quote = quote.ReplaceSingle("{noun}", SummonNouns.Data.PickRandom());
				quote = quote.ReplaceSingle("{place}", SummonPlaces.Data.PickRandom());
				quote = quote.ReplaceSingle("{stuff}", SummonStuffs.Data.PickRandom());
				quote = quote.ReplaceSingle("{verb}", SummonVerbs.Data.PickRandom());
				quote = quote.ReplaceSingle("{what}", what);
			}

			return ActAsync(quote);
		}
	}
}