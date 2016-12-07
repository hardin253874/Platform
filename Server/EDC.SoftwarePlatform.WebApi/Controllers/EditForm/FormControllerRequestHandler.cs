// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.Cache;
using EDC.Common;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;

namespace EDC.SoftwarePlatform.WebApi.Controllers.EditForm
{
    /// <summary>
    /// Form data request handler
    /// </summary>
    public class FormControllerRequestHandler
    {
        private readonly ICache<long, EntityData> _formsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormControllerRequestHandler"/> class.
        /// </summary>
        /// <param name="formsCache">The forms cache.</param>
        public FormControllerRequestHandler(ICache<long, EntityData> formsCache)
        {
            if (formsCache == null)
            {
                throw new ArgumentNullException(nameof(formsCache));
            }

            _formsCache = formsCache;
        }

        /// <summary>
        ///     Compiles the visibility calculations.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="visibilityCalculations">The visibility calculations.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        private IDictionary<long, IExpression> CompileVisibilityCalculations(EntityRef entityType,
            IDictionary<long, string> visibilityCalculations)
        {            
            var compiledCalculations = new Dictionary<long, IExpression>();

            if (visibilityCalculations.Count == 0)
                return compiledCalculations;

            var builderSettings = new BuilderSettings
            {
                RootContextType = ExprTypeHelper.EntityOfType(entityType),
                ScriptHost = ScriptHostType.Any,
                ExpectedResultType = new ExprType(DataType.Bool) // Visibility calculations should always return a bool
            };

            // Compile calculations
            foreach (var kvp in visibilityCalculations)
            {
                try
                {
                    var expression = Factory.ExpressionCompiler.Compile(kvp.Value, builderSettings);
                    if (expression.ResultType.Type == DataType.Bool)
                    {
                        compiledCalculations[kvp.Key] = expression;
                    }                 
                }
                catch (Exception ex)
                {
                    // Be resilient to compile errors.
                    EventLog.Application.WriteError(
                        "An error occured compiling visibility calculation {0} for form control {1}. Error {2}.",
                        kvp.Value, kvp.Key, ex);
                }
            }

            return compiledCalculations;
        }


