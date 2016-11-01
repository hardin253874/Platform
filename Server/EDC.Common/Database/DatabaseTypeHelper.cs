// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using EDC.Database.Types;

namespace EDC.Database
{
	/// <summary>
	///     Provides helper methods for interacting with database types.
	/// </summary>
	public static class DatabaseTypeHelper
	{
		/// <summary>
		///     Converts the type of the database type name to database.
		/// </summary>
		/// <param name="databaseTypeName">Name of the database type.</param>
		/// <returns>
		///     The database type that corresponds to the specified type name.
		/// </returns>
		/// <remarks>
		///     Consider using the activator here when time permits.
		/// </remarks>
		public static DatabaseType ConvertDatabaseTypeNameToDatabaseType( string databaseTypeName )
		{
			switch ( databaseTypeName )
			{
				case "BinaryType":
					return new BinaryType( );

				case "BoolType":
					return new BoolType( );

				case "CurrencyType":
					return new CurrencyType( );

				case "DateTimeType":
					return new DateTimeType( );

				case "DateType":
					return new DateType( );

				case "DecimalType":
					return new DecimalType( );

				case "GuidType":
					return new GuidType( );

				case "Int32Type":
					return new Int32Type( );

				case "IdentifierType":
					return new IdentifierType( );

				case "StringType":
					return new StringType( );

				case "StructureLevelsType":
					return new StructureLevelsType( );

				case "ChoiceRelationshipType":
					return new ChoiceRelationshipType( );

				case "InlineRelationshipType":
					return new InlineRelationshipType( );

				case "TimeType":
					return new TimeType( );

				case "UnknownType":
					return new UnknownType( );

				case "XmlType":
					return new XmlType( );

				case "AutoIncrementType":
					return new AutoIncrementType( );

				default:
					throw new InvalidOperationException( "The specified database type cannot be converted." );
			}
		}

		/// <summary>
		///     Converts from display name.
		/// </summary>
		/// <param name="displayName">The display name.</param>
		/// <returns></returns>
		public static DatabaseType ConvertFromDisplayName( string displayName )
		{
			switch ( displayName )
			{
				case "Binary":
					return new BinaryType( );

				case "Bool":
					return new BoolType( );

				case "Currency":
					return new CurrencyType( );

				case "DateTime":
					return new DateTimeType( );

				case "Date":
					return new DateType( );

				case "Decimal":
					return new DecimalType( );

				case "Guid":
					return new GuidType( );

				case "Int32":
					return new Int32Type( );

				case "Identifier":
					return new IdentifierType( );

				case "String":
					return new StringType( );

				case "StructureLevels":
					return new StructureLevelsType( );

				case "ChoiceRelationship":
					return new ChoiceRelationshipType( );

				case "InlineRelationship":
					return new InlineRelationshipType( );

				case "Time":
					return new TimeType( );

				case "Unknown":
					return new UnknownType( );

				case "Xml":
					return new XmlType( );

				case "AutoIncrement":
					return new AutoIncrementType( );

				default:
					throw new InvalidOperationException( string.Format( "The '{0}' database type cannot be converted.", displayName ) );
			}
		}

		/// <summary>
		///     Gets a format string suitable for displaying a particular type of data.
		/// </summary>
		/// <remarks>
		///     The format string must be suitable for use with string.Format(GetDisplayFormatString(type), value).
		///     That is it should expect one argument.
		/// </remarks>
		public static string GetDisplayFormatString( DatabaseType type )
		{
			if ( type is Int32Type )
			{
                return "{0}";
				//return "{0:N0}";
			}

			if ( type is DateTimeType )
			{
				return "{0:d} {0:t}";
			}

			if ( type is DateType )
			{
				return "{0:d}";
			}

			if ( type is TimeType )
			{
				return "{0:t}";
			}

			if ( type is CurrencyType )
			{
				//Change {0:c} to {0} to display correct decimal places of value
				return "{0:c}";
			}

			if ( type is DecimalType )
			{
				//change {0:N3} to {0} to display correct decimal places of value
				return "{0}";
			}

			return "{0}";
		}

		/// <summary>
		///     Determines whether the specified type is a numeric type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		///     <c>true</c> if the type is a numeric type; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTypeNumeric( DatabaseType type )
		{
			return type != null &&
			       ( type is Int32Type ||
			         type is IdentifierType ||
			         type is DecimalType ||
			         type is CurrencyType ||
					 type is AutoIncrementType );
		}

