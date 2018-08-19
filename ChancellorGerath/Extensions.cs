using Discord.Commands;
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
		private static Random Random { get; } = new Random();

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
	}
}
