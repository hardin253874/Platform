// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.CodeGen.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		///     Remove spaces from the source string.
		/// </summary>
		/// <param name="source">
		///     The source string whose spaces are to be removed.
		/// </param>
		/// <returns>
		///     The source string with its spaces removed.
		/// </returns>
		public static string RemoveSpaces( this string source )
		{
			if ( string.IsNullOrEmpty( source ) )
			{
				return source;
			}

			return source.Replace( " ", string.Empty );
		}

		/// <summary>
		///     Ensures the source string is in camel case.
		/// </summary>
		/// <param name="source">
		///     The source string.
		/// </param>
		/// <returns>
		///     The source string in camel case.
		/// </returns>
		public static string ToCamelCase( this string source )
		{
			if ( string.IsNullOrEmpty( source ) )
			{
				return source;
			}

			return source.Substring( 0, 1 ).ToLower( ) + source.Substring( 1 );
		}

		/// <summary>
		///     Ensures the source string is in Pascal case.
		/// </summary>
		/// <param name="source">
		///     The source string.
		/// </param>
		/// <returns>
		///     The source string in Pascal format.
		/// </returns>
		public static string ToPascalCase( this string source )
		{
			if ( string.IsNullOrEmpty( source ) )
			{
				return source;
			}

			return source.Substring( 0, 1 ).ToUpper( ) + source.Substring( 1 );
		}
	}
}