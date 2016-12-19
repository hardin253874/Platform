// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database.Types;

namespace EDC.Database
{
    /// <summary>
    /// Various helpers for data type.
    /// </summary>
    public static class DataTypeHelper
    {
        /// <summary>
        /// Converts a DataType to a DatabaseType.
        /// </summary>
        public static DatabaseType ToDatabaseType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.None:
                    return DatabaseType.UnknownType;
                case DataType.Entity:
                    return DatabaseType.IdentifierType; // This is almost certainly wrong.
                case DataType.String:
                    return DatabaseType.StringType;
                case DataType.Int32:
                    return DatabaseType.Int32Type;
                case DataType.Decimal:
                    return DatabaseType.DecimalType;
                case DataType.Currency:
                    return DatabaseType.CurrencyType;
                case DataType.Date:
                    return DatabaseType.DateType;
                case DataType.Time:
                    return DatabaseType.TimeType;
                case DataType.DateTime:
                    return DatabaseType.DateTimeType;
                case DataType.Bool:
                    return DatabaseType.BoolType;
                case DataType.Guid:
                    return DatabaseType.GuidType;
                case DataType.Binary:
                    return DatabaseType.BinaryType;
                case DataType.Xml:
                    return DatabaseType.XmlType;
                default:
                    throw new ArgumentException("dataType", "Unknown data type: " + dataType);
            }
        }

        /// <summary>
        /// Converts a DatabaseType to a DataType.
        /// </summary>
        public static DataType FromDatabaseType(DatabaseType databaseType)
        {
            if (databaseType is UnknownType)
                return DataType.None;
            if (databaseType is IdentifierType || databaseType is ChoiceRelationshipType || databaseType is InlineRelationshipType)
                return DataType.None;
            if (databaseType is StringType)
                return DataType.String;
            if (databaseType is Int32Type)
                return DataType.Int32;
            if (databaseType is CurrencyType)
                return DataType.Currency;
            if (databaseType is DecimalType)
                return DataType.Decimal;
            if (databaseType is DateType)
                return DataType.Date;
            if (databaseType is TimeType)
                return DataType.Time;
            if (databaseType is DateTimeType)
                return DataType.DateTime;
            if (databaseType is BoolType)
                return DataType.Bool;
            if (databaseType is GuidType)
                return DataType.Guid;
            if (databaseType is BinaryType)
                return DataType.Binary;
            if (databaseType is XmlType)
                return DataType.Xml;
            throw new ArgumentException("dataType", "Unknown data type: " + databaseType);
        }

        /// <summary>
        /// Converts a .Net type to a suggested data type.
        /// WARNING: .Net types do no hold rich type information. (For example 'Currency' is lost)
        /// If you're calling this function, then you've done something wrong.
        /// </summary>
        public static DataType FromDotNetType(Type type)
        {
            if (type == typeof(string))
                return DataType.String;

            if (type == typeof(decimal) || type == typeof(decimal?))
                return DataType.Decimal;

            if (type == typeof(int) || type == typeof(int?))
                return DataType.Int32;

            if (type == typeof(Guid) || type == typeof(Guid?))
                return DataType.Guid;

            if (type == typeof(bool) || type == typeof(bool?))
                return DataType.Bool;

            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return DataType.DateTime;

            if (type.Name == "EntityRef")
                return DataType.Entity;

            throw new ArgumentException("Unknown data type: " + type.Name, nameof( type ));
            
        }

        /// <summary>
        /// Returns true if this is a decimal type.
        /// </summary>
        public static bool IsDecimal(DataType dataType)
        {
            return dataType == DataType.Decimal || dataType == DataType.Currency;
        }

        /// <summary>
        /// Converts a DataType to a TableValuedParameterType that has a single column, representing the type.
        /// </summary>
        /// <remarks>
        /// Database table type has a single column called 'Data' of the prescribed type.
        /// </remarks>
        public static TableValuedParameterType ToSingleTableValuedParameterType( DataType dataType )
        {
            switch ( dataType )
            {
                case DataType.String:
                case DataType.Xml:
                    return TableValuedParameterType.NVarCharMaxListType;
                case DataType.Int32:
                    return TableValuedParameterType.Int;
                case DataType.Decimal:
                case DataType.Currency:
                    return TableValuedParameterType.Decimal;
                case DataType.Date:
                case DataType.Time:
                case DataType.DateTime:
                    return TableValuedParameterType.DateTime;
                case DataType.Guid:
                    return TableValuedParameterType.GuidList;   // this one calls its column 'Data', c.f. other Guid options use Id.
                case DataType.Bool:     // because a list of booleans is dumb
                case DataType.Binary:
                case DataType.None:
                case DataType.Entity:
                    throw new ArgumentException( "dataType", "Unsupported data type: " + dataType );
                default:
                    throw new ArgumentException( "dataType", "Unknown data type: " + dataType );
            }
        }

    }
}
