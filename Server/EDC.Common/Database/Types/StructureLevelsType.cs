// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.Database.Types
{
	/// <summary>
	///     Represent a column containing selected levels in a structure view.
	///     This column is not available in tables, but is used only be queries/reports.
	/// </summary>
	[DataContract( Namespace = Constants.DataContractNamespace, IsReference = true )]
	[XmlType( Namespace = Constants.StructuredQueryNamespace )]
	public class StructureLevelsType : DatabaseType
	{
		/// <summary>
		///     Gets the display name.
		/// </summary>
		public static string DisplayName
		{
			get
			{
				return "StructureLevels";
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
			object obj = null;

			if ( !string.IsNullOrEmpty( value ) ) // Keep encoded string representation.
			{
				string temp = value;
				obj = temp;
			}

			return obj;
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
			throw new InvalidOperationException( "The specified database type cannot be converted." );
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
			string text = string.Empty;
			//TODO:  This is hack code. we need remove later
			bool valueIsEmptyString = false;
			var stringVal = value as string;

			if ( stringVal != null )
			{
				valueIsEmptyString = string.IsNullOrEmpty( stringVal );
			}

			if ( ( value != null ) && ( !( value is DBNull ) && !valueIsEmptyString ) )
			{
				var temp = ( string ) value; // keep string
				text = temp;
			}
			return text;
		}

		/// <summary>
		///     Gets the display name.
		/// </summary>
		public override string GetDisplayName( )
		{
			return DisplayName;
		}

		/// <summary>
		///     Returns an enumeration describing the equivalent SqlDbType.
		/// </summary>
		public override string GetSqlDbTypeString( )
		{
			throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", GetType( ).Name ) );
		}
	}
}