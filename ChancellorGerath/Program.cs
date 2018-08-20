using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ChancellorGerath
{
	public class Program
	{
		public static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();

		public static string Title => "Chancellor Gerath";

		private DiscordSocketClient client;

		public async Task MainAsync()
		{
			Console.Title = Title;
			ConsoleWindow.Hide();

			var stdout = new StreamWriter(File.OpenWrite("stdout.txt"));
			stdout.AutoFlush = true;
			var stderr = new StreamWriter(File.OpenWrite("stderr.txt"));
			stderr.AutoFlush = true;
			Console.SetOut(stdout);
			Console.SetError(stderr);

			client = new DiscordSocketClient();

			var ch = new CommandHandler(client, new CommandService());
			await ch.InstallCommandsAsync();

			client.Log += Log;

			// Read the private bot token from a file.
			// If you cloned this repo and want to runthe bot, you'll need to generate your own bot token
			// at https://discordapp.com/developers/applications/
			var token = File.ReadAllText("Token.txt");
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
