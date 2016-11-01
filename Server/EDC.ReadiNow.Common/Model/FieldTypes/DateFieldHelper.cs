// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated DateField class with behaviors.
	/// </summary>
    public class DateFieldHelper : IFieldHelper
	{
        private DateField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal DateFieldHelper(DateField fieldEntity)
        {
            _field = fieldEntity;
        }

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		public object ConvertXmlStringToObject( string xml )
		{
			return DateTime.Parse( xml );
		}

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
		public string ValidateFieldValue( object value )
		{
			if ( value == null )
			{
                if ( _field.IsRequired == true )
				{
					return string.Format( GlobalStrings.MandatoryMessageTextFormat, _field.Name );
				}

				return null;
			}

            var dValue = ( DateTime ) value;

			if ( _field.MaxDate != null && dValue > _field.MaxDate )
			{
			    string formatDate = _field.MaxDate.Value.ToString( "d" );
                return string.Format( GlobalStrings.MaximumValueMessageTextFormat, _field.Name, formatDate );
			}

			if ( _field.MinDate != null && dValue < _field.MinDate )
			{
                string formatDate = _field.MinDate.Value.ToString( "d" );
                return string.Format( GlobalStrings.MinimumValueMessageTextFormat, _field.Name, formatDate );
			}

			return null;
		}

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
            return DatabaseType.DateType;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Date;
        }
	}
}
