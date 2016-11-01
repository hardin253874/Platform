// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Diagnostics;
using System.Collections.Concurrent;
using EDC.Database;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.EntityRequests;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// This class contains information about an activities structure.
    /// </summary>
    public class WorkflowMetadata
    {

        readonly Dictionary<long, ActivityImplementationBase> _cachedInstances = new Dictionary<long, ActivityImplementationBase>();
        readonly HashSet<long> _activitiesWithAddedMetadata = new HashSet<long>();    
   
        
        public List<String> ValidationMessages { get; private set; }

        public IDictionary<long, ActivityImplementationBase> CachedInstances { get { return _cachedInstances; }}
        public ConcurrentDictionary<long, IExpression> CompiledExpressionCache { get; private set; }

        private ConcurrentDictionary<long, ExprType> _expressionArgumentTypeCache = new ConcurrentDictionary<long, ExprType>();
        public ConcurrentDictionary<long, ActivityArgument> _expressionArgumentCache = new ConcurrentDictionary<long, ActivityArgument>();


        private IDictionary<long, IEnumerable<ExitPoint>> _exitPointsCache = new Dictionary<long, IEnumerable<ExitPoint>>();
        private IDictionary<Tuple<long, long>, Transition> _transitionCache = new Dictionary<Tuple<long, long>, Transition>();
        private IDictionary<Tuple<long, long>, WfExpression> _populatedByCache = new Dictionary<Tuple<long, long>, WfExpression>();

        // argument caches
        public Dictionary<string, ExprType> AllOutputTypes { get; private set; }
        public Dictionary<string, WfArgumentInstance> AllOutputInstances { get; private set; }
        public Dictionary<string, ExprType> AllInputAndVariableTypes { get; private set; }
        public Dictionary<string, ActivityArgument> AllInputAndVariableArguments { get; private set; }

        // The activities that have already cached their metadata
        public HashSet<long> ActivitiesWithAddedMetadata { get { return _activitiesWithAddedMetadata; } }

        public bool HaveValidated { get; set; }
        public bool HaveAddedInstances { get; set; }
        public bool HaveAddedMetadata { get; set; }

        public string WorkflowUpdateHash { get; private set; }
    
        //
        // workflow properties
        public bool WfRunAsOwner { get; private set; }
        public UserAccount WfSecurityOwner { get; private set; }

        //
        // Other properteis
        readonly string ownerAccountRuntimeArgName;
        readonly string triggeringUserRuntimeArgName;
        readonly string triggeringPersonRuntimeArgName;

        public WorkflowMetadata()
        {
            ValidationMessages = new List<string>();
            CompiledExpressionCache = new ConcurrentDictionary<long, IExpression>();
        }

        public WorkflowMetadata(Workflow wf): this()
        {
            using (Profiler.Measure("WorkflowMetadata"))
            {
                ownerAccountRuntimeArgName = Entity.GetName(new EntityRef("workflowOwnerAccount").Id);
                triggeringUserRuntimeArgName = Entity.GetName(new EntityRef("triggeringUserAccount").Id);
                triggeringPersonRuntimeArgName = Entity.GetName(new EntityRef("triggeringPerson").Id);

                PrecacheWorkflow(wf.Id);

                if (wf == null)
                    throw new ArgumentException();

                WorkflowUpdateHash = wf.WorkflowUpdateHash;

                foreach (var child in wf.ContainedActivities)
                {
                    if (!CachedInstances.ContainsKey(child.Id))
                    {
                        var childActivity = child.CreateWindowsActivity();

                        AddActivityToCache(childActivity);
                    }
                }
                PopulateTransitionCache(wf);
                PopulatePopulatedByCache(wf);
                PopulateArgumentCaches(wf);
                PopulateExpressionCaches(wf);
                CopyWorkflowProperties(wf);
            }
        }

        public void AddActivityToCache(ActivityImplementationBase activity)
        {
            long key = activity.ActivityInstance.Id;
            if (!_cachedInstances.ContainsKey(key))
            {
                _cachedInstances.Add(key, activity);

                _exitPointsCache.Add(key, activity.ActivityInstance.GetExitPoints());
            }
            else
            {
                throw new ApplicationException("Attempted to recache an activity instance:" + key);
            }
        }


		public bool HasViolations
		{
			get
			{
				return ValidationMessages.Count > 0;
			}
		}

        public void AddValidationError(string s)
        {
            ValidationMessages.Add(s);
        }

        public Transition GetTransitionForExitPoint(WfActivity activity, EntityRef exitPointRef)
        {
            Transition result;
            if (_transitionCache.TryGetValue(new Tuple<long, long>(activity.Id, exitPointRef.Id), out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<ExitPoint> GetExitpointsForActivity(long id)
        {
            return _exitPointsCache[id];
        }

        public ICollection<Transition> AllTransitions { get { return _transitionCache.Values; } }


        /// <summary>
        /// Prefetch the workflow run and related workflow an activities.
        /// </summary>
        /// <param name="workflowRunId"></param>
        private static void PrecacheWorkflow(long workflowId)
        {
            var expressionMapReq = "name, isOfType.id, expressionString, isTemplateString, argumentToPopulate.id, wfExpressionKnownEntities.{name, isOfType.id, referencedEntity.isOfType.id}";

            var activityArgumentReq = @"
                name, isOfType.id, argumentIsMandatory";        // We are not loading any of the value fields as they are only used is storing and loading state

            var activityTypeReq = "name, activityExecutionClass.{isOfType.id, typeName, assemblyName}, {inputArguments, internalArguments, outputArguments}.{" + activityArgumentReq + @"}, exitPoints.isOfType.id";

            var wfActivityReq = "name, isOfType.{" + activityTypeReq + @"}, expressionMap.{" + expressionMapReq + "}";

            var workflowRequest = @"
                name, isOfType.{" + activityTypeReq + @"},
                runtimeProperties.{name, isOfType.id},
                containedActivities.{" + wfActivityReq + @"},
                transitions.{name, fromActivity.isOfType.id, fromExitPoint.isOfType.id, toActivity.isOfType.id},
                firstActivity.isOfType.id,
                terminations.{fromActivity.isOfType.id, fromExitPoint.isOfType.id, workflowExitPoint.isOfType.id},
                workflowExitPoint.isOfType.id, 
                variables.{" + activityArgumentReq + @"},
                expressionParameters.{argumentInstanceActivity.isOfType.id, argumentInstanceArgument.isOfType.id, isOfType.id},
                inputArgumentForAction.{" + activityArgumentReq + @"},
                expressionMap.{" + expressionMapReq + @"}";

            BulkPreloader.Preload(new EntityRequest(workflowId, workflowRequest, "Preload workflow " + workflowId.ToString()));
        }
        
        public void PopulateTransitionCache(Workflow wf)
        {
            var transitionCount = wf.Transitions.Count();

            var uniqueTransitions = wf.Transitions.Distinct(new TransitionOriginComparer()).ToList();
            
            if (transitionCount != uniqueTransitions.Count)
                EventLog.Application.WriteInformation("Duplicate transitions origins detected and ignored in workflow '{0}'.", wf.Name);
        
            _transitionCache = uniqueTransitions.ToDictionary(t => new Tuple<long, long>(t.FromActivity.Id, t.FromExitPoint.Id));
        }
        
        public WfExpression GetPopulatedBy(WfActivity activity, ActivityArgument argument)
        {
            WfExpression result;
            var key = new Tuple<long, long>(activity.Id, argument.Id);
            if (_populatedByCache.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        
        private void PopulatePopulatedByCache(Workflow wf)
        {
            if (wf == null)
                return;

            if (wf.ContainedActivities == null)
                return;

            // When the expression is used the security will be tested for access.
            if (wf.ContainedActivities.Count > 0)
            {
                var actArgExprSet = wf.ContainedActivities.Where(
                    act => act.ExpressionMap != null && act.ExpressionMap.Count > 0)
                    .SelectMany(act =>
                        act.ExpressionMap.Where(exp => exp.ArgumentToPopulate != null)
                            .Select(
                                exp => new Tuple<long, long, WfExpression>(act.Id, exp.ArgumentToPopulate.Id, exp))
                    );

                try
                {
                    var result = actArgExprSet
                        .ToDictionary(
                            e => new Tuple<long, long>(e.Item1, e.Item2),
                            e => e.Item3);

                    _populatedByCache = result;
                }
                catch (ArgumentException ex)
                {
                    if (ex.Message.Contains("same key"))
                        AddValidationError("An expression has more than one argument to populate.");
                    else
                        throw;
                }
            }

        }

        private void PopulateArgumentCaches(Workflow wf)
        {
            // get the outputs
            var activityOutputs = wf.ContainedActivities.SelectMany(a => a.GetOutputArgumentInstances()).ToList();

            try
            {
                AllOutputTypes = activityOutputs.ToDictionary(ai => ai.Name, TypeFromInstance);
                AllOutputInstances = activityOutputs.ToDictionary(ai => ai.Name, ai => ai);
            } 
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("same key"))
                    AddValidationError("Clashing output arguments.");
                else throw;
            }

            // get the inputs and variables
            var allArgs = wf.InputArguments.Union(wf.Variables).ToList();

            try
            {
                AllInputAndVariableTypes = allArgs.ToDictionary(a => a.Name, CalcExpressionType); // Can't use GetCalcExpression as we have a chicken and egg scenario
                AllInputAndVariableArguments = allArgs.ToDictionary(a => a.Name, a => a);
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("same key"))
                    AddValidationError("An input argumment and a variable both have the same name.");
                else throw;
            }

        }

        private void PopulateExpressionCaches(Workflow wf)
        {
            using (Profiler.Measure("WorkflowMetadata.PopulateExpressionCache"))
            { 
                foreach (var wfExpr in wf.ExpressionMap)
                {
                    try
                    {
                        AddExpressionToCaches(wfExpr);
                    }
                    catch (Exception ex)
                    {
                        var messageStart ="Failed to compile workflow expression,";
                        AddValidationError(GenerateCompilationError(messageStart, wfExpr, ex));
                    }
                }

                foreach (var activity in wf.ContainedActivities)
                {
                    foreach (var wfExpr in activity.ExpressionMap)
                    {
                        try
                        {
                            AddExpressionToCaches(wfExpr);
                        }
                        catch (Exception ex)
                        {
                            var messageStart = string.Format("Failed to compile expression in activity: '{0}'", activity.Name ?? "[Unnamed]");
                            AddValidationError(GenerateCompilationError(messageStart, wfExpr, ex));
                        }

                    }
                }
            }
        }

        string GenerateCompilationError(string messageStart, WfExpression expression, Exception ex)
        {
            var errorMessage = ex is ParseException ? ex.Message : "Internal error";

            var result = string.Format(
                "{0} Message: '{1}' Expression: '{2}'",
                messageStart,
                errorMessage,
                expression.ExpressionString ?? "[Empty]"
                );

            if (!(ex is ParseException))
                EventLog.Application.WriteError("Unexpected error during workflow expression compilation. {0} \nInternal message {1}", result, ex.Message);

            return result;
        }

        private void AddExpressionToCaches(WfExpression wfExpr)
        {

            var argToPopulate = wfExpr.ArgumentToPopulate;

            if (argToPopulate != null)
            {
                _expressionArgumentCache.TryAdd(wfExpr.Id, argToPopulate);

                _expressionArgumentTypeCache.TryAdd(argToPopulate.Id, CalcExpressionType(argToPopulate));

                var expression = CompileExpression(wfExpr);
                
                CompiledExpressionCache.TryAdd(wfExpr.Id, expression);
            }
        }

        private IExpression CompileExpression(WfExpression wfExpr)
        {
            var expressionString = wfExpr.ExpressionString;

            if (string.IsNullOrWhiteSpace(expressionString))
                return null; // not sure if this is correct, just reproducing old behaviour.

            try
            {
                var resultType = GetExpressionType(wfExpr);

                var knownEntities = new Dictionary<string, Resource>();
                foreach (var ke in wfExpr.WfExpressionKnownEntities)
                {
                    if (ke.ReferencedEntity != null && !string.IsNullOrEmpty(ke.Name))
                    {
                        if (knownEntities.ContainsKey(ke.Name))
                        {
                            knownEntities[ke.Name] = ke.ReferencedEntity;
                        }
                        else
                        {
                            knownEntities.Add(ke.Name, ke.ReferencedEntity);
                        }
                    }
                }
                
                var bSettings = new BuilderSettings();
                bSettings.ScriptHost = ScriptHostType.Evaluate;

                if (resultType != null && resultType.Type != DataType.None)
                    bSettings.ExpectedResultType = resultType;

                bSettings.StaticParameterResolver = paramName => ResolveParameterType(paramName, knownEntities);
                // used for runtime parameters

                return Factory.ExpressionCompiler.Compile(expressionString, bSettings);
            }
            catch ( ParseException ex )
            {
                // User/application error with the substance of the calculation expression
                // e.g. The calculation «[Hello» has a problem: Mal-formed string literal - cannot find termination symbol. (pos 1)
                string message = $"The calculation «{expressionString}» has a problem: {ex.Message}";
                EventLog.Application.WriteWarning( message );
                AddValidationError( message );
            }
            catch ( Exception ex )
            {
                // Internal error
                string message = $"Internal error while compiling expression: {expressionString}";
                EventLog.Application.WriteError( ex.ToString( ) );
                AddValidationError( message );
            }

            return null;
        }

        private ExprType ResolveParameterType(string parameterName, IDictionary<string, Resource> knownEntities)
        {
            if (knownEntities != null && knownEntities.ContainsKey(parameterName))
            {
                //Note - the following is only looking at the first type for the known entity...
                return ExprTypeHelper.EntityOfType(knownEntities[parameterName].IsOfType.First());
            }

            if (parameterName == ownerAccountRuntimeArgName || parameterName == triggeringUserRuntimeArgName)
            {
                return ExprTypeHelper.EntityOfType(UserAccount.UserAccount_Type);
            }


            if (parameterName == triggeringPersonRuntimeArgName)
            {
                return ExprTypeHelper.EntityOfType(Person.Person_Type);
            }


            ExprType result = null;

            if (AllOutputTypes.TryGetValue(parameterName, out result))
            {
                return result;
            }

            if (AllInputAndVariableTypes.TryGetValue(parameterName, out result))
            {
                return result;
            }

            return null;
        }


        private void CopyWorkflowProperties(Workflow wf)
        {
            WfRunAsOwner = wf.WorkflowRunAsOwner ?? false;
            WfSecurityOwner = wf.SecurityOwner;
        }


        ExprType TypeFromInstance(WfArgumentInstance ai)
        {
            if (ai.InstanceConformsToType != null)
            {
                if (ai.ArgumentInstanceArgument.Is<ResourceArgument>())
                {
                    return ExprTypeHelper.EntityOfType(ai.InstanceConformsToType);
                }
                else
                {
                    return ExprTypeHelper.EntityListOfType(ai.InstanceConformsToType);
                }

            }
            else
            {
                return CalcExpressionType(ai.ArgumentInstanceArgument);
            }
        }

        readonly static Dictionary<string, ExprType> ArgTypeAliasToExprType = new Dictionary<string, ExprType>
        {
            {StringArgument.StringArgument_Type.Alias, ExprType.String},
            {IntegerArgument.IntegerArgument_Type.Alias, ExprType.Int32},
            {BoolArgument.BoolArgument_Type.Alias, ExprType.Bool},
            {DecimalArgument.DecimalArgument_Type.Alias, ExprType.Decimal},
            {CurrencyArgument.CurrencyArgument_Type.Alias, ExprType.Currency},
            //{ResourceArgument.ResourceArgument_Type, ExprTypeHelper.EntityOfType},
            {DateTimeArgument.DateTimeArgument_Type.Alias,ExprType.DateTime},
            {DateArgument.DateArgument_Type.Alias, ExprType.Date},
            {TimeArgument.TimeArgument_Type.Alias, ExprType.Time},
            {ObjectArgument.ObjectArgument_Type.Alias, ExprType.None},
            //{ResourceListArgument.ResourceListArgument_Type, ExprType.EntityList}
            {GuidArgument.GuidArgument_Type.Alias, ExprType.Guid}
        };


        public ActivityArgument GetArgumentPopulatedByExpression(WfExpression wfExpr)
        {
            ActivityArgument result;

            if (!_expressionArgumentCache.TryGetValue(wfExpr.Id, out result))
                throw new ArgumentException("Attempted to get an argument for an expression that was not added to the cache.");

            return result;
        }



        public ExprType GetExpressionType(WfExpression wfExpr)
        {
            var arg = GetArgumentPopulatedByExpression(wfExpr);
            return GetExpressionType(arg);
        }

        private ExprType GetExpressionType(ActivityArgument arg)
        {
            ExprType result;

            if (!_expressionArgumentTypeCache.TryGetValue(arg.Id, out result))
                throw new ArgumentException("Tried to get an expression type for an uncached expression.");

            return result;
        }

        private ExprType CalcExpressionType(ActivityArgument arg)
        { 
            ExprType result;

            var resArg = arg.As<ResourceArgument>();
            if (resArg != null)
            {
                var resType = resArg.ConformsToType ?? Resource.Resource_Type;
                result = ExprTypeHelper.EntityOfType(resType);
            }
            else
            {

                var resListArg = arg.As<ResourceListArgument>();
                if (resListArg != null)
                {
                    var resType = resListArg.ConformsToType ?? Resource.Resource_Type;

                    result = ExprType.EntityList.Clone();
                    result.EntityType = resType;
                }
                else
                {
                    var argType = arg.GetArgumentType();
                    result = ArgTypeAliasToExprType[argType.Alias];
                }
            }
           
            return result;
        }

    }



    public class WorkflowValidationException : WorkflowRunException
    {
        public IEnumerable<String> ValidationMessages { get; private set; }
        public Workflow Workflow { get; private set; }
        public WfActivity Activity { get; private set; }

        public WorkflowValidationException(long _workflowRunId, Workflow wf, WfActivity activity, IEnumerable<string> messages)
            : base("")
        {
            ValidationMessages = messages.ToList();
            Workflow = wf;
            Activity = activity;
        }

        public override string Message
        {
            get
            {
                return string.Format(
                    "Workflow is invalid:\n\tWorkflow: {0}({1})\n\tActivity: {2}({3})\n\tMessages: {4}",
                    Workflow != null ? Workflow.Name ?? "[Unnamed]": "[null]",
                    Workflow != null ? Workflow.Id.ToString():   "[null]",
                    Activity != null ? Activity.Name ?? "[Unnamed]": "[null]",
                    Activity != null ? Activity.Id.ToString() : "[null]",
                    ValidationMessages.Aggregate((workingSentence, next) =>
                                                   workingSentence + "\n" + next));
            }
        }


    }

    /// <summary>
    /// Determine if two value transitions have the same source and destination.
    /// If they are invalid, all bets are off        /// </summary>
    internal class TransitionOriginComparer : IEqualityComparer<Transition>
    {
        /// <summary>
        ///     Determine if two entities are the same.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(Transition x, Transition y)
        {
            if (!IsValid(x) || !IsValid(y))
                return false;

            return x.FromActivity.Id == y.FromActivity.Id
                && x.FromExitPoint.Id == y.FromExitPoint.Id;
        }

        /// <summary>
        ///     Calculates the hash-code.
        /// </summary>
        public int GetHashCode(Transition obj)
        {
            if (!IsValid(obj))
            {
                return 0;
            }

            // cofuzzing with Berstein hash
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + obj.FromActivity.Id.GetHashCode();
                hash = hash * 31 + obj.FromExitPoint.Id.GetHashCode();
                return hash;
            }
        }

        bool IsValid(Transition t)
        {
            return t != null && t.FromActivity != null && t.FromExitPoint != null && t.ToActivity != null;
        }
    }


}
