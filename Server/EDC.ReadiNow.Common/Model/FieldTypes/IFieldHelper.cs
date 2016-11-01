// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;

namespace EDC.ReadiNow.Model.FieldTypes
{
    /// <summary>
    ///     Interface for all polymorphic operations on Field.
    /// </summary>
    public interface IFieldHelper
    {
        /// <summary>
        ///     Converts a field into a database type object.
        /// </summary>
        DatabaseType ConvertToDatabaseType();

        /// <summary>
        ///     Converts a field into a database type object.
        /// </summary>
        DataType ConvertToDataType();

        /// <summary>
        ///     Converts a configuration XML field value string to an instance of its .Net type.
        /// </summary>
        object ConvertXmlStringToObject(string data);

        /// <summary>
        ///     Ensures that constraints for this field are satisfied.
        /// </summary>
        /// <returns>Null if the value is OK, otherwise an error message.</returns>
        string ValidateFieldValue(object value);
    }

	
}
