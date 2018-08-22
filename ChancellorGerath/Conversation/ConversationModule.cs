using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChancellorGerath.Conversation
{
	/// <summary>
	/// Allows the bot to manipulate Markov chains to speak like a real human!
	/// </summary>
	public class ConversationModule : MyModuleBase<SocketCommandContext>
	{
		static ConversationModule()
		{
			if (Directory.Exists("Conversation"))
			{
				Console.WriteLine($"Reading markov chains from {Path.GetFullPath("Conversation/Markov.txt")}");
				if (File.Exists("Conversation/Markov.txt"))
				{
					foreach (var line in File.ReadAllLines("Conversation/Markov.txt"))
						EveryoneGenerator.ReadChain(line);
				}
				var regex = new Regex(@".*Markov\.(.*)\.txt");

				foreach (var f in Directory.GetFiles("Conversation"))
				{
					var who = regex.Match(f).Groups[1].Captures.FirstOrDefault()?.Value;
					if (who != null)
					{
						Console.WriteLine($"Reading markov chains from {Path.GetFullPath($"Conversation/Markov.{who}.txt")}");
						Generators.Add(who, new Generator(Extensions.Random));
						foreach (var line in File.ReadAllLines(f))
							Generators[who].ReadChain(line);
					}
				}
			}
		}

		private static IDictionary<string, Generator> Generators { get; set; } = new Dictionary<string, Generator>();

		private static Generator EveryoneGenerator = new Generator(Extensions.Random);

		private static IDictionary<ISocketMessageChannel, (string Who, string Quote)> MostRecentPost = new Dictionary<ISocketMessageChannel, (string Who, string Quote)>();

		private static IDictionary<(ISocketMessageChannel Channel, string Who), string> MostRecentPosts = new Dictionary<(ISocketMessageChannel Channel, string Who), string>();

		private static IDictionary<string, ICollection<string>> Quotes =
					File.Exists("Conversation/Quotes.txt") ?
						JsonConvert.DeserializeObject<IDictionary<string, ICollection<string>>>(File.ReadAllText("Conversation/Quotes.txt")) :
						new Dictionary<string, ICollection<string>>();

		// !grab who -> grabs a quote from someone.
		[Command("grab")]
		[Summary("Grabs the most recent post as a quote.")]
		public Task GrabAsync()
		{
			var channel = Context.Channel;
			if (MostRecentPost.ContainsKey(channel) && MostRecentPost[channel].Who != null && MostRecentPost[channel].Quote != null)
			{
				Grab(MostRecentPost[channel].Who, MostRecentPost[channel].Quote);
				return ReplyAsync($"Grabbed {MostRecentPost[channel].Who}'s most recent post.");
			}
			else
				return ReplyAsync("No post found to grab!");
		}

		// !grab who -> grabs a quote from someone.
		[Command("grab")]
		[Summary("Grabs a user's most recent post as a quote.")]
		public Task GrabAsync([Remainder] [Summary("Who to grab")] string who)
		{
			var channel = Context.Channel;
			if (MostRecentPosts.ContainsKey((channel, who)))
			{
				Grab(who, MostRecentPosts[(channel, who)]);
				return ReplyAsync($"Grabbed {who}'s most recent post.");
			}
			else
				return ReplyAsync("No post found to grab!");
		}

		// !jabber Bob -> generates a random quote based on Bob's chat history.
		[Command("jabber")]
		[Summary("Mimics someone's speech patterns.")]
		public Task JabberAsync([Remainder] [Summary("Who to mimic")] string who)
		{
			Generator gen;
			if (Generators.ContainsKey(who))
				gen = Generators[who];
			else
				return ReplyAsync($"I don't have any conversation history for {who}!");

			var preferredLength = Extensions.Random.Next(4, 12);
			var maxLength = preferredLength + Extensions.Random.Next(2, 6);
			return ReplyAsync(gen.WriteSentence(preferredLength, maxLength).Text);
		}

		// !jabber -> generates a random quote based on everyone's chat history.
		[Command("jabber")]
		[Summary("Mimics everyone's speech patterns.")]
		public Task JabberAsync()
		{
			var preferredLength = Extensions.Random.Next(4, 12);
			var maxLength = preferredLength + Extensions.Random.Next(2, 6);
			if (EveryoneGenerator.Lexicon.Any())
				return ReplyAsync(EveryoneGenerator.WriteSentence(preferredLength, maxLength).Text);
			else
				return ReplyAsync($"I don't have any conversation history!");
		}

		// !grab who -> grabs a quote from someone.
		[Command("quote")]
		[Summary("Retrieves a random grabbed quote.")]
		public Task QuoteAsync()
		{
			var quotes = Quotes.SelectMany(x => x.Value);
			if (Quotes.Any())
			{
				return ReplyAsync(quotes.PickRandom());
			}
			else
				return ReplyAsync("No post found to quote!");
		}

		// !grab who -> grabs a quote from someone.
		[Command("quote")]
		[Summary("Retrieves a random grabbed quote from the specified user.")]
		public Task QuoteAsync([Remainder] [Summary("Who to quote")] string who)
		{
			if (Quotes.ContainsKey(who))
			{
				return ReplyAsync(Quotes[who].PickRandom());
			}
			else
				return ReplyAsync("No post found to quote!");
		}

		internal static async Task MarkovListenAsync(SocketMessage arg)
		{
			// don't save bot commands
			if (arg.Content.StartsWith("!"))
				return;

			var who = arg.Author.GetNicknameOrUsername();
			if (!Generators.ContainsKey(who))
				Generators.Add(who, new Generator(Extensions.Random));
			Generators[who].ReadChain(arg.Content);
			if (who != "Chancellor Gerath")
				EveryoneGenerator.ReadChain(arg.Content);
			if (!Directory.Exists("Conversation"))
				Directory.CreateDirectory("Conversation");
			if (!File.Exists($"Conversation/Markov.{who}.txt"))
				File.Create($"Conversation/Markov.{who}.txt");
			var lines = File.ReadAllLines($"Conversation/Markov.{who}.txt");
			if (!lines.Contains(arg.Content))
			{
				using (var s = await GetWriteStreamAsync($"Conversation/Markov.{who}.txt"))
				{
					using (var sw = new StreamWriter(s))
					{
						sw.WriteLine(arg.Content);
						sw.Close();
					}
				}
			}
			if (who == "Chancellor Gerath")
				return; // don't save bot messages to the main generator
			if (!File.Exists($"Conversation/Markov.txt"))
				File.Create($"Conversation/Markov.txt");
			lines = File.ReadAllLines($"Conversation/Markov.txt");
			if (!lines.Contains(arg.Content))
			{
				using (var s = await GetWriteStreamAsync($"Conversation/Markov.txt"))
				{
					using (var sw = new StreamWriter(s))
					{
						sw.WriteLine(arg.Content);
						sw.Close();
					}
				}
			}

			MostRecentPost[arg.Channel] = (who, arg.Content);
			MostRecentPosts[(arg.Channel, who)] = arg.Content;
		}

		// https://stackoverflow.com/questions/1406808/wait-for-file-to-be-freed-by-process
		private static async Task<FileStream> GetWriteStreamAsync(string path)
		{
			return await Task.Run(() =>
				{
					while (true)
					{
						try
						{
							return new FileStream(path, FileMode.Append, FileAccess.Write);
						}
						catch (IOException e)
						{
							// access error
							if (e.HResult != -2147024864)
								throw;

							Thread.Sleep(100); // wait for file to be available
						}
					}
				});
		}

		private void Grab(string who, string quote)
		{
			if (!Quotes.ContainsKey(who))
				Quotes.Add(who, new HashSet<string>());
			Quotes[who].Add(quote);
			if (!Directory.Exists("Conversation"))
				Directory.CreateDirectory("Conversation");
			File.WriteAllText("Conversation/Quotes.txt", JsonConvert.SerializeObject(Quotes));
		}
	}
}