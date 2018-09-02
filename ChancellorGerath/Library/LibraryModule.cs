using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChancellorGerath.Library
{
	/// <summary>
	/// Allows the bot to provide useful or not-so-useful information.
	/// </summary>
	public class LibraryModule : MyModuleBase<SocketCommandContext>
	{
		// !whatis se4 -> tells you a bit about se4
		[Command("whatis")]
		[Summary("Looks something up in the library.")]
		public Task WhatIsAsync([Remainder] [Summary("What to look up")] string topic)
		{
			if (Topics.Data.ContainsKey(topic.ToLower()))
				return ReplyAsync($"{topic} is {Topics.Data[topic]}");
			else
				return ReplyAsync($"Sorry, I don't know anything about {topic}. Try Googling it? http://www.google.com/search?q={HttpUtility.UrlEncode(topic)}");
		}

		// !help -> gives a list of valid bot commands
		[Command("help")]
		[Summary("Gives a list of valid bot commands.")]
		public Task HelpAsync()
		{
			return ReplyAsync("I know the following commands: " + string.Join(", ", CommandDefinitions.Select(c => c.Name).Distinct()));
		}

		// !help kill -> gives info about the kill command
		[Command("help")]
		[Summary("Gives instructions on how to use a particular command.")]
		public Task HelpAsync([Remainder] [Summary("Command to get help on")] string cmd)
		{
			var defs = CommandDefinitions.Where(d => d.Name == cmd);
			return ReplyAsync(string.Join("\n", defs.Select(d => d.Name + " " + string.Join(", ", d.Parameters.Keys) + ": " + d.Summary)));
		}

		/// <summary>
		/// Loads the command definitions by reflecting on this assembly.
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<CommandDefinition> LoadCommandDefinitions()
		{
			foreach (var t in Assembly.GetExecutingAssembly().DefinedTypes)
			{
				foreach (var m in t.GetMethods())
				{
					var cmdatt = m.GetCustomAttribute<CommandAttribute>();
					if (cmdatt != null)
					{
						var name = cmdatt.Text;
						var summary = m.GetCustomAttribute<SummaryAttribute>()?.Text ?? "I don't know what that command does!";
						var def = new CommandDefinition { Name = name, Summary = summary };
						foreach (var parm in m.GetParameters())
							def.Parameters.Add(parm.Name, parm.GetCustomAttribute<SummaryAttribute>()?.Text ?? "I don't know what this parameter does!");
						yield return def;
					}
				}
			}
		}

		private List<CommandDefinition> CommandDefinitions { get; } = LoadCommandDefinitions().OrderBy(x => x.Name).ToList();

		private Cache<IDictionary<string, string>> Topics { get; } = new Cache<IDictionary<string, string>>(() => JsonConvert.DeserializeObject<IDictionary<string, string>>(File.ReadAllText("Library/Topics.json")));

		private class CommandDefinition
		{
			public string Name { get; set; }
			public string Summary { get; set; }
			public IDictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();
		}
	}
}