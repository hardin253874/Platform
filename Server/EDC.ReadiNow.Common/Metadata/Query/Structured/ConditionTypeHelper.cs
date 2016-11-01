// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Provides helper methods for interacting with comparison operators.
    /// </summary>
    public static class ConditionTypeHelper
    {
        /// <summary>
        /// Returns an array of operators that can apply to a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ConditionType[] GetApplicableOperators(DatabaseType type)
        {
            return GetApplicableOperators(type, ConditionType.Unspecified);
        }

        /// <summary>
        /// Returns an array of operators that can apply to a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="extraCondition">An optional extra condition that can be forced into the result.</param>
        /// <returns></returns>
        public static ConditionType[] GetApplicableOperators(DatabaseType type, ConditionType extraCondition)
        {
            if (extraCondition == ConditionType.FullTextSearch)
                return new[] { ConditionType.Unspecified, ConditionType.FullTextSearch };

            var list = GetApplicableConditionTypes(type).ToList();
				
            if (extraCondition != ConditionType.Unspecified && !list.Contains(extraCondition))
                list.Add(extraCondition);

            return list.ToArray();
        }

        /// <summary>
        /// Returns ordered list of condition types for the specified data type.
        /// </summary>
        public static IEnumerable<ConditionType> GetApplicableConditionTypes(DatabaseType dataType)
        {
            var relationshipConditionTypes = new[]
            {
                ConditionType.AnyOf,
                ConditionType.AnyExcept
            };

            var equatableConditionTypes = new[]
            {

                ConditionType.Equal,
                ConditionType.NotEqual
            };

            var comparableConditionTypes = new[]
            {
                ConditionType.GreaterThan,
                ConditionType.GreaterThanOrEqual,
                ConditionType.LessThan,
                ConditionType.LessThanOrEqual
            };

            var searchableConditionTypes = new[]
            {
                ConditionType.Contains,
                ConditionType.StartsWith,
                ConditionType.EndsWith
            };

            var dateConditionTypes = new[]
            {
                ConditionType.Today,
                ConditionType.ThisWeek,
                ConditionType.ThisMonth,
                ConditionType.ThisQuarter,
                ConditionType.ThisYear,
                ConditionType.CurrentFinancialYear,
                ConditionType.LastNDays,
                ConditionType.LastNDaysTillNow,
                ConditionType.NextNDays,
                ConditionType.NextNDaysFromNow,
                ConditionType.LastNWeeks,
                ConditionType.NextNWeeks,
                ConditionType.LastNMonths,
                ConditionType.NextNMonths,
                ConditionType.LastNQuarters,
                ConditionType.NextNQuarters,
                ConditionType.LastNYears,
                ConditionType.NextNYears,
                ConditionType.LastNFinancialYears,
                ConditionType.NextNFinancialYears
            };

            if (dataType is BoolType)
                return new[] {ConditionType.IsTrue, ConditionType.IsFalse};

            if (dataType is StructureLevelsType)
                return new[]
                {
                    ConditionType.AnyAboveStructureLevel, 
                    ConditionType.AnyAtOrAboveStructureLevel, 
                    ConditionType.AnyBelowStructureLevel,
                    ConditionType.AnyAtOrBelowStructureLevel
                };

            if (dataType is ChoiceRelationshipType)
                return relationshipConditionTypes;

            if (dataType is InlineRelationshipType)
                return relationshipConditionTypes.Union(equatableConditionTypes).Union(searchableConditionTypes);

            if (dataType is GuidType)
                return equatableConditionTypes;

            if (dataType is DateType || dataType is DateTimeType)
                return equatableConditionTypes.Union(comparableConditionTypes).Union(dateConditionTypes);

            if (dataType is TimeType || dataType is Int32Type || dataType is IdentifierType || dataType is CurrencyType || dataType is DecimalType)
                return equatableConditionTypes.Union(comparableConditionTypes);

            if (dataType is StringType)
                return equatableConditionTypes.Union(searchableConditionTypes);

            throw new ArgumentException("type");
        }

        /// <summary>
        /// Gets the type of the argument.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <returns>DatabaseType.</returns>
        public static DatabaseType GetArgumentType(ConditionType comparisonType, DatabaseType fieldType)
        {
            switch (comparisonType)
            {
                case ConditionType.Equal:
                case ConditionType.NotEqual:
                case ConditionType.Contains:
                case ConditionType.StartsWith:
                case ConditionType.EndsWith:
                    if (fieldType is InlineRelationshipType)
                        return new StringType();
                    return fieldType;

                case ConditionType.LastNDays:
                case ConditionType.LastNDaysTillNow:
                case ConditionType.NextNDays:
                case ConditionType.NextNDaysFromNow:
                case ConditionType.LastNWeeks:
                case ConditionType.NextNWeeks:
                case ConditionType.LastNMonths:
                case ConditionType.NextNMonths:
                case ConditionType.LastNQuarters:
                case ConditionType.NextNQuarters:
                case ConditionType.LastNYears:
                case ConditionType.NextNYears:
                case ConditionType.LastNFinancialYears:
                case ConditionType.NextNFinancialYears:
                    return new Int32Type();// DatabaseType.Int32;

                case ConditionType.DateEquals:
                    return new DateType();  // operation on date time that only accepts date

                case ConditionType.FullTextSearch:
                    return new StringType();

                case ConditionType.AnyOf:
                case ConditionType.AnyExcept:
                case ConditionType.CurrentUser:
                    if (fieldType is ChoiceRelationshipType)
                        return new ChoiceRelationshipType();

                    if (fieldType is InlineRelationshipType || fieldType is StringType)
                        return new InlineRelationshipType();

                    return new UnknownType();

                case ConditionType.Unspecified:
                    return new UnknownType();

                default:
                    return fieldType;
            }
        }
        
        /// <summary>
        /// Determines if a given operator is a Structure Level Operator
        /// </summary>
        /// <param name="oper"></param>
        /// <returns></returns>
        public static bool IsStructureLevelOperator(ConditionType oper)
        {
            return oper == ConditionType.AnyAtOrAboveStructureLevel || oper == ConditionType.AnyAboveStructureLevel || oper == ConditionType.AnyAtOrBelowStructureLevel || oper == ConditionType.AnyBelowStructureLevel;
        }

        /// <summary>
        /// Determines the type of argument that is expected by a particular operator.
        /// </summary>
        /// <param name="comparisonType">The type of comparison being performed.</param>
        /// <param name="fieldType">The field type to which the comparison is being applied.</param>
        /// <param name="argumentNumber">The argument number.</param>
        /// <returns>The expected type of the first argument.</returns>
        /// <remarks>
        /// Ordinarily, a comparison expects that the data type would be the same as the field type.
        /// For example, if an equality test is being performed on a string, then a string argument is expected.
        /// There are some exceptions to this, for example an 'in the last n days' operation on a DateTime type expects an integer argument.
        /// </remarks>
        public static DatabaseType GetArgumentType(ConditionType comparisonType, DatabaseType fieldType, int argumentNumber)
        {
            if (argumentNumber > 0)
                throw new ArgumentException(@"Only 1 argument is currently supported.", "argumentNumber");

            // Return Unknown if an invalid argument number is requested
            return argumentNumber >= GetArgumentCount(comparisonType) ? new UnknownType() : GetArgumentType(comparisonType, fieldType);
        }

        /// <summary>
        /// Returns the number of arguments that a particular type of condition operator requires.
        /// </summary>
        /// <param name="conditionType">Type of the condition.</param>
        /// <returns></returns>
        public static int GetArgumentCount(ConditionType conditionType)
        {
            switch (conditionType)
            {
                case ConditionType.Unspecified:
                case ConditionType.Today:
                case ConditionType.ThisWeek:
                case ConditionType.ThisMonth:
                case ConditionType.ThisQuarter:
                case ConditionType.ThisYear:
                case ConditionType.CurrentFinancialYear:
                case ConditionType.IsFalse:
                case ConditionType.IsTrue:
                case ConditionType.IsNull:
                case ConditionType.IsNotNull:
                case ConditionType.CurrentUser:
                    return 0;

                default:
                    return 1;
            }
        }

        /// <summary>
        /// Determines whether the specified database type is numeric.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns>
        ///   <c>true</c> if the specified database type is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(DatabaseType databaseType)
        {
            return databaseType is DecimalType || databaseType is Int32Type || databaseType is CurrencyType;
        }
    }
}
