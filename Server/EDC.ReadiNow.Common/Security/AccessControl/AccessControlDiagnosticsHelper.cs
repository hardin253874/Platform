// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Helper methods for access rule diagnostics.
    /// </summary>
    public static class AccessControlDiagnosticsHelper
    {
        /// <summary>
        /// Write out the diagnostic messages when a structured query execution fails.
        /// </summary>
        /// <param name="structuredQuery">
        /// The <see cref="StructuredQuery"/> that failed. This cannot be null.
        /// </param>
        /// <param name="messageContext">
        /// (Optional) The <see cref="MessageContext"/> to write the details to.
        /// </param>
        /// <param name="ex">
        /// The exception thrown when the query ran, or null if no exception was thrown.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="structuredQuery"/> cannot be null.
        /// </exception>
        public static void WriteInvalidSecurityReportMessage(StructuredQuery structuredQuery, MessageContext messageContext = null, Exception ex = null)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }

            EventLog.Application.WriteWarning(
                "{0} ignored due to errors when running the report{1}",
                GetAccessRuleName(structuredQuery),
                ex == null ? string.Empty : ": " + ex.ToString());

            if (messageContext != null)
            {
                messageContext.Append(
                    () => string.Format(
                        "{0} ignored due to errors when running the report",
                        GetAccessRuleName(structuredQuery)));
            }
        }

        /// <summary>
        /// Construct a human readable name for an access rule based on its query.
        /// </summary>
        /// <param name="structuredQuery">
        /// The <see cref="StructuredQuery"/> to generate the name for. This cannot be null.
        /// </param>
        /// <returns>
        /// The name.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="structuredQuery"/> cannot be null.
        /// </exception>
        public static string GetAccessRuleName(StructuredQuery structuredQuery)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }

            if (structuredQuery.Report != null)
            {
                // Re-fetch report, because the one loaded for access rules will likely be in a constrained graph.
                // Note: this will slow things down a bit, but it's in an optional diagnostics trace.
                Report report = Factory.EntityRepository.Get<Report>(structuredQuery.Report);

                return string.Format(
                    "{0}ccess rule on type '{1}' with report '{2}'",
                    (report.ReportForAccessRule.AccessRuleHidden ?? false)
                        ? "Hidden a"
                        : "A",
                    report.ReportForAccessRule.ControlAccess.Name,
                    report.Name);
            }
            return "System rule";
        }
    }
}
