using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ChancellorGerath
{
	public static class Program
	{
		public static string Title => "Chancellor Gerath";

		#region Nested classes to support running as service
		// https://stackoverflow.com/questions/7764088/net-console-application-as-windows-service
		public const string ServiceName = "MyService";

		public class Service : ServiceBase
		{
			public Service()
			{
				ServiceName = Program.ServiceName;
			}

			protected override void OnStart(string[] args)
			{
				//Thread.Sleep(10000); // XXX time to attach debugger
				Task.Run(async () => await StartAsync());
			}

			protected override void OnStop()
			{
				Program.Stop();
			}
		}
		#endregion

		private static DiscordSocketClient client;

		public static async Task Main()
		{
			if (!Environment.UserInteractive)
			{
				// running as service
				using Service service = new();
				ServiceBase.Run(service);
			}
			else
			{
				// running as a console app
				Console.Title = Title;
				ConsoleWindow.Hide();

				await StartAsync();

				// Block this task until the program is closed.
				await Task.Delay(-1);
			}
		}

		private static async Task StartAsync()
		{
			// change working directory to the location of the executable so we can find our files
			var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Directory.SetCurrentDirectory(exeDir);

			// set up logging
			var stdout = new StreamWriter(File.OpenWrite(Path.Combine(exeDir, "stdout.txt")));
			stdout.AutoFlush = true;
			var stderr = new StreamWriter(File.OpenWrite(Path.Combine(exeDir, "stderr.txt")));
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
		}

		private static void Stop()
		{

		}



		private static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
