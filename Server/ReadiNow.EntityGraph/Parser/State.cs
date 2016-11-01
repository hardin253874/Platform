// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	///     Tokenizer states.
	/// </summary>
	internal enum State
	{
		// Ready to start finding next token, or yet to find start of next token
		Ready,

		// Within an identifier (e.g. an alias)
		Identifier,

		// Within a comment starting with two slashes
		SingleComment,

		// Within a comment starting with slash star
		MultiComment,

		// Reached end of current token
		Done
	}

}