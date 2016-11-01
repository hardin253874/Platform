// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using EDC.ReadiNow.Resources;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Utc;

namespace EDC.ReadiNow.Model.FieldTypes
{
	/// <summary>
	///     Extend the generated DateTimeField class with behaviors.
	/// </summary>
    public class DateTimeFieldHelper : IFieldHelper
	{
        private DateTimeField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal DateTimeFieldHelper(DateTimeField fieldEntity)
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
        public string ValidateFieldValue(object value)
        {
            if (value == null)
            {
                if (_field.IsRequired == true)
                {
                    return string.Format(GlobalStrings.MandatoryMessageTextFormat, _field.Name);
                }

                return null;
            }

            object localMaxDateTime = _field.MaxDateTime;
            object localMinDateTime = _field.MinDateTime;

            RequestContext requestContext = RequestContext.GetContext();

            if (requestContext != null)
            {
                /////
                // Get the time zone from the request context.
                /////
                string timeZone = requestContext.TimeZone;

                if (!string.IsNullOrEmpty(timeZone))
                {
                    if (_field.MaxDateTime != null)
                    {
                        localMaxDateTime = TimeZoneHelper.ConvertToLocalTime((DateTime)_field.MaxDateTime, timeZone);
                    }

                    if (_field.MinDateTime != null)
                    {
                        localMinDateTime = TimeZoneHelper.ConvertToLocalTime((DateTime)_field.MinDateTime, timeZone);
                    }
                }
            }
            var dValue = (DateTime)value;

            if (_field.MaxDateTime != null && dValue > _field.MaxDateTime)
            {
                return string.Format(GlobalStrings.MaximumValueMessageTextFormat, _field.Name, localMaxDateTime);
            }

            if (_field.MinDateTime != null && dValue < _field.MinDateTime)
            {
                return string.Format(GlobalStrings.MinimumValueMessageTextFormat, _field.Name, localMinDateTime);
            }

            return null;
        }

		/// <summary>
		///     Converts the type of to database.
		/// </summary>
		public DatabaseType ConvertToDatabaseType( )
		{
		    return DatabaseType.DateTimeType;
		}

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public DataType ConvertToDataType()
        {
            return DataType.DateTime;
        }
	}
}
