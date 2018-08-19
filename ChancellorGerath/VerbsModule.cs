using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath
{
	/// <summary>
	/// Allows the bot to say or do things in response to commands.
	/// </summary>
	public class VerbsModule : ModuleBase<SocketCommandContext>
	{
		// ~say hello world -> hello world
		[Command("say")]
		[Summary("Echoes a message.")]
		public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
			=> ReplyAsync(echo);
	}
}
