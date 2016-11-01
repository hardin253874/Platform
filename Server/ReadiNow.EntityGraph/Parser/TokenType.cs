// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	///     Types of tokens returned from the tokenizer
	/// </summary>
	internal enum TokenType
	{
		/// <summary>
		///     At start of token stream
		/// </summary>
		AtStart,

		/// <summary>
		///     Token is a punctuation character
		/// </summary>
		Punc,

		/// <summary>
		///     Token is an identifier. (i.e. an alias, including the colon)
		/// </summary>
		Identifier,

		/// <summary>
		///     Tokenizer has reached end of input.
		/// </summary>
		AtEnd
	}
}