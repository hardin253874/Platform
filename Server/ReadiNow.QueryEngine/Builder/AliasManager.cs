// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Globalization;

namespace ReadiNow.QueryEngine.Builder
{
	/// <summary>
	///     Creates unique query aliases given a preferred prefix.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class AliasManager
	{
		/// <summary>
		///     Dictionary of assigned aliases.
		/// </summary>
		private readonly Dictionary<string, int> _aliases = new Dictionary<string, int>( );


		/// <summary>
		///     Creates unique query aliases given a preferred prefix.
		/// </summary>
		/// <param name="prefix">The preferred alias.</param>
		/// <returns>The prefix, possibly with a number suffixed.</returns>
		public string CreateAlias( string prefix )
		{
			// First call returns "prefix"
			// Second call returns "prefix2", etc.

			int value;
			if ( _aliases.TryGetValue( prefix, out value ) )
			{
				value++;
				_aliases[ prefix ] = value;
				return prefix + value.ToString( CultureInfo.InvariantCulture );
			}

			_aliases[ prefix ] = 1;
			return prefix;
		}
	}
}