		/// <summary>
		///     Determines whether the specified type is a relationship field type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		///     <c>true</c> if the type is a numeric type; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTypeRelationshipField( DatabaseType type )
		{
			return type != null &&
			       ( type is ChoiceRelationshipType ||
			         type is InlineRelationshipType );
		}

        /// <summary>
        /// Extract the name component out of an encoded entity XML blob.
        /// </summary>
        /// <remarks>
        /// This is the format for data that is stored in ChoiceRelationship and InlineRelationship.
        /// </remarks>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string GetEntityXmlName(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return xml;
            if (xml.IndexOf("<e") == 0)
            {
                using (StringReader stringReader = new StringReader("<root>" + xml + "</root>"))
                {
                    XDocument xdoc = XDocument.Load(stringReader);
                    var entities = xdoc.Root.Descendants("e");
                    var texts = entities.Select(e => e.Attribute("text")).Select(a => a == null ? null : a.Value);
                    var result = string.Join(", ", texts.Where(t => !string.IsNullOrEmpty(t)));
                    return result;
                }
            }
            else
            {
                if (xml.IndexOf("<e") > 0)
                {
                    if (xml.IndexOf("\r") > 0)
                    {
                        System.Collections.Generic.List<string> xmlResults = new System.Collections.Generic.List<string>();
                        foreach (string subXml in xml.Split("\r".ToCharArray()))
                        {
                            string name = GetEntityXmlName(subXml);
                            if (string.IsNullOrEmpty(name)) continue;

                            xmlResults.Add(name);
                        }

                        return string.Join("\r", xmlResults);
                    }
                    else
                    {
                        string pre = xml.Substring(0, xml.IndexOf("<e"));
                        using (StringReader stringReader = new StringReader("<root>" + xml + "</root>"))
                        {
                            XDocument xdoc = XDocument.Load(stringReader);
                            var entities = xdoc.Root.Descendants("e");
                            var texts = entities.Select(e => e.Attribute("text")).Select(a => a == null ? null : a.Value);
                            var result = string.Join(", ", texts.Where(t => !string.IsNullOrEmpty(t)));
                            return pre + result;
                        }
                    }

                    
                }
                else
                {
                    return xml;
                }
            }
        }



        /// <summary>
        /// Extract the name component out of an encoded entity XML blob.
        /// </summary>
        /// <remarks>
        /// This is the format for data that is stored in ChoiceRelationship and InlineRelationship.
        /// </remarks>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static long GetEntityXmlId(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return 0;

            // Heavily optimise the case we actually expect
            if (xml.StartsWith(@"<e id="""))
            {
                int endQuote = xml.IndexOf( '"', 7 );
                string idStr = xml.Substring( 7, endQuote - 7 );
                return long.Parse( idStr );
            }

