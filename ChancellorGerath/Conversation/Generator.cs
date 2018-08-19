using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChancellorGerath.Conversation
{
	/// <summary>
	/// Generates chains using a lexicon of tokens.
	/// </summary>
	public class Generator
	{
		public Generator(Random r = null)
		{
			Rng = r ?? new Random();
		}

		private Random Rng;

		/// <summary>
		/// Any tokens known to this generator.
		/// </summary>
		public TokenCollection Lexicon { get; private set; } = new TokenCollection();

		public Token BeginEndToken
		{
			get
			{
				if (!Lexicon.Contains(""))
					Lexicon.Add(new Token(""));
				return Lexicon[""];
			}
		}

		/// <summary>
		/// Builds a randomized chain using weighted values.
		/// </summary>
		/// <param name="cap"></param>
		/// <param name="spaces"></param>
		/// <param name="c"></param>
		/// <param name="preferStopAfter">How long until we ask to stop ASAP?</param>
		/// <param name="demandStopAfter">How long until we call an immediate halt to generation?</param>
		/// <returns></returns>
		public Chain WriteChain(Capitalization cap, bool spaces, int preferStopAfter, int demandStopAfter, CultureInfo c = null)
		{
			var chain = new Chain(cap, spaces, c);
			var cur = BeginEndToken;
			bool runon = false; // did we get cut off?
			do
			{
				if (!cur.Links.Any())
					throw new InvalidOperationException($"Token \"{cur}\" has no outgoing links.");
				cur = cur.Links.PickWeighted(Rng);
				if (chain.Tokens.Count() >= demandStopAfter)
				{
					cur = BeginEndToken;
					runon = true;
				}
				if (chain.Tokens.Count() >= preferStopAfter && cur.Links.ContainsKey(BeginEndToken))
				{
					cur = BeginEndToken;
					runon = true;
				}
				chain.Append(cur);
			} while (cur != BeginEndToken);
			chain.IsRunon = runon;
			return chain;
		}

		/// <summary>
		/// Writes a DromedaryCaseEnterpriseyWordFactoryFactoryImpl.
		/// Works best if the tokens in the lexicon are *words*...
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public Chain WriteDromedaryCase(int preferStopAfter, int demandStopAfter, CultureInfo c = null)
		{
			return WriteChain(Capitalization.AllTokens, false, preferStopAfter, demandStopAfter, c);
		}

		/// <summary>
		/// Writes a wordificationbobblethingydocker.
		/// Works best if the tokens in the lexicon are roughly syllable length...
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public Chain WriteWord(int preferStopAfter, int demandStopAfter, CultureInfo c = null)
		{
			return WriteChain(Capitalization.Default, false, preferStopAfter, demandStopAfter, c);
		}

		/// <summary>
		/// Writes a sentence using suitable rules for doing so.
		/// Works best if the tokens in the lexicon are *words*...
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public Chain WriteSentence(int preferStopAfter, int demandStopAfter, CultureInfo c = null)
		{
			return WriteChain(Capitalization.FirstToken, true, preferStopAfter, demandStopAfter, c);
		}

		/// <summary>
		/// Learns to write by reading.
		/// </summary>
		/// <param name="chain"></param>
		public void ReadChain(Chain chain)
		{
			for (var i = 0; i < chain.Tokens.Count() - 1; i++)
			{
				// grab 2 consecutive tokens, advancing by one each time
				var tokens = chain.Tokens.Skip(i).Take(2).ToArray();

				// in case tokens learnt from don't belong to this lexicon!
				var s1 = tokens[0].Value;
				var s2 = tokens[1].Value;

				// make a link
				LearnLink(s1, s2);
			}
		}

		/// <summary>
		/// Learns to write by reading.
		/// </summary>
		/// <param name="sentence"></param>
		/// <param name="c"></param>
		public void ReadChain(string sentence, CultureInfo c = null)
		{
			ReadChain(ParseSentence(sentence, c));
		}

		/// <summary>
		/// Learns/strengthens a link between two tokens.
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		public void LearnLink(string first, string second)
		{
			var tok1 = Lexicon?[first];
			if (tok1 == null)
				tok1 = LearnToken(first);
			var tok2 = Lexicon?[second];
			if (tok2 == null)
				tok2 = LearnToken(second);
			if (!tok1.Links.ContainsKey(tok2))
				tok1.Links.Add(tok2, 1);
			else
				tok1.Links[tok2]++;
		}

		/// <summary>
		/// Learns a new token, but doesn't actually link it to anything.
		/// </summary>
		/// <param name="s"></param>
		public Token LearnToken(string s)
		{
			if (!Lexicon.Contains(s))
			{
				var t = new Token(s);
				Lexicon.Add(t);
				return t;
			}
			return Lexicon[s];
		}

		/// <summary>
		/// Parses a chain from a sentence or phrase.
		/// Does not attempt to learn links from it (only learns tokens in isolation);
		/// learning links is done by calling <see cref="LearnToWrite(Chain)"/>.
		/// Parsing a chain without using spaces or by guessing at capitalization rules isn't really possible...
		/// Well maybe DromedaryCaseEnterpriseyJavaBeanAppletFactoryFactoryStuffImpl, but that will have to wait...
		/// </summary>
		/// <returns></returns>
		public Chain ParseSentence(string sentence, CultureInfo c = null)
		{
			// split on whitespace
			// note that we're leaving punctuation as part of the words
			// in addition to being lazy, it also makes for more realistic sentences when generating chains :D
			var words = sentence.Split(' ', '\r', '\n', '\t');

			// make a chain!
			var chain = new Chain(Capitalization.FirstToken, true, c);

			// start!
			chain.Append(BeginEndToken);

			// add ye tokens
			foreach (var word in words)
			{
				LearnToken(word);
				chain.Append(Lexicon[word]);
			}

			// end!
			chain.Append(BeginEndToken);

			// ITS OVAR
			return chain;
		}
	}
}
