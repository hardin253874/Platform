// Copyright 2011-2014 Global Software Innovation Pty Ltd

namespace ReadiMon.AddinView.Configuration
{
	/// <summary>
	///     String extension methods.
	/// </summary>
	public static class StringExtensionMethods
	{
		/// <summary>
		///     Converts a string to camel case.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The camel case version of the string.</returns>
		public static string ToCamelCase( this string value )
		{
			if ( string.IsNullOrEmpty( value ) )
			{
				return value;
			}

			return value.Substring( 0, 1 ).ToLowerInvariant( ) + value.Substring( 1 );
		}
	}
}