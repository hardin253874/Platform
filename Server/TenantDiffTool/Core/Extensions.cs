// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Extension methods
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		///     Gets the GUID.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static Guid GetGuid( this IDataReader reader, int i, Guid nullValue )
		{
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetGuid( i );
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

		/// <summary>
		///     Lefts the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="length">The length.</param>
		/// <returns></returns>
		public static string Left( this string input, int length )
		{
			if ( input == null )
			{
				return null;
			}

			if ( input.Length <= length )
			{
				return input;
			}

			return input.Substring( 0, length ) + "...";
		}
	}
}