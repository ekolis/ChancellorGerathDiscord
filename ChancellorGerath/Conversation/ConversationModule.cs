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
				if (File.Exists("Conversation/Markov.txt"))
				{
					foreach (var line in File.ReadAllLines("Conversation/Markov.txt"))
						EveryoneGenerator.ReadChain(line);
				}
				var regex = new Regex(@"Conversation/Markov\.(.*)\.txt");

				foreach (var f in Directory.GetFiles("Conversation"))
				{
					var who = regex.Match(f).Groups[0].Captures.FirstOrDefault()?.Value;
					if (who != null)
					{
						Generators.Add(who, new Generator(Extensions.Random));
						foreach (var line in File.ReadAllLines(f))
							Generators[who].ReadChain(line);
					}
				}
			}
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

		// !jabber Bob -> generates a random quote based on Bob's chat history.
		// !jabber -> generates a random quote based on everyone's chat history.
		[Command("jabber")]
		[Summary("Mimics everyone's speech patterns.")]
		public Task JabberAsync()
		{
			var preferredLength = Extensions.Random.Next(4, 12);
			var maxLength = preferredLength + Extensions.Random.Next(2, 6);
			return ReplyAsync(EveryoneGenerator.WriteSentence(preferredLength, maxLength).Text);
		}

		private static IDictionary<string, Generator> Generators { get; set; } = new Dictionary<string, Generator>();
		private static Generator EveryoneGenerator = new Generator(Extensions.Random);

		internal static async Task ListenAsync(SocketMessage arg)
		{
			// don't save bot commands
			if (arg.Content.StartsWith("!"))
				return;

			var who = arg.Author.Username;
			if (!Generators.ContainsKey(who))
				Generators.Add(who, new Generator(Extensions.Random));
			Generators[who].ReadChain(arg.Content);
			EveryoneGenerator.ReadChain(arg.Content);
			if (!Directory.Exists("Conversation"))
				Directory.CreateDirectory("Conversation");
			if (!File.Exists($"Conversation/Markov.{who}.txt"))
				File.Create($"Conversation/Markov.{who}.txt");
			using (var s = await GetWriteStreamAsync($"Conversation/Markov.{who}.txt"))
			{
				using (var sw = new StreamWriter(s))
				{
					sw.WriteLine(arg.Content);
					sw.Close();
				}
			}
			using (var s = await GetWriteStreamAsync($"Conversation/Markov.txt"))
			{
				using (var sw = new StreamWriter(s))
				{
					sw.WriteLine(arg.Content);
					sw.Close();
				}
			}
			return;
		}

		// https://stackoverflow.com/questions/1406808/wait-for-file-to-be-freed-by-process
		private static async Task<FileStream> GetWriteStreamAsync(string path)
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
		}
	}
}
