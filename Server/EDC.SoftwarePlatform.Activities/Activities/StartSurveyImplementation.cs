// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Distributes a survey for completion.
    /// </summary>
    public class StartSurveyImplementation : ResumableActivityImplementationBase
    {
        private const decimal DefaultTimeOutMins = 0;     // no time out

        /// <summary>
        /// Start the activity running
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused. Along with a sequence number of if it is paused</returns>
        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var pause = GetArgumentValue<bool>(inputs, "inStartSurveyPause", false);
            var campaign = GetArgumentEntity<SurveyCampaign>(inputs, "inStartSurveyCampaign");
            var dueInDays = GetArgumentValue<decimal>(inputs, "inStartSurveyDueDays", DefaultTimeOutMins);
            var targetObject = GetArgumentEntity<UserResource>(inputs, "inStartSurveyTarget");
            var taskName = GetArgumentValue<string>(inputs, "inStartSurveyTaskName");
            
            var dueOn = campaign.SurveyClosesOn;
            if (dueInDays > 0)
                dueOn = DateTime.UtcNow.AddDays(decimal.ToDouble(dueInDays));

            var tasks = campaign.Launch(dueOn, targetObject, taskName).ToList();

            if (pause)
            {
                foreach (var task in tasks)
                    context.SetUserTask(task.Cast<BaseUserTask>());     // this will do the save for us.
            }
            else
            {
                Entity.Save(tasks);
            }

            var responses = tasks.Select(t => t.UserSurveyTaskSurveyResponse);
            context.SetArgValue(ActivityInstance, GetArgumentKey("core:outStartSurveyResponses"), responses);

            // Deal with time-outs. Time-outs can only occur if there is due date and we are pausing.
            if (pause)
            {
                SetTimeoutIfNeeded(context, dueInDays);
            }

            // Note that there is no need to set an exit point if we are not pausing as the default exit point will be used.
            return !pause;
        }

        /// <summary>
        /// Continue a paused activity
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused.</returns>
        public override bool OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            HandleTimeout(context, resumeEvent);

            return true;
        }
    }
}
