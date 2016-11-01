// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics
{
	/// <summary>
	///     Statistics helper class.
	/// </summary>
	public static class StatisticsHelper
	{
		/// <summary>
		///     The "_rowsCopied" field name
		/// </summary>
		private const String RowsCopiedFieldName = "_rowsCopied";

		/// <summary>
		///     The "_rowsCopied" field
		/// </summary>
		private static FieldInfo _rowsCopiedField;

		/// <summary>
		///     Appends the line format.
		/// </summary>
		/// <param name="stringBuilder">The string builder.</param>
		/// <param name="format">The format.</param>
		/// <param name="args">The arguments.</param>
		/// <exception cref="System.NullReferenceException">StringBuilder is null.</exception>
		public static void AppendLineFormat( this StringBuilder stringBuilder, string format, params object[] args )
		{
			if ( stringBuilder == null )
			{
				throw new NullReferenceException( "StringBuilder is null." );
			}

			stringBuilder.AppendLine( string.Format( format, args ) );
		}

		/// <summary>
		///     Capitalizes the specified enumeration value.
		/// </summary>
		/// <param name="enumValue">The enumeration value.</param>
		/// <returns></returns>
		public static string Capitalize( string enumValue )
		{
			if ( string.IsNullOrEmpty( enumValue ) )
			{
				return enumValue;
			}

			return Regex.Replace( enumValue, "(\\B[A-Z])", " $1" );
		}

		/// <summary>
		///     Determines the number of rows copied by the SqlBulkCopy class.
		/// </summary>
		/// <param name="bulkCopy">The bulk copy.</param>
		/// <returns>
		///     The number of rows copied.
		/// </returns>
		public static int RowsCopiedCount( this SqlBulkCopy bulkCopy )
		{
			if ( _rowsCopiedField == null )
			{
				_rowsCopiedField = typeof ( SqlBulkCopy ).GetField( RowsCopiedFieldName, BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance );
			}

			if ( _rowsCopiedField != null )
			{
				return ( int ) _rowsCopiedField.GetValue( bulkCopy );
			}

			return -1;
		}
	}
}