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
			// try to convert legacy posts
			if (ConvertLegacyPosts())
			{
				// if there were legacy posts, save them in the new format and archive the originals
				SavePosts();
				Directory.Move(ModuleDirectory, $"{ModuleDirectory}.bak");
				Directory.CreateDirectory(ModuleDirectory);
				File.Move($"{ModuleDirectory}.bak/{PostsJsonFilename}", PostsJsonPath);
			}
		}

		private static IDictionary<string, Generator> Generators { get; set; } = new Dictionary<string, Generator>();

		private static Generator EveryoneGenerator;

		/// <summary>
		/// All known posts.
		/// </summary>
		private static Cache<ISet<Post>> Posts { get; } = new Cache<ISet<Post>>(LoadPosts);

		private IEnumerable<Post> GetPostsInCurrentServer()
			=> Posts.Data.Where(q => q.Server == Context.Guild.Name);

		private IEnumerable<Post> GetPostsInChannelOnCurrentServer(string channel)

			=> GetPostsInCurrentServer().Where(q => q.Channel == channel);

		// !grab -> grabs the most recent post.
		[Command("grab")]
		[Summary("Grabs the most recent post as a quote.")]
		public Task GrabAsync()
		{
			var channel = Context.Channel;
			var mostRecentPost = GetPostsInCurrentServer().OrderByDescending(q => q.Timestamp ?? DateTime.MinValue).FirstOrDefault();
			if (mostRecentPost is not null)
			{
				Grab(mostRecentPost);
				return ReplyAsync($"Grabbed {mostRecentPost.User}'s most recent post.");
			}
			else
				return ReplyAsync("No post found to grab!");
		}

		// !grab who -> grabs a quote from someone.
		[Command("grab")]
		[Summary("Grabs a user's most recent post as a quote.")]
		public Task GrabAsync([Remainder][Summary("Who to grab")] string who)
		{
			var channel = Context.Channel;
			var mostRecentPost = GetPostsInChannelOnCurrentServer(Context.Channel.Name).Where(q => q.User == who).OrderByDescending(q => q.Timestamp ?? DateTime.MinValue).FirstOrDefault();
			if (mostRecentPost is not null)
			{
				Grab(mostRecentPost);
				return ReplyAsync($"Grabbed {who}'s most recent post.");
			}
			else
				return ReplyAsync("No post found to grab!");
		}

		// !jabber Bob -> generates a random quote based on Bob's chat history.
		[Command("jabber")]
		[Summary("Mimics someone's speech patterns.")]
		public Task JabberAsync([Remainder][Summary("Who to mimic")] string who)
		{
			Generator gen;
			if (!Generators.ContainsKey(who))
				Generators.Add(who, CreateMarkovGenerator(who));
			gen = Generators[who];

			if (!gen.Lexicon.Any() || !IsUserKnownInCurrentServer(who))
				return ReplyAsync($"I don't have any conversation history for {who}!");

			var preferredLength = Extensions.Random.Next(8, 32);
			var maxLength = preferredLength + Extensions.Random.Next(8, 32);
			return ReplyAsync(gen.WriteSentence(preferredLength, maxLength).Text);
		}

		// !jabber -> generates a random quote based on everyone's chat history.
		[Command("jabber")]
		[Summary("Mimics everyone's speech patterns.")]
		public Task JabberAsync()
		{
			var preferredLength = Extensions.Random.Next(8, 32);
			var maxLength = preferredLength + Extensions.Random.Next(8, 32);
			if (EveryoneGenerator is null)
				EveryoneGenerator = CreateMarkovGenerator(null);
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
			var quotes = Posts.Data.Where(q => q.IsQuoted);
			if (quotes.Any())
			{
				return ReplyAsync(quotes.PickRandom().Content);
			}
			else
				return ReplyAsync("No post found to quote!");
		}

		// !grab who -> grabs a quote from someone.
		[Command("quote")]
		[Summary("Retrieves a random grabbed quote from the specified user.")]
		public Task QuoteAsync([Remainder][Summary("Who to quote")] string who)
		{
			var quotes = Posts.Data.Where(q => q.IsQuoted && q.User == who);
			if (quotes.Any() && IsUserKnownInCurrentServer(who))
			{
				return ReplyAsync(quotes.PickRandom().Content);
			}
			else
				return ReplyAsync($"No post from {who} found to quote!");
		}

		internal static async Task MarkovListenAsync(SocketMessage arg, string server)
		{
			// don't save bot commands
			if (arg.Content.StartsWith("!"))
				return;

			var who = arg.Author.GetNicknameOrUsername();

			Posts.Data.Add(new Post(server, arg.Channel.Name, who, arg.Content, arg.Timestamp.ToUniversalTime().UtcDateTime));

			if (!Generators.ContainsKey(who))
				Generators.Add(who, CreateMarkovGenerator(who));
			Generators[who].ReadChain(arg.Content);
			if (who != "Chancellor Gerath")
			{
				if (EveryoneGenerator is null)
					EveryoneGenerator = CreateMarkovGenerator(null);
				EveryoneGenerator.ReadChain(arg.Content);
			}
			if (!Directory.Exists(PostsJsonPath))
				Directory.CreateDirectory(PostsJsonPath);
			SavePosts();
		}

		/// <summary>
		/// Creates a Markov chain generator.
		/// </summary>
		/// <param name="who">The user whose posts we want to read, or null to read all posts.</param>
		/// <returns>The generator.</returns>
		private static Generator CreateMarkovGenerator(string? who)
		{
			IEnumerable<Post> posts = Posts.Data;
			var generator = new Generator(Extensions.Random);
			if (who is not null)
				posts = posts.Where(q => q.User == who);
			foreach (var post in posts)
				generator.ReadChain(post.Content);
			return generator;

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

		private void Grab(Post post)
		{
			var quote = post with { IsQuoted = true };
			Posts.Data.Add(quote);
			Posts.Data.Remove(post);
			SavePosts();
		}

		private static bool ConvertLegacyPosts()
		{
			if (Directory.Exists(ModuleDirectory) && !File.Exists(PostsJsonPath))
			{
				// load the old chains
				var regex = new Regex(@".*Markov\.(.*)\.txt");
				foreach (var f in Directory.GetFiles(ModuleDirectory))
				{
					var who = regex.Match(f).Groups[1].Captures.FirstOrDefault()?.Value;
					if (who != null)
					{
						Console.WriteLine($"Reading markov chains from {Path.GetFullPath($"{ModuleDirectory}/Markov.{who}.txt")}");
						Generators.Add(who, new Generator(Extensions.Random));
						foreach (var line in File.ReadAllLines(f))
						{
							Posts.Data.Add(new Post(LegacyServer, UnknownChannel, who, line, null));
						}
					}
				}

				// load the old quotes
				var quotes = JsonConvert.DeserializeObject<IDictionary<string, ICollection<string>>>(File.ReadAllText($"{ModuleDirectory}/Quotes.txt"));
				var quotedPosts = new HashSet<Post>();
				var removePosts = new HashSet<Post>();
				foreach (var quotedUser in quotes)
				{
					var (quotedUsername, userQuotes) = (quotedUser.Key, quotedUser.Value);
					foreach (var quote in userQuotes)
					{
						foreach (var post in Posts.Data.Where(q => q.Server == LegacyServer && q.User == quotedUsername && q.Content == quote))
						{
							quotedPosts.Add(post with { IsQuoted = true });
							removePosts.Add(post);
						}
					}
				}
				foreach (var quotedPost in quotedPosts)
					Posts.Data.Add(quotedPost);
				foreach (var removePost in removePosts)
					Posts.Data.Remove(removePost);

				// converted
				return true;
			}
			else
			{
				// didn't convert
				return false;
			}
		}

		/// <summary>
		/// Loads posts from disk.
		/// </summary>
		/// <returns></returns>
		private static ISet<Post> LoadPosts()
		{
			if (Directory.Exists(ModuleDirectory))
			{
				Console.WriteLine($"Reading posts from {Path.GetFullPath(PostsJsonPath)}");
				if (File.Exists(PostsJsonPath))
				{
					var json = File.ReadAllText(PostsJsonPath);
					return JsonConvert.DeserializeObject<HashSet<Post>>(json);
				}
				else
				{
					// no posts found
					return new HashSet<Post>();
				}
			}
			else
			{
				// no posts found
				return new HashSet<Post>();
			}
		}

		/// <summary>
		/// Saves posts to disk.
		/// </summary>
		private static void SavePosts()
		{
			var json = JsonConvert.SerializeObject(Posts.Data);
			if (!Directory.Exists(ModuleDirectory))
				Directory.CreateDirectory(ModuleDirectory);
			Console.WriteLine($"Writing posts to {Path.GetFullPath(PostsJsonPath)}");
			File.WriteAllText(PostsJsonPath, json);
		}

		private const string ModuleDirectory = "Conversation";

		private const string PostsJsonFilename = "Posts.json";

		private const string PostsJsonPath = ModuleDirectory + "/" + PostsJsonFilename;

		private const string LegacyServer = "Space Empires";

		private const string UnknownChannel = "(unknown)";

		/// <summary>
		/// Has the user posted to our knowledge in the current server?
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		private bool IsUserKnownInCurrentServer(string user)
		{
			return Posts.Data.Any(q => q.User == user && q.Server == Context.Guild.Name);
		}
	}
}