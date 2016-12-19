// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.Database.Types
{
	/// <summary>
	///     A 32-bit signed integer.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	public class Int32Type : NumericType<Int32>
	{
		/// <summary>
		///     Returns whether it is possible to transform this value in a manner that places values on a relative linear scale.
		/// </summary>
		public override bool CanLinearScale
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent DbType.
		/// </summary>
		public static DbType DbType
		{
			get
			{
				return DbType.Int32;
			}
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		public static string DisplayName
		{
			get
			{
				return "Int32";
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent runtime type.
		/// </summary>
		public static Type RunTimeType
		{
			get
			{
				return typeof ( Int32 );
			}
		}


		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public static SqlDbType SqlDbType
		{
			get
			{
				return SqlDbType.Int;
			}
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		/// <returns></returns>
		public static string SqlDbTypeString
		{
			get
			{
				return Enum.Format( typeof ( SqlDbType ), SqlDbType, "g" ).ToLower( );
			}
		}

		/// <summary>
		/// Converts a database value from a string encoding to its native type.
		/// </summary>
		/// <param name="value">
		///     A string containing the database value.
		/// </param>
		/// <returns>
		///     An object representing the database value.
		/// </returns>
		public override object ConvertFromString( string value )
		{
			if ( !String.IsNullOrEmpty( value ) )
			{
				int temp;

				if ( int.TryParse( value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp ) )
				{
					return temp;
				}
			}

			return null;
		}

		/// <summary>
		///     Converts the value to a type that can be embedded directly in a SQL query, inclusive of any applicable quotes and escaping.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string literal representation of the database value.
		/// </returns>
		public override string ConvertToSqlLiteral( object value )
		{
			return ConvertToSqlString( value );
		}

		/// <summary>
		///     Converts the database value from its native type to a string encoding
		///     supported by SQL server, that can be used in queries.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public override string ConvertToSqlString( object value )
		{
			string text = string.Empty;

			if ( ( value != null ) && ( !( value is DBNull ) ) )
			{
				int temp;

				if ( int.TryParse( value.ToString( ), out temp ) )
				{
					return temp.ToString( CultureInfo.InvariantCulture );
				}
			}

			return text;
		}

		/// <summary>
		///     Converts the database value from its native type to a string encoding.
		/// </summary>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public override string ConvertToString( object value )
		{
			if ( value == null || value == DBNull.Value )
			{
				return string.Empty;
			}

			string stringValue = value.ToString( );

			int temp;

			if ( int.TryParse( stringValue, out temp ) )
			{
				return temp.ToString( CultureInfo.InvariantCulture );
			}

			return string.Empty;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent DbType.
		/// </summary>
		public override DbType GetDbType( )
		{
			return DbType;
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		public override string GetDisplayName( )
		{
			return DisplayName;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent runtime type.
		/// </summary>
		public override Type GetRunTimeType( )
		{
			return RunTimeType;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public override SqlDbType GetSqlDbType( )
		{
			return SqlDbType;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		/// <returns></returns>
		public override string GetSqlDbTypeString( )
		{
			return SqlDbTypeString;
		}
	}
}