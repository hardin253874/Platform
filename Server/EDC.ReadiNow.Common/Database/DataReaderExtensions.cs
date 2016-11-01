// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Class representing the DataReaderExtensions type.
	/// </summary>
	public static class DataReaderExtensions
	{
		/// <summary>
		///     Gets the boolean.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">if set to <c>true</c> [null value].</param>
		/// <returns></returns>
		public static bool GetBoolean( this IDataReader reader, int i, bool nullValue )
		{
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetBoolean( i );
		}

		/// <summary>
		///     Gets the date time.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <param name="kind">The kind.</param>
		/// <returns></returns>
		public static DateTime GetDateTime( this IDataReader reader, int i, DateTime nullValue, DateTimeKind kind = DateTimeKind.Unspecified )
		{
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return GetDateTime( reader, i, kind );
		}

		/// <summary>
		///     Gets the date time.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="kind">The kind.</param>
		/// <returns></returns>
		public static DateTime GetDateTime( this IDataReader reader, int i, DateTimeKind kind = DateTimeKind.Unspecified )
		{
			DateTime dateTime = reader.GetDateTime( i );

			if ( kind != DateTimeKind.Unspecified && dateTime.Kind == DateTimeKind.Unspecified )
			{
				dateTime = DateTime.SpecifyKind( dateTime, kind );
			}

			return dateTime;
		}

		/// <summary>
		///     Gets the int32.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="i">The i.</param>
		/// <param name="nullValue">The null value.</param>
		/// <returns></returns>
		public static int GetInt32( this IDataReader reader, int i, int nullValue )
		{
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetInt32( i );
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
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetInt64( i );
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
			if ( reader.IsDBNull( i ) )
			{
				return nullValue;
			}

			return reader.GetString( i );
		}
	}
}