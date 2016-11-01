// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Model.FieldTypes
{
    /// <summary>
	///     Extends the base type for all fields.
	/// </summary>
    public abstract class FieldHelper : IFieldHelper
	{
        private Field _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal FieldHelper(Field fieldEntity)
        {
            _field = fieldEntity;
        }

        /// <summary>
        /// DatabaseTypeCache
        /// </summary>
        private static readonly IDictionary<long, DataType> DataTypeCache = new Dictionary<long, DataType>();

        /// <summary>
        /// DatabaseTypeCache
        /// </summary>
        private static readonly IDictionary<long, DatabaseType> DatabaseTypeCache = new Dictionary<long, DatabaseType>();

        public static IFieldHelper ConvertToFieldHelper(IEntity fieldEntity)
        {
            if (fieldEntity == null)
                throw new ArgumentNullException("fieldEntity");
            
            // Ensure we have the specific type of field in question.
            IEntity field = Entity.AsNative(fieldEntity);
            if (field == null)
                throw new Exception("Assert false");            

            StringField stringField = field as StringField;
            if (stringField != null)
                return new StringFieldHelper(stringField);

            AliasField aliasField = field as AliasField;
            if (aliasField != null)
                return new AliasFieldHelper(aliasField);
                
            AutoNumberField autoNumberField = field as AutoNumberField;
            if (autoNumberField != null)
                return new AutoNumberFieldHelper(autoNumberField);
                
            BoolField boolField = field as BoolField;
            if (boolField != null)
                return new BoolFieldHelper(boolField);
                
            CurrencyField currencyField = field as CurrencyField;
            if (currencyField != null)
                return new CurrencyFieldHelper(currencyField);
                
            DateField dateField = field as DateField;
            if (dateField != null)
                return new DateFieldHelper(dateField);
                
            DateTimeField dateTimeField = field as DateTimeField;
            if (dateTimeField != null)
                return new DateTimeFieldHelper(dateTimeField);
                
            DecimalField decimalField = field as DecimalField;
            if (decimalField != null)
                return new DecimalFieldHelper(decimalField);
                
            IntField intField = field as IntField;
            if (intField != null)
                return new IntFieldHelper(intField);
                
            TimeField timeField = field as TimeField;
            if (timeField != null)
                return new TimeFieldHelper(timeField);
                
            GuidField guidField = field as GuidField;
            if (guidField != null)
                return new GuidFieldHelper(guidField);
                
            XmlField xmlField = field as XmlField;
            if (xmlField != null)
                return new XmlFieldHelper(xmlField);

            throw new Exception("Entity is not a valid field type: " + field.GetType().ToString());
        }

        public static DataType ConvertToDataType(IEntity fieldEntity)
        {
            DataType type;

            long typeId = fieldEntity.TypeIds.First();

            if (!DataTypeCache.TryGetValue(typeId, out type))
            {
                lock (DatabaseTypeCache)
                {
                    if (!DataTypeCache.TryGetValue(typeId, out type))
                    {
                        IFieldHelper fieldHelper = ConvertToFieldHelper(fieldEntity);
                        type = fieldHelper.ConvertToDataType();

                        DataTypeCache[typeId] = type;
                    }
                }
            }

            return type;
        }
		
        /// <summary>
		///     Gets the DatabaseType object for this field.
		/// </summary>
        public abstract DatabaseType ConvertToDatabaseType();

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public abstract DataType ConvertToDataType();

		/// <summary>
		///     Converts a configuration XML field value string to an instance of its .Net type.
		/// </summary>
		public abstract object ConvertXmlStringToObject( string data );

		/// <summary>
		///     Ensures that constraints for this field are satisfied.
		/// </summary>
		/// <returns>
		///     Null if the value is OK, otherwise an error message.
		/// </returns>
        public abstract string ValidateFieldValue(object value);

        /// <summary>
        ///     Returns a function for converting strings to values.
        /// </summary>
        /// <returns></returns>
        public static Func<string, object> GetConverterFunction(Field field)
        {
            // Todo: this method should be converted to avoid relying on DatabaseType

            DatabaseType type = field.ConvertToDatabaseType();
            return type.ConvertFromString;
        }
	}
}

/////
// ReSharper restore CheckNamespace
/////