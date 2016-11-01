// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Convert a <see cref="Report"/> to a <see cref="StructuredQuery"/>.
    /// </summary>
    public interface IReportToQueryConverter
    {
        /// <summary>
        /// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
        /// </summary>
        /// <param name="report">The <see cref="Report" /> to convert. This cannot be null.</param>
        /// <returns>
        /// The converted report.
        /// </returns>
        StructuredQuery Convert( Report report );

        /// <summary>
        /// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
        /// </summary>
        /// <param name="report">The <see cref="Report" /> to convert. This cannot be null.</param>
        /// <param name="settings">Any settings to be applied during the conversion process.</param>
        /// <returns>
        /// The converted report.
        /// </returns>
        StructuredQuery Convert( Report report, ReportToQueryConverterSettings settings );
    }
}
