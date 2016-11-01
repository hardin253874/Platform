// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;

namespace EDC.ReadiNow.Model.FieldTypes
{
    /// <summary>
    ///     Extend the generated CurrencyField class with behaviors.
    /// </summary>
    public class CurrencyFieldHelper : DecimalFieldHelper
    {
        private CurrencyField _field;

        /// <summary>
        ///     Constructor.
        /// </summary>
        internal CurrencyFieldHelper(CurrencyField fieldEntity)
            : base(fieldEntity.As<DecimalField>())
        {
            _field = fieldEntity;
        }

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public override DatabaseType ConvertToDatabaseType()
        {
            return DatabaseType.CurrencyType;
        }

        /// <summary>
        ///     Converts the type of to database.
        /// </summary>
        public override DataType ConvertToDataType()
        {
            return DataType.Currency;
        }
    }
}
