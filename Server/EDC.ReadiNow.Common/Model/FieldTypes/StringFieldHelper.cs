// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text.RegularExpressions;
using EDC.Database;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated StringField class with behaviors.
	/// </summary>
    public class StringFieldHelper : IFieldHelper
	{
        /// <summary>
        /// The maximum length that a string field can be 
        /// irrespective of how it is configured. 
        /// </summary>
        public const int RealMaximumStringFieldLength = 10 * 1000; 

		private StringField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal StringFieldHelper(StringField fieldEntity)
        {
            _field = fieldEntity;
        }

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		public object ConvertXmlStringToObject( string xml )
		{
			return xml;
		}

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
		public string ValidateFieldValue( object value )
		{
			var sValue = ( string ) value;

			if ( _field.IsRequired == true && string.IsNullOrEmpty( sValue ) )
			{
				return string.Format( GlobalStrings.MandatoryMessageTextFormat, _field.Name );
			}

			if ( sValue != null )
			{
				if ( sValue.Length > RealMaximumStringFieldLength)
				{
					return string.Format( GlobalStrings.MaximumLengthMessageTextFormat, _field.Name, RealMaximumStringFieldLength);
				}

                if (_field.MaxLength > 0 && sValue.Length > _field.MaxLength)
                {
                    return string.Format(GlobalStrings.MaximumLengthMessageTextFormat, _field.Name, _field.MaxLength);
                }

                if ( sValue.Length < _field.MinLength )
				{
					return string.Format( GlobalStrings.MinimumLengthMessageTextFormat, _field.Name, _field.MinLength );
				}

				if ( !string.IsNullOrEmpty( sValue ) )
				{
					StringPattern pattern = _field.Pattern ?? Entity.Get<StringPattern>( "core:defaultPattern" );

					// Note: Regex will only get applied if a value is set.
					// Use IsRequired to ensure that a value is set.

					string regex = pattern.Regex;
					if ( string.IsNullOrEmpty( regex ) )
					{
						throw new Exception( GlobalStrings.RegexNullExceptionMessage );
					}

					if ( !Regex.IsMatch( sValue, regex ) )
					{
						return string.Format( GlobalStrings.PatternMatchMessageTextFormat, pattern.Name );
					}
				}
			}

			return null;
		}

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
            return DatabaseType.StringType;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.String;
        }
	}

	public class StringPatternException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the StringPatternException class.
		/// </summary>
		public StringPatternException( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the StringPatternException class.
		/// </summary>
		public StringPatternException( string message )
			: base( message )
		{
		}
	}
}
