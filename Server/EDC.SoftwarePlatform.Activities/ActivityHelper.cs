// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities
{
    public static class ActivityHelper
    {


        ///// <summary>
        ///// The default time before an activity timeouts
        ///// </summary>
        //public static TimeSpan DefaultActivityTimeout { get { return new TimeSpan(0, 1, 0); } } //TODO: move into system configuration

        ///// <summary>
        ///// The default number of times a workflow will execute directly contained activities.
        ///// </summary>
        //public static int DefaultWorkflowMaxActivityExecutes { get { return 1000; } } //TODO: move into system configuration


        /// <summary>
        /// The default exit point for an activity
        /// </summary>
        public static IEnumerable<ExitPoint> GetExitPoints(this WfActivity activity)
        {
            IEnumerable<ExitPoint> result = new List<ExitPoint>();

            var ioe = activity.As<EntityWithArgsAndExits>();
            if (ioe != null)
                result = ioe.ExitPoints.ToList();

            var aType = activity.IsOfType.FirstOrDefault(t => t.Is<ActivityType>());

            if (aType != null)
                result = result.Union(aType.Cast<ActivityType>().ExitPoints.ToList());
                      
            return result;
        }

        /// <summary>
        /// The default exit point for an activity
        /// </summary>
        public static ExitPoint GetDefaultExitPoint(this WfActivity activity)
        {
            ExitPoint result = null;

            var ioe = activity.As<EntityWithArgsAndExits>();
            if (ioe != null)
                result = ioe.ExitPoints.FirstOrDefault(ep => ep.IsDefaultExitPoint ?? false);

            if (result == null)
            {
                var aType = activity.IsOfType.FirstOrDefault(t => t.Is<ActivityType>());
                
                if (aType != null)
                    result = aType.Cast<ActivityType>().ExitPoints.FirstOrDefault(ep => ep.IsDefaultExitPoint ?? false);
            }

            if (result == null)
                    throw new ApplicationException(string.Format("Unable to find the default exit point for {0}({1})", activity.Name ?? "[Unnamed]", activity.Id));

            return result;
        }

        /// <summary>
        /// The default exit point for an activity
        /// </summary>
        public static ExitPoint GetDefaultExitPoint(this Workflow wf)
        {
            return wf.As<WfActivity>().GetDefaultExitPoint();
        }



        /// <summary>
        /// Create a windows activity instance for the given model activity
        /// </summary>
        /// <param name="nextActivity"></param>
        /// <param name="workflowRunId">The ID of the workflow run to record information against. 0 indicates there is no run.</param>
        /// <returns></returns>
        public static ActivityImplementationBase CreateWindowsActivity(this WfActivity nextActivity)
        {
            if ((nextActivity as IEntityInternal).IsTemporaryId)
                throw new ArgumentException(string.Format("The activity '{0}':{1} has not been saved. All activities must be saved before being used.", nextActivity.Name, nextActivity.Id));

            
            var result = ActivityHelper.GetBackingClassInstanceForWfActivity(nextActivity);

            result.Initialize(nextActivity);

            return result;
        }


        private static ActivityImplementationBase GetBackingClassInstanceForWfActivity(WfActivity activity)
        {
            var backingClassEntity = activity.IsOfType.Select(t=>t.As<ActivityType>()).First(t=> t != null);
            Class firstActivityBackingClass = backingClassEntity.ActivityExecutionClass;
            var s = typeof(LogActivityImplementation).FullName;
            var q = typeof(LogActivityImplementation).FullName;
            var typeString = string.Format("{0}, {1}", firstActivityBackingClass.TypeName, firstActivityBackingClass.AssemblyName);
            var activityType = Type.GetType(typeString, true);
            var activityInstance = (ActivityImplementationBase) Activator.CreateInstance(activityType);

           

            return activityInstance;
        }



        ///// <summary>
        ///// Create a transition between the given activity instances for the provided exit point
        ///// </summary>
        //public static void AddTermination(Workflow wf, WfActivity from)
        //{
        //    var fromDefault = from.GetDefaultExitPoint();
        //    var wfDefault = wf.As<WfActivity>().GetDefaultExitPoint();

        //    if (fromDefault == null)
        //        throw new ArgumentException("From activity missing default exit point.");

        //    if (wfDefault == null)
        //        throw new ArgumentException("Workflow missing default exit point.");

        //    AddTermination(wf, from, fromDefault, wfDefault, null);
        //}
    }


}
