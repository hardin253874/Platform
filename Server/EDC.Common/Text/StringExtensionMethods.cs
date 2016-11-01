// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Text
{
	/// <summary>
	/// </summary>
	public static class StringExtensionMethods
	{
		/// <summary>
		///     Determines whether the specified string contains control chars.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///     <c>true</c> if the specified string contains control chars; otherwise, <c>false</c>.
		/// </returns>
		public static bool ContainsControlChars( this string value )
		{
			bool containsControlChars = false;

			if ( !string.IsNullOrEmpty( value ) )
			{
				foreach ( char c in value )
				{
					containsControlChars = char.IsControl( c ) && !char.IsWhiteSpace( c );
					if ( containsControlChars )
					{
						break;
					}
				}
			}

			return containsControlChars;
		}
	}
}