using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath.Conversation
{
	/// <summary>
	/// A semantic token used to build chains.
	/// </summary>
	public class Token
	{
		public Token(string value)
		{
			Value = value;
		}

		/// <summary>
		/// The textual value of the token.
		/// If null, it signifies beginning or end of chain as appropriate.
		/// </summary>
		public string Value { get; private set; }

		/// <summary>
		/// Links to other tokens, by weight.
		/// </summary>
		public IDictionary<Token, int> Links { get; private set; } = new Dictionary<Token, int>();

		public override string ToString()
		{
			return Value ?? "<begin/end chain>";
		}
	}

	public class TokenCollection : KeyedCollection<string, Token>
	{
		protected override string GetKeyForItem(Token item)
		{
			return item.Value;
		}
	}
}
