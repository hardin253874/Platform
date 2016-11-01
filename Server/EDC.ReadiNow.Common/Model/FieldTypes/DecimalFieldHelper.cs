// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Resources;

namespace EDC.ReadiNow.Model.FieldTypes
{
    /// <summary>
    ///     Extend the generated DecimalField class with behaviors.
    /// </summary>
    public class DecimalFieldHelper : IFieldHelper
    {
        private DecimalField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal DecimalFieldHelper(DecimalField fieldEntity)
        {
            _field = fieldEntity;
        }

        // Under no circumstances should any logic that applies to the 'base' be placed in these methods.
        // It won't get called if the user is holding onto an instance of a concrete type.

        /// <summary>
        ///     Converts a configuration XML field value string to an instance of its .Net type.
        /// </summary>
        public object ConvertXmlStringToObject(string xml)
        {
            return decimal.Parse(xml);
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

            decimal iValue;

            if (value is int)
                iValue = (decimal)((int)value);            // we can't rely on the implicit cast to handle as it is an object.
            else
                iValue = (decimal)value;

            if (_field.MaxDecimal != null && iValue > _field.MaxDecimal)
            {
                return string.Format(GlobalStrings.MaximumValueMessageTextFormat, _field.Name, _field.MaxDecimal);
            }

            if (_field.MinDecimal != null && iValue < _field.MinDecimal)
            {
                return string.Format(GlobalStrings.MinimumValueMessageTextFormat, _field.Name, _field.MinDecimal);
            }

            return null;
        }

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public virtual DatabaseType ConvertToDatabaseType()
        {
            return DatabaseType.DecimalType;
        }

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public virtual DataType ConvertToDataType()
        {
            return DataType.Decimal;
        }
    }
}
