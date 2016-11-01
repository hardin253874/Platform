// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using ReadiNow.Expressions.Compiler;

namespace ReadiNow.Expressions.CalculatedFields
{
    /// <summary>
    /// Service for providing results on calculated fields.
    /// </summary>
    /// <remarks>
    /// This is essentially the wrapping service that satisfies the public interface and calls the various internal services.
    /// </remarks>
    internal class CalculatedFieldMetadataProvider : ICalculatedFieldMetadataProvider
    {
        IExpressionCompiler _expressionCompiler;
        IEntityRepository _entityRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expressionCompiler">Service for compiling the expressions.</param>
        /// <param name="entityRepository">Source of entity data, both for the calculated field metadata, and also passed into the evaluator for actual data.</param>
        public CalculatedFieldMetadataProvider(IExpressionCompiler expressionCompiler, IEntityRepository entityRepository)
        {
            if (expressionCompiler == null)
                throw new ArgumentNullException("expressionCompiler");
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");

            _expressionCompiler = expressionCompiler;
            _entityRepository = entityRepository;
        }


        /// <summary>
        /// Determine if a field is a calculated field.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public bool IsCalculatedField(long fieldId)
        {
            try
            {
                // Hard-code check for FieldCalculation itself, to prevent circular logic of the field loading itself to load itself.
                if (fieldId == WellKnownAliases.CurrentTenant.FieldCalculation)
                    return false;
            }
            catch (ArgumentException)
            {
                // Thrown if the fieldCalculation field isn't in the database .. i.e. upgrading from a version prior to calculated fields.
                return false;
            }

            IEntity field = _entityRepository.Get(fieldId);
            if (field == null)
                return false;

            // Note: the isCalculatedField bool field is primarily intended to support the client, and is generally unused on the server
            // (This allows the client to determine if a field is calculated, without disclosing the calculation)

            string calc = field.GetField<string>(WellKnownAliases.CurrentTenant.FieldCalculation);

            return !string.IsNullOrEmpty(calc);
        }


        /// <summary>
        /// Creates the evaluator for a group of calculated fields.
        /// </summary>
        /// <param name="calculatedFieldIds">ID of the calculated field.</param>
        /// <param name="settings">Settings.</param>
        /// <returns></returns>
        public IReadOnlyCollection<CalculatedFieldMetadata> GetCalculatedFieldMetadata(IReadOnlyCollection<long> calculatedFieldIds, CalculatedFieldSettings settings)
        {
            if (calculatedFieldIds == null)
                throw new ArgumentNullException("calculatedFieldIds");

            IReadOnlyCollection<Field> calculatedFields;
            List<CalculatedFieldMetadata> result;

            // Load field schema info
            string preloadQuery = "fieldCalculation, fieldIsOnType.id, isOfType.alias";
            calculatedFields = _entityRepository.Get<Field>(calculatedFieldIds, preloadQuery);

            result = new List<CalculatedFieldMetadata>();

            foreach (long fieldId in calculatedFieldIds)
            {
                Field field;
                CalculatedFieldMetadata metadata;

                // Get field
                field = calculatedFields.FirstOrDefault(f => f.Id == fieldId);
                if (field == null)
                {
                    throw new ArgumentException(string.Format("Specified field ID {0} is not a field.", fieldId), "calculatedFieldIds");
                }

                // Get calculation
                metadata = GetSingleCalculatedFieldMetadata(field, settings);
                result.Add(metadata);
            }

            return result;
        }


        /// <summary>
        /// Creates the evaluator for a single calculated field.
        /// </summary>
        /// <param name="calculatedField">Entity for the calculated field.</param>
        /// <param name="settings">Settings.</param>
        /// <returns></returns>
        private CalculatedFieldMetadata GetSingleCalculatedFieldMetadata(Field calculatedField, CalculatedFieldSettings settings)
        {
            string calculation = null;
            IExpression expression = null;
            ParseException exception = null;
            BuilderSettings builderSettings;

            try
            {
                // Get calculation
                calculation = calculatedField.FieldCalculation;
                if (string.IsNullOrEmpty(calculation))
                {
                    throw new ArgumentException("The field has no calculation script. It may not be a calculated field.");
                }

                // Get settings
                builderSettings = CreateBuilderSettingsForField(calculatedField, settings);

                // Compile
                expression = _expressionCompiler.Compile(calculation, builderSettings);

                // Register cache invalidations
                if (CacheContext.IsSet())
                {
                    CalculationDependencies dependencies = _expressionCompiler.GetCalculationDependencies(expression);

                    using (CacheContext cacheContext = CacheContext.GetContext())
                    {
                        cacheContext.Entities.Add(calculatedField.Id);
                        cacheContext.Entities.Add(dependencies.IdentifiedEntities);
                        cacheContext.Entities.Add(builderSettings.RootContextType.EntityTypeId);
                    }
                }
            }
            catch (InvalidMemberParseException ex)
            {
                exception = ex;

                // If a parse-exception resulted from being unable to look up a member name, then it may be corrected by renaming some arbitrary field or relationship
                // that could not be otherwise detected by dependencies.IdentifiedEntities. So invalidate parse exceptions if any field/relationship changes.
                if (CacheContext.IsSet())
                {
                    using (CacheContext cacheContext = CacheContext.GetContext())
                    {
                        cacheContext.Entities.Add(ex.TypeId);
                        // TODO: ideally just listen for all fields/relationships attached to type
                        var fieldTypes = PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(new EntityRef("core:field").Id);
                        cacheContext.EntityTypes.Add(fieldTypes);
                        cacheContext.EntityTypes.Add(new EntityRef("core:relationship").Id);
                    }
                }
            }
            catch (ParseException ex)
            {
                exception = ex;
            }

            // Build metadata
            CalculatedFieldMetadata metadata = new CalculatedFieldMetadata(calculatedField.Id, calculation, expression, exception);

            return metadata;
        }


        /// <summary>
        /// Create compilation settings that are appropriate for a given field.
        /// </summary>
        /// <param name="calculatedField">The calculated field.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        internal static BuilderSettings CreateBuilderSettingsForField(Field calculatedField, CalculatedFieldSettings settings)
        {
            // Determine calculation context type
            EntityType type = calculatedField.FieldIsOnType;
            ExprType contextType = ExprTypeHelper.EntityOfType(new EntityRef(type.Id));

            // Determine result type
            ExprType expectedResult = ExprTypeHelper.FromFieldEntity(calculatedField);
            expectedResult.DisallowList = true;

            // Create settings
            BuilderSettings builderSettings = new BuilderSettings();
            builderSettings.ScriptHost = ScriptHostType.Any;
            builderSettings.RootContextType = contextType;
            builderSettings.ExpectedResultType = expectedResult;

            return builderSettings;
        }

    }
}

