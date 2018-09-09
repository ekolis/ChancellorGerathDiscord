using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChancellorGerath
{
	public static class Extensions
	{
		/// <summary>
		/// Our friendly neighborhood random number generator.
		/// </summary>
		public static Random Random { get; } = new Random();

		/// <summary>
		/// If the user is a SocketGuildUser, get their nickname.
		/// Otherwise just use their username.
		/// </summary>
		/// <param name="u"></param>
		/// <returns></returns>
		public static string GetNicknameOrUsername(this SocketUser u)
		{
			if (u is SocketGuildUser gu)
				return gu.Nickname ?? u.Username;
			return u.Username;
		}

		/// <summary>
		/// Picks a random item from a list.
		/// </summary>
		/// <typeparam name="T">The type of item.</typeparam>
		/// <param name="list">The list.</param>
		/// <returns>A randomly chosen item from the list.</returns>
		public static T PickRandom<T>(this IEnumerable<T> list)
		{
			return list.ElementAt(Random.Next(list.Count()));
		}

		public static T PickWeighted<T>(this IDictionary<T, int> dict, Random r)
		{
			if (!dict.Any())
				throw new ArgumentException("Cannot pick a weighted item from an empty dictionary.");

			var total = dict.Sum(kvp => kvp.Value);
			var diceroll = r.Next(total);

			// TODO - this isn't really PRNG safe as dictionaries enumerate in arbitrary order
			var count = 0;
			foreach (var kvp in dict)
			{
				count += kvp.Value;
				if (diceroll < count)
					return kvp.Key;
			}

			throw new InvalidOperationException($"Failed to pick a weighted item from a dictionary containing {dict.Count} items totaling {total} weight with a dice roll of {diceroll}. Current count is {count}.");
		}

		public static string ReplaceSingle(this string fullstring, string original, string replacement)
		{
			var pos = fullstring.IndexOf(original);
			if (pos < 0)
				return fullstring;
			return fullstring.Substring(0, pos) + replacement + fullstring.Substring(pos + original.Length);
		}

		/// <summary>
		/// Shuffles a list into a random order.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
		{
			return list.Select(value => (Random.Next(), value)).OrderBy(x => x.Item1).Select(x => x.value);
		}

		public static bool ContainsRange<T>(this IEnumerable<T> haystack, IEnumerable<T> needle)
		{
			var idx = 0;
			for (var i = 0; i < haystack.Count(); i++)
			{
				if (idx == needle.Count())
					break;
				var item = haystack.ElementAt(i);
				var match = needle.ElementAt(idx);
				if (item.Equals(match))
					idx++;
				else
					idx = 0;
			}
			return idx == needle.Count();
		}
	}
}