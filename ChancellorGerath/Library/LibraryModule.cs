using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChancellorGerath.Library
{
	/// <summary>
	/// Allows the bot to say or do things in response to commands.
	/// </summary>
	public class LibraryModule : MyModuleBase<SocketCommandContext>
	{
		// !whatis se4 -> tells you a bit about se4
		[Command("whatis")]
		[Summary("Looks something up in the library.")]
		public Task WhatIsAsync([Remainder] [Summary("What to look up")] string topic)
		{
			if (Topics.ContainsKey(topic.ToLower()))
				return ReplyAsync($"{topic} is {Topics[topic]}");
			else
				return ReplyAsync($"Sorry, I don't know anything about {topic}. Try Googling it? http://www.google.com/search?q={HttpUtility.UrlEncode(topic)}");
		}

		private IDictionary<string, string> Topics { get; } = JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("Library/Topics.json"));


	}
}
