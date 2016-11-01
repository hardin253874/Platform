// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.FieldTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Static helpers for ExprType.
    /// </summary>
    public static class ExprTypeHelper
    {
        /// <summary>
        /// Create a databasetype object that represents an expression type. (or as close as we can).
        /// </summary>
        /// <param name="exprType">The expression type.</param>
        public static DatabaseType ToDatabaseType(ExprType exprType)
        {
            if (exprType == null)
                throw new ArgumentNullException("exprType");

            if (exprType.Type != DataType.Entity)
                return DataTypeHelper.ToDatabaseType(exprType.Type);

            if (exprType.EntityType == null)
                return DatabaseType.InlineRelationshipType;

            var e = Entity.Get<EntityType>(exprType.EntityType.Id);
            if (e.Is<EnumType>())
            {
                return DatabaseType.ChoiceRelationshipType;
            }
            return DatabaseType.InlineRelationshipType;
        }

        /// <summary>
        /// Create an ExprType that represents an entity of a specific type.
        /// </summary>
        /// <param name="entityType">The type of entity being represented.</param>
        /// <returns>An expression type object.</returns>
        public static ExprType EntityOfType(EntityRef entityType)
        {
            return new ExprType
            {
                EntityType = entityType,
                Type = DataType.Entity
            };
        }

        /// <summary>
        /// Create an ExprType that represents an entity of a specific type.
        /// </summary>
        /// <param name="entityType">The type of entity being represented.</param>
        /// <returns>An expression type object.</returns>
        public static ExprType EntityListOfType(EntityRef entityType)
        {
            var result = ExprType.EntityList;
            result.EntityType = entityType;
            return result;
        }

        /// <summary>
        /// Create an ExprType that represents a list of some specific type.
        /// </summary>
        /// <param name="dataType">The type of data being represented.</param>
        /// <returns>An expression type object.</returns>
        public static ExprType ListOf(DataType dataType)
        {
            return new ExprType
            {
                Type = dataType,
                IsList = true,
            };
        }

        /// <summary>
        /// Get an ExprType that's appropriate for a given field.
        /// </summary>
        /// <param name="fieldEntity"></param>
        /// <returns></returns>
        public static ExprType FromFieldEntity(IEntity fieldEntity)
        {
            DataType dataType = FieldHelper.ConvertToDataType(fieldEntity);
            ExprType exprType = new ExprType(dataType);
            DecimalField decimalField = fieldEntity.As<DecimalField>();
            if (decimalField != null)
            {
                exprType.DecimalPlaces = decimalField.DecimalPlaces;
            }
            return exprType;
        }
    }
}
