// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using System.Linq;
using EDC.ReadiNow.Model.FieldTypes;

namespace EDC.ReadiNow.Model
{
    public static class FieldExtensionMethods
    {
        /// <summary>
        /// Gets the field type for a field.
        /// </summary>
        /// <param name="field">A field, such as the name field Entity.</param>
        /// <returns>A field type such as StringField.</returns>
        public static FieldType GetFieldType(this IEntity field)
        {
            return field.EntityTypes.First().As<FieldType>();
        }

        /// <summary>
        /// Converts a field helper to a (legacy) database type object.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>DatabaseType</returns>
        public static DatabaseType ConvertToDatabaseType(this Field field)
        {
            IFieldHelper fieldHelper = FieldHelper.ConvertToFieldHelper(field);
            return fieldHelper.ConvertToDatabaseType();
        }

        /// <summary>
        ///     Ensures that constraints for this field are satisfied.
        /// </summary>
        /// <returns>Null if the value is OK, otherwise an error message.</returns>
        public static string ValidateFieldValue(this Field field, object value)
        {
            IFieldHelper fieldHelper = FieldHelper.ConvertToFieldHelper(field);
            return fieldHelper.ValidateFieldValue(value);
        }

    }
}
