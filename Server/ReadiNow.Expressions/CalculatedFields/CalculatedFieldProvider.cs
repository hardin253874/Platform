// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Service for providing results on calculated fields.
    /// </summary>
    /// <remarks>
    /// This is essentially the wrapping service that satisfies the public interface and calls the various internal services.
    /// </remarks>
    internal class CalculatedFieldProvider : ICalculatedFieldProvider
    {
        ICalculatedFieldMetadataProvider _metadataProvider;
        IExpressionRunner _expressionRunner;
        IEntityRepository _entityRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadataProvider">The calculated field metadata provider.</param>
        /// <param name="expressionRunner">Runner that gets used by the field evaluator that will be generated with the metadata.</param>
        /// <param name="entityRepository">Source of entity data, both for the calculated field metadata, and also passed into the evaluator for actual data.</param>
        public CalculatedFieldProvider(ICalculatedFieldMetadataProvider metadataProvider, IExpressionRunner expressionRunner, IEntityRepository entityRepository)
        {
            if (metadataProvider == null)
                throw new ArgumentNullException("metadataProvider");
            if (expressionRunner == null)
                throw new ArgumentNullException("expressionRunner");
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");

            _metadataProvider = metadataProvider;
            _expressionRunner = expressionRunner;
            _entityRepository = entityRepository;
        }

        /// <summary>
        /// Fetch results for calculated fields.
        /// </summary>
        /// <remarks>
        /// Processes all specified fields for all specified entities.
        /// </remarks>
        /// <param name="fieldIDs">The calculated fields to evaluate.</param>
        /// <param name="entityIDs">The root entities for which the calculation is being evaluated.</param>
        /// <param name="settings">Additional settings.</param>
        /// <returns>Calculation results.</returns>
        public IReadOnlyCollection<CalculatedFieldResult> GetCalculatedFieldValues(IReadOnlyCollection<long> fieldIDs, IReadOnlyCollection<long> entityIDs, CalculatedFieldSettings settings)
        {
            if (fieldIDs == null)
                throw new ArgumentNullException("fieldIDs");
            if (entityIDs == null)
                throw new ArgumentNullException("entityIDs");
            if (settings == null)
                throw new ArgumentNullException("settings");

            // Create the evaluators for this field
            IReadOnlyCollection<CalculatedFieldMetadata> fieldsMetadata;
            fieldsMetadata = _metadataProvider.GetCalculatedFieldMetadata(fieldIDs, settings);

            // Create result container
            List<CalculatedFieldResult> results = new List<CalculatedFieldResult>(fieldIDs.Count);

            // Create expression evaluator settings
            EvaluationSettings evalSettings = CreateEvaluationSettings(settings);

            // Fill
            foreach (long fieldId in fieldIDs)
            {
                CalculatedFieldMetadata fieldMetadata;
                IReadOnlyCollection<CalculatedFieldSingleResult> fieldResults;
                CalculatedFieldResult result;

                // Find the evaluator
                // (Deemed cheaper to just scan, rather than use a dictionary)
                fieldMetadata = fieldsMetadata.First(fe => fe.CalculatedFieldId == fieldId);

                if (fieldMetadata.Exception != null)
                {
                    result = new CalculatedFieldResult(fieldId, fieldMetadata.Exception);
                }
                else
                {
                    // And get the values
                    fieldResults = GetSingleFieldValues(fieldMetadata, entityIDs, evalSettings);
                    result = new CalculatedFieldResult(fieldId, fieldResults);
                }

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Create an evaluation-settings object.
        /// </summary>
        /// <remarks>
        /// The settings object will be reused for all entities requested in this iteration.
        /// The context entity will be set in each time.
        /// </remarks>
        internal static EvaluationSettings CreateEvaluationSettings(CalculatedFieldSettings settings)
        {
            EvaluationSettings evalSettings = new EvaluationSettings();
            evalSettings.TimeZoneName = settings.TimeZone;

            if (evalSettings.TimeZoneName == null)
            {
                var requestContext = RequestContext.GetContext();
                if (requestContext != null)
                    evalSettings.TimeZoneName = requestContext.TimeZone;

                if (evalSettings.TimeZoneName == null)
                    throw new InvalidOperationException("TimeZone information must be provided in settings object, or via RequestContext.");
            }

            return evalSettings;
        }

        /// <summary>
        /// Get the calculated field value for the specified entities.
        /// </summary>
        /// <param name="fieldMetadata">The metadata for the calculated field.</param>
        /// <param name="entityIDs">Entities</param>
        /// <param name="evalSettings">Evaluation settings</param>
        /// <returns>List of results.</returns>
        private IReadOnlyCollection<CalculatedFieldSingleResult> GetSingleFieldValues(CalculatedFieldMetadata fieldMetadata, IReadOnlyCollection<long> entityIDs, EvaluationSettings evalSettings)
        {
            if (fieldMetadata == null)
                throw new ArgumentNullException("fieldMetadata");
            if (entityIDs == null)
                throw new ArgumentNullException("entityIDs");

            // Handle statically broken calculated-fields
            // (may be null due to not being a calculated field, or the script failing to compile)
            if (fieldMetadata.Expression == null)
            {
                // If the calculated field was somehow invalid, return null for values.
                return entityIDs.Select(id => new CalculatedFieldSingleResult(id, null)).ToArray();
            }

            // Load entities
            IReadOnlyCollection<IEntity> entities;
            entities = _entityRepository.Get(entityIDs);

            var results = new List<CalculatedFieldSingleResult>(entities.Count);

            // Perform calculations
            foreach (IEntity entity in entities)
            {
                ExpressionRunResult runResult;
                CalculatedFieldSingleResult singleResult;

                evalSettings.ContextEntity = entity;

                try
                {
                    runResult = _expressionRunner.Run(fieldMetadata.Expression, evalSettings);
                    singleResult = new CalculatedFieldSingleResult(entity.Id, runResult.Value);
                }
                catch (EvaluationException ex)
                {
                    singleResult = new CalculatedFieldSingleResult(entity.Id, ex);
                }

                results.Add(singleResult);
            }

            return results;
        }

    }
}