            using (StringReader stringReader = new StringReader("<root>" + xml + "</root>"))
            {
                var xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };
                using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
                {
                    XDocument xdoc = XDocument.Load(xmlReader);
                    var entities = xdoc.Root.Descendants("e");
                    long result = entities.Select(e => e.Attribute("id")).Select(a => a == null ? 0 : long.Parse(a.Value)).FirstOrDefault();
                    return result;
                }
            }
        }

		#region Inside is original code. Remove implementation of following methods from concrete classes

		#region Constants

		/// <summary>
		///     The format string used to convert a Date type to it's supported SQL string format.
		/// </summary>
		private const string SqlDateFormatString = @"yyyy-MM-dd";


		/// <summary>
		///     The format string used to convert a DateTime type to it's supported SQL string format.
		/// </summary>
		private const string SqlDateTimeFormatString = @"yyyy-MM-dd HH\:mm\:ss.fffffff";


		/// <summary>
		///     The format string used to convert a Time type to it's supported SQL string format.
		/// </summary>
		private const string SqlTimeFormatString = @"1753-01-01T{0:HH\:mm\:ss}";


		/// <summary>
		///     The format string for serialising/deserialising a Date type.
		/// </summary>
		public static readonly string DateFormatString = @"yyyy-MM-dd";


		/// <summary>
		///     The format string for serialising/deserialising a DateTime type.
		/// </summary>
		public static readonly string DateTimeFormatString = @"yyyy-MM-dd HH\:mm\:ss";

		#endregion

		/// <summary>
		///     Returns whether it is possible to transform this value in a manner that places values on a relative linear scale.
		/// </summary>
		/// <param name="databaseType">The database type to check.</param>
		/// <returns>True if it can be placed on a linar scale, otherwise false.</returns>
		public static bool CanLinearScale( DatabaseType databaseType )
		{
			if ( databaseType is DateTimeType || databaseType is DateType || databaseType is TimeType || databaseType is Int32Type
			     || databaseType is CurrencyType || databaseType is DecimalType  )
			{
				return true;
			}

			return false;
		}

		/// <summary>
		///     Converts a database value from a string encoding to its native type.
		/// </summary>
		/// <param name="type">
		///     An enumeration describing the database type.
		/// </param>
		/// <param name="value">
		///     A string containing the database value.
		/// </param>
		/// <param name="defaultDateTimeKind">
		///		The default date time kind.
		/// </param>
		/// <returns>
		///     An object representing the database value.
		/// </returns>
		public static object ConvertFromString( DatabaseType type, string value, DateTimeKind defaultDateTimeKind = DateTimeKind.Unspecified )
		{
			object obj = null;

            if (value == "" && (type is StringType || type is XmlType))
			{
				return "";
			}

			if ( !String.IsNullOrEmpty( value ) )
			{
				if ( type is BinaryType )
				{
					byte[] temp = Convert.FromBase64String( value );
					obj = temp;
				}
				else if ( type is BoolType )
				{
					bool temp = Convert.ToBoolean( value );
					obj = temp;
				}
				else if ( type is DateType )
				{
					DateTime temp;
					if ( !DateTime.TryParseExact( value, DateFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp ) )
					{
						temp = DateTime.Parse( value );
					}

					if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
					{
						temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
					}

					obj = temp;
				}
				else if ( type is DateTimeType )
				{
					DateTime temp;
					if ( !DateTime.TryParseExact( value, DateTimeFormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp ) )
					{
						temp = DateTime.Parse( value );
					}

					if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
					{
						temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
					}

					obj = temp;
				}
				else if ( type is DecimalType )
				{
                    decimal temp = 0;

                    if (Decimal.TryParse(value, out temp))
                    {
                        temp = Convert.ToDecimal(value);
                        obj = temp;
                    }
                    else
                    {
                        obj = value;
                    }
				}
				else if ( type is CurrencyType )
				{
                    decimal temp = 0;
                    if (Decimal.TryParse(value, out temp))
                    {
                        temp = Decimal.Parse(value, NumberStyles.AllowCurrencySymbol | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
                        obj = temp;
                    }
                    else
                    {
                        obj = value;
                    }
					
				}
				else if ( type is GuidType )
				{
					var temp = new Guid( value );
					obj = temp;
				}
				else if ( type is AutoIncrementType )
				{
					int temp = Convert.ToInt32( value );
					obj = temp;
				}
				else if ( type is Int32Type )
				{
					int temp = Convert.ToInt32( value );
					obj = temp;
				}
                else if (type is IdentifierType)
				{
					if ( value.Contains( ':' ) )
					{
						// Hack to support using aliases for identifier type
						obj = value;
					}
                    else if (value.StartsWith("<"))
                    {
                        long temp = DatabaseTypeHelper.GetEntityXmlId(value);
                        obj = temp;
                    }
                    else
				    {
						long temp = Convert.ToInt64( value );
						obj = temp;
					}
				}
                else if (type is StructureLevelsType || type is ChoiceRelationshipType || type is InlineRelationshipType || type is StringType || type is XmlType)
				{
					string temp = value; // Keep encoded string representation.
					obj = temp;
				}
				else if ( type is TimeType )
				{
                    // Note: .Net database uses TimeSpan for times
	                bool parsed;
                    DateTime temp;
                    // HH:mm:ss
                    // Correct/preferred format:  HH:mm:ss
                    if (value.Length == 8)
				    {
                        parsed = DateTime.TryParse("1753-01-01T" + value, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp );
				    }
				    else
				    {
                        // Legacy
				        parsed = DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp);
				    }

	                if ( parsed )
	                {
		                if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
		                {
			                temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
		                }

		                obj = temp;
	                }
				}
				else
				{
					throw new InvalidOperationException( "The specified database type cannot be converted." );
				}
			}

			return obj;
		}

		/// <summary>
		///     Converts the value to a type that can be embedded directly in a SQL query, inclusive of any applicable quotes and escaping.
		/// </summary>
		/// <param name="type">
		///     An enumeration describing the database type.
		/// </param>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string literal representation of the database value.
		/// </returns>
		public static string ConvertToSqlLiteral( DatabaseType type, object value )
		{
			string sqlString = ConvertToSqlString( type, value );

			if ( type is StringType || type is XmlType || type is GuidType )
			{
				return "'" + sqlString.Replace( "'", "''" ) + "'";
			}

			if ( type is BinaryType || type is TimeType || type is DateType || type is DateTimeType )
			{
				return "'" + sqlString + "'";
			}

			if ( type is UnknownType )
			{
				throw new InvalidOperationException( "The specified database type cannot be converted." );
			}

			return sqlString;
		}

		/// <summary>
		///     Converts the database value from its native type to a string encoding
		///     supported by SQL server, that can be used in queries.
		/// </summary>
		/// <param name="type">
		///     An enumeration describing the database type.
		/// </param>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		private static string ConvertToSqlString( DatabaseType type, object value )
		{
			string text = string.Empty;

			if ( ( value != null ) && ( !( value is DBNull ) ) )
			{
				if ( type is BinaryType )
				{
					var temp = ( byte[] ) value;
					text = Convert.ToBase64String( temp );
				}
				else if ( type is BoolType )
				{
					var temp = ( bool ) value;
					text = temp ? "1" : "0";
				}
				else if ( type is CurrencyType )
				{
					var temp = ( decimal ) value;
					text = temp.ToString( CultureInfo.InvariantCulture );
				}
				else if ( type is DateType )
				{
					var temp = ( DateTime ) value;
					text = temp.ToString( SqlDateFormatString, CultureInfo.InvariantCulture );
				}
				else if ( type is DateTimeType )
				{
					var temp = ( DateTime ) value;
					text = temp.ToString( SqlDateTimeFormatString, CultureInfo.InvariantCulture );
				}
				else if ( type is DecimalType )
				{
					var temp = ( decimal ) value;
					text = temp.ToString( CultureInfo.InvariantCulture );
				}
				else if ( type is GuidType )
				{
					var temp = ( Guid ) value;
					text = temp.ToString( "B" );
				}
				else if ( type is Int32Type )
				{
					var temp = ( int ) value;
					text = temp.ToString( CultureInfo.InvariantCulture );
				}
				else if ( type is IdentifierType )
				{
					var temp = ( long ) value;
					text = temp.ToString( CultureInfo.InvariantCulture );
				}
				else if ( type is StringType || type is XmlType )
				{
					var temp = ( string ) value;
					text = temp;
				}
				else if ( type is TimeType )
				{
					var temp = ( TimeSpan ) value;
					text = string.Format(CultureInfo.InvariantCulture, SqlTimeFormatString, temp);
				}
				else
				{
					throw new InvalidOperationException( "The specified database type cannot be converted." );
				}
			}

			return text;
		}

		/// <summary>
		///     Converts the database value from its native type to a string encoding.
		/// </summary>
		/// <param name="type">
		///     An enumeration describing the database type.
		/// </param>
		/// <param name="value">
		///     An object representing the database value.
		/// </param>
		/// <param name="defaultDateTimeKind">
		///		The default date time kind.
		/// </param>
		/// <returns>
		///     A string containing the string representation of the database value.
		/// </returns>
		public static string ConvertToString( DatabaseType type, object value, DateTimeKind defaultDateTimeKind = DateTimeKind.Unspecified )
		{
			return ConvertToString( type, value, null );
		}

		/// <summary>
		///		Converts the database value from its native type to a string encoding.
		/// </summary>
		/// <param name="type">
		///		An enumeration describing the database type.
		/// </param>
		/// <param name="value">
		///		An object representing the database value.
		/// </param>
		/// <param name="displayFormat">
		///		The display format.
		/// </param>
		/// <param name="defaultDateTimeKind">
		///		The default date time kind.
		/// </param>
		/// <returns>
		///		A string containing the string representation of the database value.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		///		The specified database type cannot be converted.
		/// </exception>
		public static string ConvertToString( DatabaseType type, object value, string displayFormat, DateTimeKind defaultDateTimeKind = DateTimeKind.Unspecified )
		{
			string text = null;
			//TODO:  This is hack code. we need remove later

            if (value == null || value is DBNull)
            {
                return null;
            }

            if (type is StringType || type is StructureLevelsType || type is XmlType)
            {
                var temp = value as string; // keep string 
                text = temp;
            }
            else if (type is BinaryType)
            {
                var temp = (byte[])value;
                text = Convert.ToBase64String(temp);
            }
            else if (type is BoolType)
            {
                var boolStringVal = value as string;

                if (boolStringVal != null)
                {
                    bool temp = bool.Parse(boolStringVal);
                    text = temp.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    var temp = (bool)value;
                    text = temp.ToString();
                }
            }
            else if (type is CurrencyType)
            {
                var currentStringVal = value as string;

                if (currentStringVal != null)
                {
                    decimal temp = decimal.Parse(currentStringVal);
                    text = temp.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    decimal temp = Convert.ToDecimal(value);
                    text = temp.ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (type is DateType)
            {
                var dateStringVal = value as string;

                if (dateStringVal != null)
                {
                    // if a date string with 'Z' is passed in then treat it as utc.
                    DateTime temp = DateTime.Parse(dateStringVal, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

	                if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
	                {
		                temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
	                }

	                text = temp.ToString(DateFormatString, CultureInfo.InvariantCulture);
                }
                else
                {
                    var temp = (DateTime)value;

					if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
					{
						temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
					}

                    text = temp.ToString(DateFormatString, CultureInfo.InvariantCulture);
                }
            }
            else if (type is DateTimeType)
            {
                var dateTimeStringVal = value as string;

                if (dateTimeStringVal != null)
                {
                    // if a date string with 'Z' is passed in then treat it as utc.
                    DateTime temp = DateTime.Parse(dateTimeStringVal, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

					if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
					{
						temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
					}

                    text = temp.ToString(DateTimeFormatString, CultureInfo.InvariantCulture);
                }
                else
                {
                    var temp = (DateTime)value;

					if ( temp.Kind == DateTimeKind.Unspecified && defaultDateTimeKind != DateTimeKind.Unspecified )
					{
						temp = DateTime.SpecifyKind( temp, defaultDateTimeKind );
					}

                    text = temp.ToString(DateTimeFormatString, CultureInfo.InvariantCulture);
                }
            }
            else if (type is DecimalType)
            {
                var decimalStringVal = value as string;

                if (decimalStringVal != null)
                {
                    decimal temp = decimal.Parse(decimalStringVal);
                    text = temp.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    decimal temp = Convert.ToDecimal(value);
                    text = temp.ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (type is GuidType)
            {
                var guidStringVal = value as string;

					if ( guidStringVal != null )
					{
						Guid temp = Guid.Parse( guidStringVal );
						text = temp.ToString( "B" );
					}
					else
					{
						var temp = ( Guid ) value;
						text = temp.ToString( "B" );
					}
				}
				else if ( type is AutoIncrementType )
				{
					var autoIncrementStringVal = value.ToString( );

					/////
					// TODO: Replace this with a Number Format parser.
					/////
					string parsed = Regex.Replace( autoIncrementStringVal, @"[^\d]", "" );

					if ( string.IsNullOrEmpty( parsed ) )
					{
						parsed = "0";
					}

					int temp = int.Parse( parsed );
					text = temp.ToString( displayFormat, CultureInfo.InvariantCulture );
				}
				else if ( type is Int32Type )
				{
				    int temp;
				    text = int.TryParse(value.ToString(), out temp) ? temp.ToString(displayFormat, CultureInfo.InvariantCulture) : value.ToString();
				}
				else if ( type is IdentifierType )
				{
					var identifierStringVal = value as string;

                if (identifierStringVal != null)
                {
                    text = identifierStringVal;
                }
                else
                {
                    long temp = Convert.ToInt64(value);
                    text = temp.ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (type is ChoiceRelationshipType || type is InlineRelationshipType)
            {
	            long temp;

	            text = long.TryParse( value.ToString( ), out temp ) ? temp.ToString( CultureInfo.InvariantCulture ) : value.ToString( );
            }
            else if (type is TimeType)
            {
                if (value is TimeSpan)
                {
                    var temp = (TimeSpan)value;
                    text = temp.ToString("c");
                }
                else if (value is DateTime)
                {
                    var temp = (DateTime)value; // Note: .Net database uses TimeSpan for times
                    text = temp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                }
                else
                {
                    DateTime temp = DateTime.Parse((string)value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal); // Note: .Net database uses TimeSpan for times
                    text = temp.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                }
            }
            else if (type is UnknownType)
            {
                text = null;
            }
            else
            {
                throw new InvalidOperationException("The specified database type cannot be converted.");
            }

            return text;
        }

		#endregion
	}
}