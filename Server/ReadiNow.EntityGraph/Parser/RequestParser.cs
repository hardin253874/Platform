// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	///     Parses an input request and returns a term structure.
	/// </summary>
	internal class RequestParser : IRequestParser
    {
        private const string TermDescription = "alias or * or {";

		private Tokenizer _tokens;
        private Dictionary<string, List<Term>> _variables;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Repository to load schema data from.</param>
        public RequestParser(IEntityRepository entityRepository)
        {
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            EntityRepository = entityRepository;
        }


        /// <summary>
        ///     Repository to load schema data from.
        /// </summary>
        internal IEntityRepository EntityRepository { get; private set; }


        /// <summary>
        /// Parser of graph (entity member / entity-info-service) style requests.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="settings">Parse settings.</param>
        /// <returns>Request object model.</returns>
        public EntityMemberRequest ParseRequestQuery(string request, RequestParserSettings settings = null)
        {
            settings = settings ?? RequestParserSettings.Default;

            _variables = new Dictionary<string, List<Term>>();

            // Prepare tokenizer
            _tokens = new Tokenizer(request);
            _tokens.MoveNext();

            // Parse
            List<Term> result = null;
            Expect(ParseTermList(ref result), TermDescription);
            Expect(_tokens.Current.Type == TokenType.AtEnd, "end");

            // Convert parse tree to EntityMemberRequest structure
            var decorator = new TermDecorator(EntityRepository);
            decorator.Variables = _variables;
            EntityMemberRequest entityMemberRequest = decorator.ConvertToRequest(result, settings.Validate);
            entityMemberRequest.RequestString = request;
            return entityMemberRequest;
        }


        /// <summary>
        ///     Consumes the current token if it is punctuation and matches the requested type.
        ///     Returns true if the match was successful.
        /// </summary>
        internal bool Accept(string puncToken)
        {
            if (_tokens.Current.Type != TokenType.Punc)
            {
                return false;
            }
            if (_tokens.Current.Data != puncToken)
            {
                return false;
            }

            _tokens.MoveNext();
            return true;
        }

        /// <summary>
        ///     Consumes the current token if it is punctuation and matches the requested type.
        ///     Returns true if the match was successful.
        /// </summary>
        internal bool AcceptKeyword(string keyword)
        {
            if (_tokens.Current.Type != TokenType.Identifier)
            {
                return false;
            }
            if (_tokens.Current.Data != keyword)
            {
                return false;
            }

            _tokens.MoveNext();
            return true;
        }

        /// <summary>
        ///     Requires that the current token match the expected punctuation, and raises an error if it did not.
        /// </summary>
        internal void Expect( string puncToken )
		{
			Expect( Accept( puncToken ), puncToken );
		}

        /// <summary>
        ///     Raises a parse error if some condition was not met.
        /// </summary>
        internal void Expect( bool test, string expectedTerm )
		{
			if ( !test )
			{
				_tokens.RaiseError( "Expected '" + expectedTerm + "'" );
			}
		}

		/// <summary>
		///     Follows a list of term paths and returns their endpoints.
		/// </summary>
		private IEnumerable<Term> FindTails( IEnumerable<Term> terms )
		{
			var done = new HashSet<Term>( );
			var queue = new Queue<Term>( terms );

			while ( queue.Count > 0 )
			{
				Term next = queue.Dequeue( );
				if ( done.Contains( next ) )
				{
					continue;
				}
				if ( next.Children != null && next.Children.Count > 0 )
				{
					if ( next.Children != null )
					{
						foreach ( Term child in next.Children )
						{
							queue.Enqueue( child );
						}
					}
				}
				else
				{
					yield return next;
				}
			}
		}

		/// <summary>
        ///     term := '@' identifier | ['-'] ( '#' number ['*'] | identifier ['*'] | '*' )
		/// </summary>
		private bool ParseTerm( ref Term result )
		{
            bool reverseDirection = Accept("-");

			if ( Accept( "#" ) )
			{
				Expect( _tokens.Current.Type == TokenType.Identifier, "Id" );
				string idString = _tokens.Current.Data;
				long idValue;
				Expect( long.TryParse( idString, out idValue ), "Id number" );
				result = new Term
					{
						Id = idValue,
                        Reverse = reverseDirection
					};
				_tokens.MoveNext( );

				// Recursive asterisk
				result.Asterisk = Accept( "*" );
				return true;
            }

            if (Accept("@"))
            {
                result = new Term
                {
                    Identifier = _tokens.Current.Data,
                    VariableAccess = true
                };
                _tokens.MoveNext();
                return true;
            }

			if ( _tokens.Current.Type == TokenType.Identifier )
			{
			    long tmp;
                if (long.TryParse(_tokens.Current.Data, out tmp))
			        throw new Exception("Expected # before ID number.");

				result = new Term
					{
                        Identifier = _tokens.Current.Data,
                        Reverse = reverseDirection
					};
				_tokens.MoveNext( );

				// Recursive asterisk
				result.Asterisk = Accept( "*" );
				return true;
            }

            // Metadata-only questionmark
            if (Accept("?"))
            {
                result = new Term
                {
                    MetadataOnly = true,
                    Reverse = reverseDirection,  // has no particular meaning                    
                };
                return true;
            }

			// Wildcard asterisk
			if ( Accept( "*" ) )
			{
				result = new Term
					{
                        Asterisk = true,
                        Reverse = reverseDirection  // has no particular meaning
					};
				return true;
			}

			return false;
		}

		/// <summary>
		///     termChain := termSet [ '.' termSet ]*
		/// </summary>
		private bool ParseTermChain( ref List<Term> result )
		{
			// returns the head term(s), not the chain

			if ( !ParseTermSet( ref result ) )
			{
				return false;
			}

			List<Term> curTerms = result;
			List<Term> nextTerms = null;

			while ( true )
			{
				string data = _tokens.Current.Data;
				if ( !( Accept( "." ) ) )
				{
					break;
				}

				Expect( ParseTermSet( ref nextTerms ), TermDescription );
				foreach ( Term term in FindTails( curTerms ) )
				{
                    term.Children = nextTerms;
				}
				curTerms = nextTerms;
			}
			return true;
		}

		/// <summary>
        ///     termList := assignment* termChain [ ',' termChain]*
		/// </summary>
		private bool ParseTermList( ref List<Term> result )
		{
            while (ParseVariableOrAssignment())
            { }

            List<Term> chainHead = null;
			if ( !ParseTermChain( ref chainHead ) )
			{
				return false;
			}

			result = new List<Term>( );
			result.AddRange( chainHead );

			while ( Accept( "," ) )
			{
				Expect( ParseTermChain( ref chainHead ), TermDescription );
				result.AddRange( chainHead );
			}
			return true;
		}

		/// <summary>
		///     termSet := '{' termList '}'
		/// </summary>
		private bool ParseTermSet( ref List<Term> result )
		{
            Term term = null;
			if ( ParseTerm( ref term ) )
			{
				result = new List<Term>
					{
						term
					};
				return true;
			}

			if ( Accept( "{" ) )
			{
				Expect( ParseTermList( ref result ), TermDescription );
				Expect( "}" );
				return true;
			}

			return false;
		}

        /// <summary>
        ///     assignment := '@' identifier '=' termSet
        /// </summary>
        private bool ParseVariableOrAssignment()
        {
            if (!AcceptKeyword("let"))
                return false;

            Expect(Accept("@"), "Expected @");

            // Get variable
            Expect(_tokens.Current.Type == TokenType.Identifier, "Expected identifier after @");
            string variable = _tokens.Current.Data;
            _tokens.MoveNext();
            Expect(variable == variable.ToUpperInvariant(), "Variables must be uppercase");
            Expect(char.IsLetter(variable[0]), "Variables must start with @ then a letter");
            Expect(!_variables.ContainsKey(variable), "Variable " + variable + " is already defined.");
            
            Expect("=");

            List<Term> result = null;
            Expect( ParseTermList( ref result ), TermDescription );
            _variables[variable] = result;
            return true;
        }
	}
}