// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Public interface for a service that provides compile-time metadata about calculated fields.
    /// </summary>
    public interface ICalculatedFieldMetadataProvider
    {
        /// <summary>
        /// Returns true if the ID number refers to a calculated field. Otherwise, false.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns>Returns true if the ID number refers to a calculated field. Otherwise, false.</returns>
        bool IsCalculatedField(long fieldId);

        /// <summary>
        /// Get the static information about a calculated field, which can be used to more quickly evaluate individual entities.
        /// </summary>
        /// <param name="fieldIDs">The field IDs to load</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns></returns>
        IReadOnlyCollection<CalculatedFieldMetadata> GetCalculatedFieldMetadata(IReadOnlyCollection<long> fieldIDs, CalculatedFieldSettings settings);
    }

}
