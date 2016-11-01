// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
	/// <summary>
	///     Create the report used for a new access rule.
	/// </summary>
	public interface IAccessRuleReportFactory
	{
	    /// <summary>
	    ///     Get the default display report for the given <see cref="EntityType" />.
	    ///     If there is no display report, do a breadth-first recursion through
	    ///     the inherited types to find a suitable display report.
	    /// </summary>
	    /// <param name="securableEntity">
	    ///     The type (or other <see cref="SecurableEntity" /> the report will be for.
	    /// </param>
	    /// <param name="structuredQuery">
        ///     An optional <see cref="StructuredQuery" /> to use for the report..
        /// </param>
	    /// <returns>
	    ///     A <see cref="ReadiNow.Model.Report" /> or null, if not report is found.
	    /// </returns>
	    /// <exception cref="ArgumentNullException">
	    ///     <paramref name="securableEntity" /> cannot be null.
	    /// </exception>
	    Report GetDisplayReportForSecurableEntity( SecurableEntity securableEntity, StructuredQuery structuredQuery = null );
	}
}