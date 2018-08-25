using ChancellorGerath.Conversation;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath
{
	public class CommandHandler
	{
		public CommandHandler(DiscordSocketClient client, CommandService commands)
		{
			_commands = commands;
			_client = client;
		}

		private static IDictionary<string, ICollection<string>> Spam { get; } = JsonConvert.DeserializeObject<IDictionary<string, ICollection<string>>>(File.ReadAllText("Conversation/Spam.json"));
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		private DateTimeOffset nextSpamTime;

		public async Task InstallCommandsAsync()
		{
			// Hook the MessageReceived event into our command handler
			_client.MessageReceived += HandleCommandAsync;

			// Hook up our chitchat listener
			_client.MessageReceived += ConversationModule.MarkovListenAsync;

			// Here we discover all of the command modules in the entry
			// assembly and load them. Starting from Discord.NET 2.0, a
			// service provider is required to be passed into the
			// module registration method to inject the
			// required dependencies.
			//
			// If you do not use Dependency Injection, pass null.
			// See Dependency Injection guide for more information.
			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly());
		}

		private async Task HandleCommandAsync(SocketMessage messageParam)
		{
			// Don't process the command if it was a system message
			var message = messageParam as SocketUserMessage;
			if (message == null) return;

			// Create a number to track where the prefix ends and the command begins
			int argPos = 0;

			// Create a WebSocket-based command context based on the message
			var context = new SocketCommandContext(_client, message);

			// Determine if the message is a command based on the prefix
			if (!(message.HasCharPrefix('!', ref argPos) ||
				message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
			{
				// not a command, try and spam a message if rate limit timer is up and we have a reply for this message and we're not replying to ourselves
				if ((nextSpamTime == null || nextSpamTime <= DateTimeOffset.Now) && message.Author.Username != "Chancellor Gerath")
				{
					foreach (var kvp in Spam.Shuffle())
					{
						if (messageParam.Content.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
						{
							await context.Channel.SendMessageAsync(kvp.Value.PickRandom());
							nextSpamTime = DateTimeOffset.Now + new TimeSpan(0, 1, 0); // wait one minute
							break;
						}
					}
				}
			}
			else
			{
				// Execute the command with the command context we just
				// created, along with the service provider for precondition checks.
				// Optionally, we may inform the user if the command fails
				// to be executed; however, this may not always be desired,
				// as it may clog up the request queue should a user spam a
				// command.
				// if (!result.IsSuccess)
				// await context.Channel.SendMessageAsync(result.ErrorReason);
				// Keep in mind that result does not indicate a return value
				// rather an object stating if the command executed successfully.
				var result = await _commands.ExecuteAsync(
						   context: context,
						   argPos: argPos,
						   services: null);
			}
		}
	}
}