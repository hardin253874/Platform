// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Distributes a survey for completion.
    /// </summary>
    public class LaunchPersonCampaignImplementation : ResumableActivityImplementationBase
    {
        /// <summary>
        /// Start the activity running
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused. Along with a sequence number of if it is paused</returns>
        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var survey = GetArgumentEntity<UserSurvey>(inputs, "inLaunchPersonSurvey");
            var recipients = GetArgumentEntityList<Person>(inputs, "inLaunchPersonRecipients").ToList();
            var targetObject = GetArgumentEntity<UserResource>(inputs, "inLaunchPersonTarget");
            var taskName = GetArgumentValue<string>(inputs, "inLaunchPersonTaskName");
            var dueInDays = GetArgumentValue(inputs, "inLaunchPersonDueDays", 0m);
            var pause = GetArgumentValue(inputs, "inLaunchPersonPause", false);

            // ensure that there is at least one recipient
            if (recipients.Count <= 0)
            {
                throw new WorkflowRunException("The recipients list for the Survey was empty.");
            }

            var campaign = Entity.Create<SurveyPersonCampaign>();
            campaign.Name = $"Person Campaign ({survey.Name})";
            campaign.SurveyForCampaign = survey;
            campaign.CampaignPersonRecipients.AddRange(recipients);
            if (dueInDays > 0)
            {
                campaign.SurveyClosesOn = DateTime.UtcNow.AddDays(decimal.ToDouble(dueInDays));
            }

            var tasks = campaign.Launch(campaign.SurveyClosesOn, targetObject, taskName).ToList();

            if (pause)
            {
                foreach (var task in tasks)
                {
                    // this will do the save for us.
                    context.SetUserTask(task.Cast<BaseUserTask>());
                }
            }
            else
            {
                Entity.Save(tasks);
            }

            var responses = tasks.Select(t => t.UserSurveyTaskSurveyResponse);
            context.SetArgValue(ActivityInstance, GetArgumentKey("core:outLaunchPersonResponses"), responses);

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
