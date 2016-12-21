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
            var targets = GetArgumentEntityList<UserResource>(inputs, "inLaunchTargetTargets").ToList();
            var survey = GetArgumentEntity<UserSurvey>(inputs, "inLaunchTargetSurvey");
            var surveyTaker = GetArgumentEntity<Relationship>(inputs, "inLaunchTargetSurveyTaker");
            var targetDefinition = GetArgumentEntity<Definition>(inputs, "inLaunchTargetTargetObject");
            var taskName = GetArgumentValue<string>(inputs, "inLaunchTargetTaskName");
            var dueInDays = GetArgumentValue(inputs, "inLaunchTargetDueDays", 0m);
            var pause = GetArgumentValue(inputs, "inLaunchTargetPause", false);

            EntityType targetType;
            DirectionEnum_Enumeration targetDirection;

            // ensure that there is at least one target
            if (targets.Count <= 0)
            {
                throw new WorkflowRunException("The targets list for the Survey was empty.");
            }

            // ensure the relationship is valid (as there is no UI validation in workflow)
            var surveyTakerAttachesToPerson =
                surveyTaker.ToType.GetAncestorsAndSelf().Select(t => t.Id).Contains(Person.Person_Type.Id) ||
                surveyTaker.FromType.GetAncestorsAndSelf().Select(t => t.Id).Contains(Person.Person_Type.Id);

            if (!surveyTakerAttachesToPerson)
            {
                throw new WorkflowRunException("Invalid relationship used for Survey taker. Must connect to Person.");
            }

            // Derive the direction and (base-most) target type for this campaign, based on the relationship that has been input
            if (surveyTaker.ToType
                .GetAncestorsAndSelf().Select(t => t.Id)
                .Contains(Person.Person_Type.Id))
            {
                targetType = surveyTaker.FromType;
                targetDirection = DirectionEnum_Enumeration.Forward;
            }
            else
            {
                targetType = surveyTaker.ToType;
                targetDirection = DirectionEnum_Enumeration.Reverse;
            }

            // If a target object definition has been specified, then enforce it
            if (targetDefinition != null)
            {
                // override the target type derived above
                targetType = targetDefinition.As<EntityType>();

                // all the inputs we have should be of this type
                var targetsConform = targets.All(target =>
                {
                    return target.GetAllTypes().Select(t => t.Id).Contains(targetType.Id);
                });

                if (!targetsConform)
                {
                    throw new WorkflowRunException("All targets must conform to the Target object if it has been specified.");
                }
                
                // validate that the relationship also connects to the target object type
                var relTypeToCheck = targetDirection == DirectionEnum_Enumeration.Forward
                    ? surveyTaker.FromType
                    : surveyTaker.ToType;

                var surveyTakerAttachesToTargetObject =
                    targetType.GetAncestorsAndSelf().Select(t => t.Id).Contains(relTypeToCheck.Id);

                if (!surveyTakerAttachesToTargetObject)
                {
                    throw new WorkflowRunException("Invalid relationship used for Survey taker. Must connect to the Target object if it has been specified.");
                }
            }

            var campaign = Entity.Create<SurveyTargetCampaign>();
            campaign.Name = $"Target Campaign ({survey.Name})";
            campaign.SurveyForCampaign = survey;
            campaign.CampaignTarget = targetType.As<Definition>();
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
