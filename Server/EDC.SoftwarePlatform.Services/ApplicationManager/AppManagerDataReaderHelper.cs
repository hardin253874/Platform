// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;

namespace EDC.SoftwarePlatform.Services.ApplicationManager
{
	/// <summary>
	///     Helper methods for working with certain types and application manager.
	/// </summary>
	internal static class AppManagerDataReaderHelper
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
			return reader.IsDBNull( i ) ? nullValue : reader.GetDateTime( i );
		}

		/// <summary>
		///     Gets the unique identifier.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static Guid GetGuid( this IDataReader reader, int i, Guid nullValue )
		{
			return reader.IsDBNull( i ) ? nullValue : reader.GetGuid( i );
		}

		/// <summary>
		///     Gets the int64.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static long GetInt64( this IDataReader reader, int i, long nullValue )
		{
			return reader.IsDBNull( i ) ? nullValue : reader.GetInt64( i );
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static string GetString( this IDataReader reader, int i, string nullValue )
		{
			return reader.IsDBNull( i ) ? nullValue : reader.GetString( i );
		}
	}
}