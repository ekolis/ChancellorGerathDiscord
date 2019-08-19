using ChancellorGerath.Conversation;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
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

		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

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
			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), null);
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

			// Determine if the message is a command based ons the prefix
			if (!(message.HasCharPrefix('!', ref argPos) ||
				message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
			{
				await new Spam().TryToReplyAsync(message, context);
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