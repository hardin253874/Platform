// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated TimeField class with behaviors.
	/// </summary>
    public class TimeFieldHelper : IFieldHelper
	{
		private TimeField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal TimeFieldHelper(TimeField fieldEntity)
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
			if ( value == null )
			{
				if ( _field.IsRequired == true )
				{
					return string.Format( GlobalStrings.MandatoryMessageTextFormat, _field.Name );
				}

				return null;
			}
			var dValue = ( DateTime ) value;
			if ( _field.MaxTime != null && dValue > _field.MaxTime )
			{
                return string.Format(GlobalStrings.MaximumValueMessageTextFormat, _field.Name, _field.MaxTime.Value.ToString("T"));
			}

			if ( _field.MinTime != null && dValue < _field.MinTime )
			{
				return string.Format( GlobalStrings.MinimumValueMessageTextFormat, _field.Name, _field.MinTime.Value.ToString("T") );
			}

			return null;
		}

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
		    return DatabaseType.TimeType;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.Time;
        }
	}
}
