// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // Notes: this class is similar to but not identical to the ComparisonOperator.
    // The primary difference between ConditionType and ComparisonOperator is that the former is used
    // for strongly typed structured queries, whereas the latter is used for XML based queries.
    // While it was desirable to reuse the same enum the problem was that the XML code is shared across both projects
    // without (originally) being exposed directly through WCF contracts. This meant that if they were to be exposed
    // directly though contracts as well then there would be two incompatible instances on the client side that could
    // not be reconsiled without having to repeatedly adjust the generated proxy code.
    // The future plan is to reduce the reliance on code sharing, and increase the reliance on WCF contracts.

    /// <summary>
    /// Defines the operators available comparison expressions.
    /// </summary>
    // type: operatorEnum
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum ConditionType
    {
        /// <summary>
        /// No comparison is defined.
        /// </summary>
        [EnumMember]
        Unspecified,

        /// <summary>
        /// Equal operator.
        /// </summary>
        [EnumMember]
        Equal,

        /// <summary>
        /// Not equal operator.
        /// </summary>
        [EnumMember]
        NotEqual,

        /// <summary>
        /// Greater than operator
        /// </summary>
        [EnumMember]
        GreaterThan,

        /// <summary>
        /// Greater than or equal operator.
        /// </summary>
        [EnumMember]
        GreaterThanOrEqual,

        /// <summary>
        /// Less than operator.
        /// </summary>
        [EnumMember]
        LessThan,

        /// <summary>
        /// Less than or equal operator.
        /// </summary>
        [EnumMember]
        LessThanOrEqual,

        /// <summary>
        /// Contains operator.
        /// </summary>
        [EnumMember]
        Contains,

        /// <summary>
        /// Starts-with operator.
        /// </summary>
        [EnumMember]
        StartsWith,

        /// <summary>
        /// Ends-with operator.
        /// </summary>
        [EnumMember]
        EndsWith,

        /// <summary>
        /// A boolean must be true.
        /// </summary>
        [EnumMember]
        IsTrue,

        /// <summary>
        /// A boolean must be false.
        /// </summary>
        [EnumMember]
        IsFalse,

        /// <summary>
        /// A value must not be null or empty string.
        /// </summary>
        [EnumMember]
        IsNotNull,

        /// <summary>
        /// A value must be null or empty string.
        /// </summary>
        [EnumMember]
        IsNull,

        /// <summary>
        /// A resource exists above a given structure level.
        /// </summary>
        [EnumMember]
        AnyAboveStructureLevel,

        /// <summary>
        /// A resource exists at or above a given structure level.
        /// </summary>
        [EnumMember]
        AnyAtOrAboveStructureLevel,

        /// <summary>
        /// A resource exists below a given structure level.
        /// </summary>
        [EnumMember]
        AnyBelowStructureLevel,

        /// <summary>
        /// A resource exists at or below a given structure level.
        /// </summary>
        [EnumMember]
        AnyAtOrBelowStructureLevel, 

        /// <summary>
        /// Specifies a full text search query.
        /// </summary>
        [EnumMember]
        FullTextSearch,

        /// <summary>
        /// A resource exists at or under a given relationship control.
        /// </summary>
        [EnumMember]
        AnyOf,

        /// <summary>
        /// A resource NOT exists at or under a given relationship control.
        /// </summary>
        [EnumMember]
        AnyExcept,

        /// <summary>
        /// A user account resource is current user.
        /// </summary>
        [EnumMember]
        CurrentUser,

        /// <summary>
        /// The date component of a date-time equals some value.
        /// </summary>
        [EnumMember]
        DateEquals,

        /// <summary>
        /// Date or DateTime is the current day.
        /// </summary>
        [EnumMember]
        Today,

        /// <summary>
        /// Date or DateTime is the current year.
        /// </summary>
        [EnumMember]
        ThisMonth,

        /// <summary>
        /// Date or DateTime is in the current quarter.
        /// </summary>
        [EnumMember]
        ThisQuarter,

        /// <summary>
        /// Date or DateTime is the current year and month.
        /// </summary>
        [EnumMember]
        ThisYear,

        /// <summary>
        /// Date or DateTime is the current Financial year and month.
        /// </summary>
        [EnumMember]
        CurrentFinancialYear,

        /// <summary>
        /// A date must be within the last N days, and not in the future.
        /// </summary>
        [EnumMember]
        LastNDays,

        /// <summary>
        /// A date must be within the next N days, and not in the past.
        /// </summary>
        [EnumMember]
        NextNDays,

        /// <summary>
        /// A Date or DateTime must be within the last N months, and not in the future.
        /// </summary>
        [EnumMember]
        LastNMonths,

        /// <summary>
        /// A Date or DateTime must be within the next N months, and not in the past.
        /// </summary>
        [EnumMember]
        NextNMonths,

        /// <summary>
        /// A Date or DateTime must be within the last N quarters, and not in the future. (quarter -- Jan-Mar, Apr-Jun, Jul-Sep, Oct-Dec)
        /// </summary>
        [EnumMember]
        LastNQuarters,
        
        /// <summary>
        /// A Date or DateTime must be within the next N quarters, and not in the past.
        /// </summary>
        [EnumMember]
        NextNQuarters,

        /// <summary>
        /// A Date or DateTime must be within the last N years, and not in the future.
        /// </summary>
        [EnumMember]
        LastNYears,

        /// <summary>
        /// A Date or DateTime must be within the next N years, and not in the past.
        /// </summary>
        [EnumMember]
        NextNYears,

        /// <summary>
        /// A Date  or DateTime must be within the last N Financial years, and not in the future.
        /// </summary>
        [EnumMember]
        LastNFinancialYears,

        /// <summary>
        /// A Date or DateTime must be within the next N Financial years, and not in the past.
        /// </summary>
        [EnumMember]
        NextNFinancialYears,

        /// <summary>
        /// A date must be within the last N days, and not in the future.
        /// </summary>
        [EnumMember]
        LastNDaysTillNow,

        /// <summary>
        /// A date must be within the next N days, and not in the past.
        /// </summary>
        [EnumMember]
        NextNDaysFromNow,

        /// <summary>
        /// Date or DateTime is in the current week. Week assumed to commence on Monday.
        /// </summary>
        [EnumMember]
        ThisWeek,

        /// <summary>
        /// A date must be within the last N weeks, and not in the future.
        /// </summary>
        [EnumMember]
        LastNWeeks,

        /// <summary>
        /// A date must be within the next N weeks, and not in the past.
        /// </summary>
        [EnumMember]
        NextNWeeks
    }
}
