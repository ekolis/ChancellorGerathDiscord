using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath
{
	/// <summary>
	/// Extra functionality for ModuleBase.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MyModuleBase<T> : ModuleBase<T>
		where T : class, ICommandContext
	{
		/// <summary>
		/// Performs an action by sending italicized text (same as the /me user command).
		/// </summary>
		/// <param name="action">The action to perform.</param>
		/// <returns></returns>
		protected async Task<IUserMessage> ActAsync(string action)
		{
			return await ReplyAsync($"*{action}*");
		}
	}
}
