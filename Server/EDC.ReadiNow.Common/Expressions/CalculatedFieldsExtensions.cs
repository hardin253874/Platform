// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Extension methods for calculated fields.
    /// </summary>
    /// <remarks>
    /// This is intended as a helper. Do not put any application logic in here.
    /// </remarks>
    public static class CalculatedFieldsExtensions
    {
        /// <summary>
        /// Fetch metadata for a single calculated field.
        /// </summary>
        /// <param name="provider">The calculated field metadata provider being extended.</param>
        /// <param name="fieldId">The field to load.</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns>Calculation results.</returns>
        public static CalculatedFieldMetadata GetCalculatedFieldMetadata(this ICalculatedFieldMetadataProvider provider, long fieldId, CalculatedFieldSettings settings)
        {
            if (provider == null)
                throw new ArgumentNullException("provider"); // assert false

            var fields = new[] { fieldId };

            // Call provider
            var metadatas = provider.GetCalculatedFieldMetadata(fields, settings);

            // Extract results
            CalculatedFieldMetadata metadata = metadatas.FirstOrDefault();
            return metadata;
        }

        /// <summary>
        /// Fetch results for calculated fields.
        /// </summary>
        /// <remarks>
        /// Extension method for processing a single field.
        /// Script errors cause null result.
        /// </remarks>
        /// <param name="provider">The calculated field provider.</param>
        /// <param name="fieldId">The calculated field to evaluate.</param>
        /// <param name="entityIds">The root entities for which the calculation is being evaluated.</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns>Calculation results.</returns>
        public static CalculatedFieldResult GetCalculatedFieldValues(this ICalculatedFieldProvider provider, long fieldId, IReadOnlyCollection<long> entityIds, CalculatedFieldSettings settings)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");                // assert false

            var fields = new[] { fieldId };

            // Call provider
            var results = provider.GetCalculatedFieldValues(fields, entityIds, settings);

            // Extract results
            CalculatedFieldResult result = results.FirstOrDefault();
            if (result == null)
                throw new Exception("Unexpected null result.");             // assert false .. provider isn't following the rules

            return result;
        }

        /// <summary>
        /// Fetch results for calculated fields.
        /// </summary>
        /// <remarks>
        /// Extension method for processing a single entity and single field.
        /// Script errors cause null result.
        /// </remarks>
        /// <param name="provider">The calculated field provider.</param>
        /// <param name="fieldId">The calculated field to evaluate.</param>
        /// <param name="entityId">The root entity for which the calculation is being evaluated.</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns>Calculation results.</returns>
        public static object GetCalculatedFieldValue(this ICalculatedFieldProvider provider, long fieldId, long entityId, CalculatedFieldSettings settings)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");                // assert false

            var entities = new[] { entityId };

            // Call provider
            var results = provider.GetCalculatedFieldValues(fieldId, entities, settings);

            // Extract results
            if (results == null)
                throw new Exception("Unexpected null result.");             // assert false .. provider isn't following the rules

            // Parse exception means there are no individual results
            if (results.ParseException != null)
                return null;

            CalculatedFieldSingleResult singleResult = results.Entities.FirstOrDefault();
            if (singleResult == null)
                throw new Exception("Could not load calculated field for entity. Likely cause that entity could not be loaded.");

            // Return result
            return singleResult.Result;
        }

    }
}
