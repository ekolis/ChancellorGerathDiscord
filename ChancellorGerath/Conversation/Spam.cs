﻿using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath.Conversation
{
	public class Spam
	{
		private static IDictionary<string, ICollection<string>> Triggers { get; } = JsonConvert.DeserializeObject<IDictionary<string, ICollection<string>>>(File.ReadAllText("Conversation/Spam/Triggers.json"));

		private string[] Races { get; } = File.ReadAllLines("Conversation/Spam/Races.txt");

		private static DateTimeOffset nextSpamTime;
		private static IDictionary<ISocketMessageChannel, int> spamMessageCountdowns = new Dictionary<ISocketMessageChannel, int>();

		/// <summary>
		/// Tries to reply to a spam trigger.
		/// </summary>
		public async Task TryToReplyAsync(SocketMessage messageParam, SocketCommandContext context)
		{
			var message = messageParam as SocketUserMessage;
			if (message == null)
				return;

			// not a command, try and spam a message if rate limit timer/message-counter is up and we have a reply for this message and we're not replying to ourselves
			if ((nextSpamTime == null || nextSpamTime <= DateTimeOffset.Now)
				&& (!spamMessageCountdowns.ContainsKey(message.Channel) || spamMessageCountdowns[message.Channel] <= 0)
				&& message.Author.Username != "Chancellor Gerath")
			{
				foreach (var kvp in Triggers.Shuffle())
				{
					if (messageParam.Content.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
					{
						var text = kvp.Value.PickRandom();
						text = text.Replace("{race}", Races.PickRandom());
						await context.Channel.SendMessageAsync(text);
						nextSpamTime = DateTimeOffset.Now + new TimeSpan(0, 0, Extensions.Random.Next(30, 121)); // wait between 30 seconds and 2 minutes
						spamMessageCountdowns[message.Channel] = Extensions.Random.Next(5, 16); // wait between 5 and 15 messages
						break;
					}
				}
			}
			else
			{
				// count down messages until we can spam again
				if (message.Author.Username != "Chancellor Gerath")
				{
					foreach (var chan in spamMessageCountdowns.Keys)
						spamMessageCountdowns[chan]--;
				}
			}
		}
	}
}