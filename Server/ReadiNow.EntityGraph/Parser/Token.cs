// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	///     A token returned from the tokenizer.
	/// </summary>
	internal class Token
	{
		/// <summary>
		///     Data for token. Either the punctuation character, or the identifier name.
		/// </summary>
		public string Data;

		/// <summary>
		///     Type of token.
		/// </summary>
		public TokenType Type;

		public Token( TokenType type, string data )
		{
			Type = type;
			Data = data;
		}
	}
}