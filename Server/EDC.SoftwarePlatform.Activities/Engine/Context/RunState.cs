// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// 
    /// </summary>
    public class RunState : RunStateBase
    {
        readonly string ownerAccountRuntimeArgName;
        readonly string triggeringUserRuntimeArgName;
        readonly string triggeringPersonRuntimeArgName;
        
        private readonly IDictionary<Tuple<long, long>, WfArgumentInstance> _argInstCache =
            new Dictionary<Tuple<long, long>, WfArgumentInstance>();

        public RunState(WorkflowMetadata metaData, WorkflowRun workflowRun, RequestContextData effectiveSecurityContext)
            : base(metaData, workflowRun, effectiveSecurityContext)
        {
            ownerAccountRuntimeArgName = Entity.GetName(new EntityRef("workflowOwnerAccount").Id);
            triggeringUserRuntimeArgName = Entity.GetName(new EntityRef("triggeringUserAccount").Id);
            triggeringPersonRuntimeArgName = Entity.GetName(new EntityRef("triggeringPerson").Id);
        }

        private IDictionary<Tuple<long, long>, object> argValues = new Dictionary<Tuple<long, long>, object>();
        
        /// <summary>
        /// Set the value of an argument instance in the context
        /// </summary>
        /// <param name="targetArg"></param>
        /// <param name="value"></param>
        public override void SetArgValue(ActivityArgument targetArg, object value)
        {
            SetArgValue_Imp(0, targetArg.Id, value);
        }

        /// <summary>
        /// Set the value of an argument instance in the context
        /// </summary>
        /// <param name="targetArgInst"></param>
        /// <param name="value"></param>
        /// <param name="activity"></param>
        public override void SetArgValue(WfActivity activity, ActivityArgument targetArg, object value)
        {
            SetArgValue_Imp(activity != null ? activity.Id : 0, targetArg.Id, value);
        }

        private void SetArgValue_Imp(long activityId, long targetArgId, object value)
        {
            if (!IsValueArgValueType(value))
                throw new ArgumentException("Attempting to set a variable with a value that is not an accepted type: " + value.GetType().AssemblyQualifiedName);

            var key = new Tuple<long, long>(activityId, targetArgId);

            argValues[key] = value;
        }

        public WfArgumentInstance GetArgInstance(ActivityArgument targetArg, WfActivity activity)
        {
            WfArgumentInstance targetArgInst = null;
            var key = new Tuple<long, long>(activity.Id, targetArg.Id);

            if (_argInstCache.TryGetValue(key, out targetArgInst))
            {
                return targetArgInst;
            }

            targetArgInst = targetArg.GetArgInstance(activity);

            _argInstCache.Add(key, targetArgInst);

            return targetArgInst;
        }
        
        public override T GetArgValue<T>(WfActivity activity, ActivityArgument targetArg)
        {
            return (T)GetArgValue_imp(activity.Id, targetArg.Id);
        }

        public override T GetArgValue<T>(ActivityArgument targetArg)
        {
            return (T)GetArgValue_imp(0, targetArg.Id);
        }

        /// <summary>
        /// Get rid of all internal ags, only leaving the output arguments
        /// </summary>
        public override void FlushInternalArgs()
        {
            var workflow = WorkflowRun.WorkflowBeingRun;
            if (workflow != null)
            {
                var outputArgIds = workflow.OutputArguments.Select(a => a.Id);

                var otherKeys = argValues.Where(kvp => !outputArgIds.Contains(kvp.Key.Item2)).Select(kvp => kvp.Key).ToList();

                foreach (var key in otherKeys)
                    argValues.Remove(key);
            }
        }
       
        /// <summary>
        /// Set the value of an argument instance in the context
        /// </summary>
        private object GetArgValue_imp(long activityId, long targetArgId)
        {
            var key = new Tuple<long, long>(activityId, targetArgId);

            object o = null;

            if (argValues.TryGetValue(key, out o))
            {
                return o;
            }
            
            return o;
        }

        /// <summary>
        /// Get all the stored argument values
        /// </summary>
        /// <returns></returns>
        override public IEnumerable<Tuple<WfActivity, ActivityArgument, object>> GetArgValues()
        {
            return argValues.Select(kvp => new Tuple<WfActivity, ActivityArgument, object>(
                 kvp.Key.Item1 == 0 ? WorkflowRun.WorkflowBeingRun.As<WfActivity>() : Entity.Get<WfActivity>(kvp.Key.Item1), 
                 Entity.Get<ActivityArgument>(kvp.Key.Item2), kvp.Value));
        }
        
        public override IDictionary<string, object> GetResult(IEnumerable<ActivityArgument> args = null)
        {
            if (args == null)
            {
#if DEBUG
                if (
                    WorkflowRun.WorkflowBeingRun.OutputArguments.Any(
                        arg => arg.ArgumentInstanceFromArgument == null || arg.ArgumentInstanceFromArgument.Count() != 1))
                {
                    throw new Exception("Workflow seems to be missing arginstances for the output args");
                }
#endif
                args = WorkflowRun.WorkflowBeingRun.OutputArguments;
            }

            var result = args.ToDictionary(arg => arg.Name,
                                         arg => GetArgValue<object>(WorkflowRun.WorkflowBeingRun.As<WfActivity>(), arg));

            result.Add(WorkflowActivityHelper.ExitPointIdKeyName, ExitPointId);
            return result;
        }

        /// <summary>
        /// Remove any references to this item, used after a delete.
        /// </summary>
        /// <param name="removedEntityRef">The entity that was removed.</param>
        public override void RemoveReference(EntityRef removedEntityRef)
        {
            var remove = argValues.Where(a => a.Value is Resource && ((Resource)a.Value).Id == removedEntityRef.Id).ToList();
            
            remove.ForEach(r => argValues.Remove(r));
        }


        /// <summary>
        /// Attempt to resolve the value of a named parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter. (In the script, this will be prefixed with @, but here it is without the prefix).</param>
        /// <param name="knownEntities">Lookup of names to values.</param>
        /// <returns>The actual value of the parameter.</returns>
        override public object ResolveParameterValue(string parameterName, IDictionary<string, Resource> knownEntities)
        {
            if (parameterName == ownerAccountRuntimeArgName)
            {
                return WorkflowRun.WorkflowBeingRun.SecurityOwner;
            }

            if (parameterName == triggeringUserRuntimeArgName)
            {
                EnsureTriggeringUser(WorkflowRun);

                return WorkflowRun.TriggeringUser;
            }

            if (parameterName == triggeringPersonRuntimeArgName)
            {
                EnsureTriggeringUser(WorkflowRun);

                return WorkflowRun.TriggeringUser != null ? WorkflowRun.TriggeringUser.AccountHolder : null;
            }

            if (knownEntities != null && knownEntities.ContainsKey(parameterName))
            {
                return knownEntities[parameterName];
            }

            WfArgumentInstance argInst;
            ActivityArgument arg;

            if (Metadata.AllOutputInstances.TryGetValue(parameterName, out argInst))
            {
                return GetArgValue<object>(argInst.ArgumentInstanceActivity, argInst.ArgumentInstanceArgument);

            }
            
            if (Metadata.AllInputAndVariableArguments.TryGetValue(parameterName, out arg))
            {
                return GetArgValue<object>(arg);
            }

            throw new ArgumentException("Tried to get a Parameter Value where there was none", parameterName);
        }

        private static UserAccount EnsureTriggeringUser(WorkflowRun workflowRun)
        {
            if (!workflowRun.IsTemporaryId || workflowRun.TriggeringUser != null)
                return null;

            var deferredRun = workflowRun as WorkflowRunDeferred;
            if (deferredRun != null && deferredRun.TriggeringUserId > 0)
            {
                return deferredRun.GetTriggeringUser();
            }

            return null;
        }
    }
}
