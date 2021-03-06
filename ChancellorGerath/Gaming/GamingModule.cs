﻿using Discord.Commands;
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
	/// Allows the bot to play games.
	/// </summary>
	public class GamingModule : MyModuleBase<SocketCommandContext>
	{
		// !roll -> rolls dice
		[Command("roll")]
		[Summary("Rolls some dice.")]
		public Task RollAsync([Remainder][Summary("Dice to roll, in the format xdy+z.")] string dice)
		{
			try
			{
				dice = dice.ToLower();

				// some special cases
				if (dice == "tide")
					return ReplyAsync(new string[] { "I'm my own grandpa!", "Incest planets are the best (for increasing reproduction)!" }.PickRandom());
				if (dice == "out")
					return ReplyAsync("Go go gadget tanks!");

				var regex = new Regex(@"([-+]?[0-9]*)d([-]?[0-9]*)(.?)(\d*)");
				var match = regex.Match(dice);

				int count;
				if (string.IsNullOrWhiteSpace(match.Groups[1].Value))
					count = 1;
				else
					count = int.Parse(match.Groups[1].Value);
				if (count == 0)
					return ReplyAsync("You now know the sound of zero dice rolling.");
				if (count < 0)
					return ReplyAsync("What, you want me to make some dice out of antimatter?");

				var sides = int.Parse(match.Groups[2].Value);

				string op;
				if (string.IsNullOrWhiteSpace(match.Groups[3].Value))
					op = "+";
				else
					op = match.Groups[3].Value ?? "+";

				int modifier;
				if (string.IsNullOrWhiteSpace(match.Groups[4].Value))
					modifier = 0;
				else
					modifier = int.Parse(match.Groups[4].Value);

				var rolls = new List<int>();
				for (var i = 0; i < count; i++)
				{
					if (sides > 0)
						rolls.Add(Extensions.Random.Next(1, sides + 1));
					else if (sides < 0)
						rolls.Add(-Extensions.Random.Next(1, -sides + 1));
					else
						rolls.Add(0);
				}
				var result = rolls.Sum();
				if (op == "+")
					result += modifier;
				else if (op == "-")
					result -= modifier;
				else if (op == "*")
					result *= modifier;
				else
					throw new InvalidOperationException($"What the heck kind of math operator is {op}?");
				if (count > 10)
					return ReplyAsync($"I rolled a bunch of dice and got {result}.");
				else
					return ReplyAsync($"I rolled {string.Join(", ", rolls)} and got {result}.");
			}
			catch (Exception ex)
			{
				return ReplyAsync("What kind of weird dice are *you* using?");
			}
		}
	}
}