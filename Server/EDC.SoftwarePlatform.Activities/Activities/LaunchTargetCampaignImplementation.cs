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
    public class LaunchTargetCampaignImplementation : ResumableActivityImplementationBase
    {
        /// <summary>
        /// Start the activity running
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused. Along with a sequence number of if it is paused</returns>
        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var survey = GetArgumentEntity<UserSurvey>(inputs, "inLaunchTargetSurvey");
            var targets = GetArgumentEntityList<UserResource>(inputs, "inLaunchTargetTargets");
            var surveyTaker = GetArgumentEntity<Relationship>(inputs, "inLaunchTargetSurveyTaker");
            var taskName = GetArgumentValue<string>(inputs, "inLaunchTargetTaskName");
            var dueInDays = GetArgumentValue(inputs, "inLaunchTargetDueDays", 0m);
            var pause = GetArgumentValue(inputs, "inLaunchTargetPause", false);

            // Derive the target type for this campaign, based on the relationship that has been input
            EntityType target;
            DirectionEnum_Enumeration targetDirection;
            if (surveyTaker.ToType
                .GetAncestorsAndSelf().Select(t => t.Id)
                .Contains(Person.Person_Type.Id))
            {
                target = surveyTaker.FromType;
                targetDirection = DirectionEnum_Enumeration.Forward;
            }
            else
            {
                target = surveyTaker.ToType;
                targetDirection = DirectionEnum_Enumeration.Reverse;
            }

            var campaign = Entity.Create<SurveyTargetCampaign>();
            campaign.Name = $"Target Campaign ({survey.Name})";
            campaign.SurveyForCampaign = survey;
            campaign.CampaignTarget = target.As<Definition>();
            campaign.CampaignTargetRelationship = surveyTaker;
            campaign.CampaignTargetRelationshipDirection_Enum = targetDirection;
            campaign.CampaignTargetTargets.AddRange(targets);
            if (dueInDays > 0)
            {
                campaign.SurveyClosesOn = DateTime.UtcNow.AddDays(decimal.ToDouble(dueInDays));
            }
            
            var tasks = campaign.Launch(campaign.SurveyClosesOn, taskName).ToList();

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
            context.SetArgValue(ActivityInstance, GetArgumentKey("core:outLaunchTargetResponses"), responses);

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
