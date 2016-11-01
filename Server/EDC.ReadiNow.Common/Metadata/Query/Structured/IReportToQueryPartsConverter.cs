// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Service interface for converting parts of a report
    /// </summary>
    public interface IReportToQueryPartsConverter
    {
        /// <summary>
        /// Create a context object that can be passed to conversion methods.
        /// </summary>
        IReportToQueryContext CreateContext( Report report );

        /// <summary>
        /// Check if an expression is valid.
        /// </summary>
        bool IsValidExpression(ReportExpression expression);

        /// <summary>
        /// Convert a single condition.
        /// </summary>
        Condition ConvertCondition( ReportCondition reportCondition, DatabaseType columnType, IReportToQueryContext context );
    }

    /// <summary>
    /// A context object that can be passed between conversion calls.
    /// </summary>
    public interface IReportToQueryContext
    {
    }

}
