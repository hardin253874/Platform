// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Diagnostics;
using EDC.SoftwarePlatform.Activities.Engine;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.Activities
{


    public static class WorkflowActivityHelper
    {

        public const string ExitPointIdKeyName = "ExitPointId";


        public static object GetValueFromDictionary(IDictionary<string, object> dictionary, string key)
        {
            return dictionary[key];
        }


        public static string GetIdString(this Workflow workflow)
        {
            return string.Format("'{0}' Id={1}", workflow.Name ?? "unnamed", workflow.Id);
        }




        public static ActivityArgument GetInputArgument(this WfActivity activity, string implementationName)
        {
            return activity.GetInputArguments().FirstOrDefault(a => a.Name == implementationName);     // note that his code assumes the first type is an activity type
        }

        public static IEnumerable<ActivityArgument> GetInputArguments(this WfActivity activity)
        {
            // for a workflow the input arguments are on the instance, for an activity they are on the type
            var typeArguments = activity.IsOfType.Select(t=>t.As<ActivityType>()).First(t=>t != null).InputArguments;

            var entityWithArgsAndExits = activity.As<EntityWithArgsAndExits>();
            if (entityWithArgsAndExits != null)
                return entityWithArgsAndExits.InputArguments.Union(typeArguments);
            else
                return typeArguments; // note that his code assumes the first type is an activity type
        }



        public static ActivityArgument GetOutputArgument(this WfActivity activity, string implementationName)
        {
            return activity.GetOutputArguments().FirstOrDefault(a => a.Name == implementationName);     // note that his code assumes the first type is an activity type
        }

        public static IEnumerable<ActivityArgument> GetOutputArguments(this WfActivity activity)
        {
            IEnumerable<ActivityArgument> result;
            // get the type defined arguments
            var type = activity.IsOfType.Select(t => t.As<ActivityType>()).First(t => t != null);
            result = type.OutputArguments;

            // get the instance defined arguments
            var entityWithArgsAndExits = activity.As<EntityWithArgsAndExits>();
            if (entityWithArgsAndExits != null)
            {
                result = result.Union(entityWithArgsAndExits.OutputArguments);
            }

            return result;
        }

        public static WfArgumentInstance GetOutputArgumentInstance(this WfActivity activity, string implementationName)
        {
            return activity.GetOutputArgumentInstances().FirstOrDefault(a => a.ArgumentInstanceArgument.Name == implementationName);     // note that his code assumes the first type is an activity type
        }


        /// <summary>
        /// Get the argumentInstances for the activity, that is all the instances off the arguments that have an activity matching this one
        /// </summary>
        public static IEnumerable<WfArgumentInstance> GetOutputArgumentInstances(this WfActivity activity)
        {
            var argIds = activity.GetOutputArguments().Select(arg=>arg.Id);
            return activity.ArgumentInstanceFromActivity.Where(ai => ai.ArgumentInstanceArgument != null && argIds.Contains(ai.ArgumentInstanceArgument.Id));
        }

        public static IEnumerable<ActivityArgument> GetInternalVariables(this WfActivity activity)
        {
                var type = activity.IsOfType.Select(t => t.As<ActivityType>()).First(t => t != null);
                return type.InternalArguments;
        }



        #region Validation

        /// <summary>
        /// Validate the given workflow. Throw an exception is it is not valid.
        /// </summary>
        /// <param name="wf"></param>
        public static IEnumerable<string> Validate(this Workflow wf)
        {
            using (Profiler.Measure("WorkflowActivityHelper.Validate"))
            using (new SecurityBypassContext())
            {

                var nextActivity = wf.Cast<WfActivity>().CreateWindowsActivity();

                var metaData = Factory.Current.Resolve<IWorkflowMetadataFactory>().Create(wf);

                return metaData.ValidationMessages;
            }
        }

        


        #endregion



    }
}
