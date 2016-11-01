// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Activities;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{

    public sealed class ForeachImplementation : ActivityImplementationBase, IRunNowActivity
    {
        public const string LoopExitPointAlias = "core:foreachLoopExitPoint";
      
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var haveStarted = GetArgumentKey("forEachHaveStarted");
            var foreachList = GetArgumentKey("forEachResourceList");
            var resourceListKey = GetArgumentKey("foreachList");
            var selectedResourceKey = GetArgumentKey("foreachSelectedResource");


            IEnumerable<IEntity> resourceList;

             
            if (!(context.GetArgValue<bool?>(ActivityInstance, haveStarted) ?? false))
            {
                context.SetArgValue(ActivityInstance, haveStarted, true);

                //
                // it's the first time into the loop
                //
                object listObj;
                if (inputs.TryGetValue(resourceListKey, out listObj))
                {
                    if (listObj == null)
                        throw new ApplicationException("The [List] argument is null. This should never happen.");

                    resourceList = (IEnumerable<IEntity>) listObj;
                }
                else
                    throw new ApplicationException("The [List] is missing from the input arguments. This should never happen.");
            }
            else
            {
                resourceList = context.GetArgValue<IEnumerable<IEntity>>(ActivityInstance, foreachList);
            }

            var selectedResourceRef = resourceList.FirstOrDefault();


            if (selectedResourceRef != null)
            {
                context.SetArgValue(ActivityInstance, selectedResourceKey, selectedResourceRef);
                resourceList = resourceList.Skip(1);
                context.SetArgValue(ActivityInstance, foreachList, resourceList);
                context.ExitPointId = Entity.GetId(LoopExitPointAlias);
            }
            else
            {
                // We have finished
                context.SetArgValue(ActivityInstance, haveStarted, false);
                context.SetArgValue(ActivityInstance, foreachList, null);
                context.ExitPointId = Entity.GetId("core", "foreachCompletedExitPoint");
            }
        }

        public override ICollection<WfExpression> GetInputArgumentsExpressions(IRunState runState, WfActivity activity)
        {
            var expressions = base.GetInputArgumentsExpressions(runState, activity);

            var haveStarted = GetArgumentKey("forEachHaveStarted");

            // Only do the expression fetch if we are not in a running loop
            if (runState.GetArgValue<bool?>(ActivityInstance, haveStarted) ?? false)
            {
                expressions = expressions.Where(e => e.ArgumentToPopulate.Alias != "core:foreachList").ToList() ;
            } 

            return expressions;
        }

        public override void Validate(WorkflowMetadata metadata)
        {
            base.Validate(metadata);

            var activityInstanceAs = ActivityInstance.Cast<ForEachResource>();

            var terminationsCount =
                activityInstanceAs.ContainingWorkflow.Terminations.Count(t => t.FromActivity.Id == activityInstanceAs.Id);

            var transitionsCount = activityInstanceAs.ContainingWorkflow.Transitions.Count(t => t.FromActivity.Id == activityInstanceAs.Id);

            if (transitionsCount + terminationsCount != 2)
            {
                metadata.AddValidationError(string.Format("Every 'For Each' activity must have a Loop and a Finish transition. Activity: '{0}'", activityInstanceAs.Name));
            }
        }
    }

}
