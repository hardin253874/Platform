// Copyright 2011-2015 Global Software Innovation Pty Ltd

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     String extension methods.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		///     Gets the namespace alias.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="nameSpace">The name space.</param>
		/// <param name="alias">The alias.</param>
		public static bool GetNamespaceAlias( this string value, out string nameSpace, out string alias )
		{
			nameSpace = null;
			alias = null;

			if ( string.IsNullOrEmpty( value ) )
			{
				return false;
			}

			value = value.Trim( );

			if ( value.IndexOf( ' ' ) > 0 )
			{
				return false;
			}

			var strings = value.Split( ':' );

			if ( strings.Length == 1 )
			{
				alias = strings[ 0 ];
				nameSpace = "core";

				return true;
			}

			if ( strings.Length == 2 )
			{
				alias = strings[ 1 ];
				nameSpace = strings[ 0 ];

				return true;
			}

			return false;
		}
	}
}