        /// <summary>
        ///     Gets the control visibility calculate dependencies.
        /// </summary>
        /// <param name="compiledExpressions">The compiled expressions.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private IDictionary<long, VisibilityCalcDependencies> GetControlVisibilityCalcDependencies(
            IDictionary<long, IExpression> compiledExpressions)
        {            
            var controlVisibilityDependencies = new Dictionary<long, VisibilityCalcDependencies>();

            foreach (var kvp in compiledExpressions)
            {
                long controlId = kvp.Key;

                CalculationDependencies calculationDependencies = Factory.ExpressionCompiler.GetCalculationDependencies(kvp.Value);

                if (calculationDependencies == null)
                    continue;

                VisibilityCalcDependencies controlDependencies;

                if (!controlVisibilityDependencies.TryGetValue(controlId, out controlDependencies))
                {
                    controlDependencies = new VisibilityCalcDependencies();                    
                    controlVisibilityDependencies[controlId] = controlDependencies;
                }

                if (calculationDependencies.Fields != null && calculationDependencies.Fields.Count > 0)
                {
                    if (controlDependencies.Fields == null)
                    {
                        controlDependencies.Fields = new HashSet<long>();
                    }
                    controlDependencies.Fields.AddRange(calculationDependencies.Fields);
                }

                if (calculationDependencies.Relationships != null && calculationDependencies.Relationships.Count > 0)
                {
                    if (controlDependencies.Relationships == null)
                    {
                        controlDependencies.Relationships = new HashSet<long>();
                    }

                    controlDependencies.Relationships.AddRange(calculationDependencies.Relationships);
                }                                
            }

            return controlVisibilityDependencies;
        }

        /// <summary>
        ///     Packages the form data response.
        /// </summary>
        /// <param name="entityData">The entity data.</param>
        /// <param name="initiallyHiddenControls">The initially hidden controls.</param>
        /// <returns></returns>
        private FormDataResponse PackageFormDataResponse(EntityData entityData, ISet<long> initiallyHiddenControls)
        {
            var context = new EntityPackage();
            context.AddEntityData(entityData, "entity");

            var response = new FormDataResponse
            {
                FormDataEntity = context.GetQueryResult(),
                InitiallyHiddenControls = initiallyHiddenControls                
            };

            return response;
        }

        /// <summary>
        ///     Gets the control visibility calculations.
        /// </summary>
        /// <param name="formEntityData">The form entity data.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private IDictionary<long, string> GetControlVisibilityCalculations(EntityData formEntityData)
        {            
            ISet<long> controlTypes =
                PerTenantEntityTypeCache.Instance.GetDescendantsAndSelf(WellKnownAliases.CurrentTenant.ControlOnForm);

            var controlVisibilityCalculations = new Dictionary<long, string>();

            // Enumerate form controls and find visibilityCalculations                    
            EntityData.WalkRelationshipTree(formEntityData, node =>
            {
                RelationshipData isOfType = node.GetRelationship(WellKnownAliases.CurrentTenant.IsOfType);
                if (isOfType?.Entities == null)
                    return;

                if (!controlTypes.Overlaps(isOfType.Entities.Select(e => e.Id.Id)))
                    return;

                FieldData visibilityCalculationField = node.GetField(WellKnownAliases.CurrentTenant.VisibilityCalculation);

                var visibilityCalculation = visibilityCalculationField?.Value?.Value as string;

                if (string.IsNullOrWhiteSpace(visibilityCalculation))
                    return;

                controlVisibilityCalculations[node.Id.Id] = visibilityCalculation;
            }, null, null);

            return controlVisibilityCalculations;
        }

        /// <summary>
        ///     Parses the query string.
        /// </summary>
        /// <param name="queryString">The query string.</param>
        /// <returns></returns>
        private bool ParseQueryString(string queryString)
        {
            try
            {
                using (new SecurityBypassContext())
                {
                    Factory.RequestParser.ParseRequestQuery(queryString);
                }
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the form as entity data.
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <param name="isInDesignMode">if set to <c>true</c> [is in design mode].</param>
        /// <returns></returns>        
        public EntityData GetFormAsEntityData(long formId, bool isInDesignMode)
        {
            EntityData formEntityData;

            _formsCache.TryGetOrAdd(formId, out formEntityData, key =>
            {
                var formEntity = EditFormHelper.GetFormAsEntityData(key, isInDesignMode);
                if (formEntity == null)
                    throw new Exception("entityData was null");

                return formEntity;
            });

            return formEntityData;
        }

        /// <summary>
        ///     Validates the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public bool ValidateRequest(FormDataRequest request, out string errorMessage)
        {
            errorMessage = string.Empty;

            var messageBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(request.EntityId))
                messageBuilder.Append("EntityId was null.");
            if (string.IsNullOrWhiteSpace(request.FormId))
                messageBuilder.Append("FormId was null.");
            if (string.IsNullOrWhiteSpace(request.Query))
                messageBuilder.Append("Query was null.");
            if (!string.IsNullOrWhiteSpace(request.Query) &&
                !ParseQueryString(request.Query))
                messageBuilder.Append("Failed to parse query string.");

            if (messageBuilder.Length <= 0) return true;

            messageBuilder.Insert(0, "Cannot parse post data.");
            errorMessage = messageBuilder.ToString();

            return false;
        }

        /// <summary>
        ///     Gets the type to edit with form.
        /// </summary>
        /// <param name="formEntityData">The form entity data.</param>
        /// <returns></returns>
        private long GetTypeToEditWithForm(EntityData formEntityData)
        {
            RelationshipData entityTypeRelData = formEntityData?.GetRelationship(WellKnownAliases.CurrentTenant.TypeToEditWithForm);
            if ((entityTypeRelData?.Entities == null) || !entityTypeRelData.Entities.Any())
                return -1;

            return entityTypeRelData.Entities.Select(e => e.Id.Id).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the hidden controls.
        /// </summary>
        /// <param name="contextEntity">The context entity.</param>
        /// <param name="controlsWithVisibilityCalculations"></param>
        /// <param name="compiledExpressions">The compiled expressions.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        ///     TimeZone information must be provided in settings object, or via
        ///     RequestContext.
        /// </exception>
        private ISet<long> GetHiddenControls(IEntity contextEntity, IEnumerable<long> controlsWithVisibilityCalculations, IDictionary<long, IExpression> compiledExpressions)
        {
            var hiddenControls = new HashSet<long>();

            var evaluationSettings = new EvaluationSettings {ContextEntity = contextEntity};

            var requestContext = RequestContext.GetContext();
            if (requestContext != null)
                evaluationSettings.TimeZoneName = requestContext.TimeZone;

            if (evaluationSettings.TimeZoneName == null)
                throw new InvalidOperationException(
                    "TimeZone information must be provided in settings object, or via RequestContext.");

            // Enumerate through the controls with visibility calcs
            // and mark any without compiled expressions as hidden.
            // A control without a compiled expression indicates that something went wrong during the compile.
            foreach (var controlId in controlsWithVisibilityCalculations)
            {
                if (!compiledExpressions.ContainsKey(controlId))
                {
                    hiddenControls.Add(controlId);
                }
            }

            // Evalute any compiled expressions
            foreach (var kvp in compiledExpressions)
            {
                long controlId = kvp.Key;
                IExpression expression = kvp.Value;

                try
                {
                    // The control is shown if the calculation returns true
                    ExpressionRunResult result = Factory.ExpressionRunner.Run(expression, evaluationSettings);
                    
                    if (result.Value.Equals(false))
                        hiddenControls.Add(controlId);
                }
                catch (Exception ex)
                {
                    // Something went wrong. Hide in case of error
                    hiddenControls.Add(controlId);
                    EventLog.Application.WriteError(
                        "An error occurred trying to evaluate visibility calculation for control {0}. Error {1}.",
                        controlId, ex);
                }
            }            

            return hiddenControls;
        }


        /// <summary>
        ///     Gets the form data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public FormDataResponse GetFormData(FormDataRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var entityIdRef = new EntityRef(request.EntityId);

            // Get the entity data
            EntityData entityData = BulkRequestRunner.GetEntityData(entityIdRef, request.Query, request.Hint);

            // Set the result to NotFound for Basic and BasicWithDemand only
            if (entityData == null)
                return null;

            var formId = new EntityRef(request.FormId).Id;
            EntityData formEntityData = null;

            // Get the form entity data for non gen forms        
            if (!EntityTemporaryIdAllocator.IsAllocatedId(formId))
            {
                try
                {
                    formEntityData = GetFormAsEntityData(formId, false);
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("Failed to get form with id {0}. Unable to get initial form control visibility. Error: {1}.", formId, ex);
                }
            }

            ISet<long> initiallyHiddenControls = null;

            if (formEntityData == null) return PackageFormDataResponse(entityData, null);

            IDictionary<long, IExpression> compiledExpressions = null;
            IDictionary<long, string> controlVisibilityCalculations;

            using (new SecurityBypassContext())
            {
                // Have form. Get any visibility calculations
                controlVisibilityCalculations = GetControlVisibilityCalculations(formEntityData);

                long entityTypeId = GetTypeToEditWithForm(formEntityData);

                if (controlVisibilityCalculations.Count > 0 && entityTypeId > 0)
                {
                    // Now we have all the calculations
                    compiledExpressions = CompileVisibilityCalculations(new EntityRef(entityTypeId), controlVisibilityCalculations);
                }                    
            }

            if (controlVisibilityCalculations.Count > 0)
                initiallyHiddenControls = GetHiddenControls(entityIdRef.Entity, controlVisibilityCalculations.Keys, compiledExpressions);

            return PackageFormDataResponse(entityData, initiallyHiddenControls);
        }

        /// <summary>
        /// Gets the form calculation dependencies.
        /// </summary>
        /// <param name="formRef">The form reference.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public FormVisibilityCalcDependenciesResponse GetFormCalculationDependencies(EntityRef formRef)
        {
            var response = new FormVisibilityCalcDependenciesResponse();

            if (formRef == null)
            {
                throw new ArgumentNullException(nameof(formRef));
            }

            EntityData formEntityData = null;

            // Get the form entity data for non gen forms        
            if (!EntityTemporaryIdAllocator.IsAllocatedId(formRef.Id))
            {
                try
                {
                    formEntityData = GetFormAsEntityData(formRef.Id, false);
                }
                catch (Exception ex)
                {
                    EventLog.Application.WriteError("Failed to get form with id {0}. Unable to get form control visibility calculation dependencies. Error: {1}.", formRef.Id, ex);
                }
            }            

            if (formEntityData == null) return response;

            using (new SecurityBypassContext())
            {
                // Have form. Get any visibility calculations
                IDictionary<long, string> controlVisibilityCalculations = GetControlVisibilityCalculations(formEntityData);

                long entityTypeId = GetTypeToEditWithForm(formEntityData);

                if (controlVisibilityCalculations.Count > 0 && entityTypeId > 0)
                {
                    // Now we have all the calculations
                    IDictionary<long, IExpression> compiledExpressions = CompileVisibilityCalculations(new EntityRef(entityTypeId), controlVisibilityCalculations);
                    IDictionary<long, VisibilityCalcDependencies> dependencies = GetControlVisibilityCalcDependencies(compiledExpressions);
                    if (dependencies.Count > 0)
                    {
                        response.VisibilityCalcDependencies = dependencies;
                    }
                }                                                            
            }

            return response;
        }
    }
}