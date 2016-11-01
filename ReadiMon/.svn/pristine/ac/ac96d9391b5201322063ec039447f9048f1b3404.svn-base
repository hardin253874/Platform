// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Data;

namespace ReadiMon.Shared.Data
{
	/// <summary>
	///     IDataReader extensions.
	/// </summary>
	public static class DataReaderExtensions
	{
		/// <summary>
		///     Gets the boolean.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">if set to <c>true</c> [default value].</param>
		/// <returns></returns>
		public static bool GetBoolean( this IDataReader reader, int index, bool defaultValue )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			return reader.GetBoolean( index );
		}

		/// <summary>
		///     Gets the date time.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static DateTime GetDateTime( this IDataReader reader, int index, DateTime defaultValue )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			return reader.GetDateTime( index );
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static decimal GetDecimal( this IDataReader reader, int index, decimal defaultValue )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			return reader.GetDecimal( index );
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static Guid GetGuid( this IDataReader reader, int index, Guid defaultValue )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			return reader.GetGuid( index );
		}

		/// <summary>
		///     Gets the int.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static int GetInt32( this IDataReader reader, int index, int defaultValue )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			return reader.GetInt32( index );
		}

		/// <summary>
		///     Gets the int64.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static long GetInt64( this IDataReader reader, int index, long defaultValue )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			return reader.GetInt64( index );
		}

		/// <summary>
		///     Gets the string.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="index">The index.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="emptyValue">The empty value.</param>
		/// <returns></returns>
		public static string GetString( this IDataReader reader, int index, string defaultValue, string emptyValue = null )
		{
			if ( reader.IsDBNull( index ) )
			{
				return defaultValue;
			}

			string value = reader.GetString( index );

			if ( emptyValue != null && string.IsNullOrEmpty( value ) )
			{
				return emptyValue;
			}

			return value;
		}
	}
}