using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath.Conversation
{
	/// <summary>
	/// A chain of tokens.
	/// </summary>
	public class Chain
	{
		public Chain(Capitalization cap, bool spaces, CultureInfo c = null)
		{
			Capitalization = cap;
			UseSpaces = spaces;
			Culture = c;
		}

		/// <summary>
		/// The sequence of tokens in this chain.
		/// </summary>
		public IEnumerable<Token> Tokens => tokens; // I knew there was a syntax for lambda getters... even easier than I thought! :D

		private IList<Token> tokens = new List<Token>();

		/// <summary>
		/// Capitalization rules for this chain.
		/// </summary>
		public Capitalization Capitalization { get; set; }

		/// <summary>
		/// Culture used for capitalization, or null to use the current culture.
		/// </summary>
		public CultureInfo Culture { get; set; }

		/// <summary>
		/// Insert spaces between tokens?
		/// </summary>
		public bool UseSpaces { get; set; }

		/// <summary>
		/// Adds a token to the end of the chain.
		/// </summary>
		/// <param name="t"></param>
		public void Append(Token t)
		{
			tokens.Add(t);
		}

		/// <summary>
		/// Adds a token to the end of the chain.
		/// </summary>
		/// <param name="word"></param>
		/// <param name="lexicon"></param>
		public void Append(string word, TokenCollection lexicon)
		{
			Append(lexicon[word]);
		}

		public string Text
		{
			get
			{
				return string.Join(UseSpaces ? " " : "", Tokens.Select((t, i) =>
				{
					if (string.IsNullOrWhiteSpace(t.Value))
						return "";
					if (Capitalization == Capitalization.AllTokens || Capitalization == Capitalization.FirstToken && i == 0)
						return char.ToUpper(t.Value[0], Culture ?? CultureInfo.CurrentCulture) + t.Value.Substring(1);
					return t.Value;
				}).ToArray()).Trim() + (IsRunon ? "..." : "");
			}
		}

		/// <summary>
		/// Did this chain get cut off in generation due to being too long?
		/// It will be displayed with ellipses if it did...
		/// </summary>
		public bool IsRunon { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}

	public enum Capitalization
	{
		/// <summary>
		/// tokens are left alone, Capitalized or uncapitalized.
		/// </summary>
		Default,
		/// <summary>
		/// The first token is capitalized; all others are left alone.
		/// </summary>
		FirstToken,
		/// <summary>
		/// All Tokens Are Capitalized.
		/// </summary>
		AllTokens
	}
}
