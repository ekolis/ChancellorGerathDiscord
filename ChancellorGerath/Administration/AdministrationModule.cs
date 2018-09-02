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
	/// Various administrative functionality.
	/// </summary>
	public class AdministrationModule : MyModuleBase<SocketCommandContext>
	{
		// !roll -> rolls dice
		[Command("reload")]
		[Summary("Reloads cached data from disk.")]
		public Task ReloadAsync()
		{
			Cache.ReloadAll();
			ReplyAsync("Reloaded my caches!");
			return Task.CompletedTask;
		}
	}
}