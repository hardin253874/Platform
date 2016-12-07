// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;

namespace ApplicationManager.Support
{
	/// <summary>
	///     Extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		///     Gets the date time.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static DateTime GetDateTime( this IDataReader reader, int i, DateTime nullValue )
		{
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetDateTime( i );
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The ordinal.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static string GetString( this IDataReader reader, int i, string nullValue )
		{
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetString( i );
		}
	}
}