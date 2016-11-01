// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Public interface for a service that provides calculations.
    /// </summary>
    /// <remarks>
    /// HELPFUL NOTES:
    /// 1. Callers should not cache results .. rely on caching by implementations of the providers.
    /// 2. Useful extension methods in this namespace.
    /// </remarks>
    public interface ICalculatedFieldProvider
    {
        /// <summary>
        /// Fetch results for calculated fields.
        /// </summary>
        /// <remarks>
        /// Processes all specified fields for all specified entities.
        /// </remarks>
        /// <param name="fieldIds">The calculated fields to evaluate.</param>
        /// <param name="entityIds">The root entities for which the calculation is being evaluated.</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns>Calculation results.</returns>
        IReadOnlyCollection<CalculatedFieldResult> GetCalculatedFieldValues(IReadOnlyCollection<long> fieldIds, IReadOnlyCollection<long> entityIds, CalculatedFieldSettings settings);
    }

}
