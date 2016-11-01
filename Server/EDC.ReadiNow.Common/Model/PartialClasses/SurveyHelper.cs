// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Model.PartialClasses
{
    /// <summary>
    /// Helper class for Surveys
    /// </summary>
    public static class SurveyHelper
    {
        /// <summary>
        /// Create the results and task for a survey.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="taskDueOn">Task due on date, null indicates no due date.</param>
        /// <param name="targetObject">The target object.</param>
        /// <param name="taskName">If not null or empty, use as task name.</param>
        /// <returns>The tasks for each recipient of the survey.</returns>
        public static IEnumerable<UserSurveyTask> Launch(this SurveyCampaign campaign, DateTime? taskDueOn = null, UserResource targetObject = null, string taskName = null)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            // relaunch is not permitted.
            if (campaign.CampaignIsLaunched == true)
            {
                throw new Exception("Campaign has already been launched.");
            }

            campaign = campaign.AsWritable<SurveyCampaign>();
            campaign.CampaignIsLaunched = true;
            campaign.CampaignLaunchedOn = DateTime.UtcNow;
            campaign.Save();
            
            return LaunchCampaign(campaign, taskDueOn, targetObject, taskName);
        }

        public static IEnumerable<UserSurveyTask> Launch(this SurveyPersonCampaign campaign, DateTime? taskDueOn = null,
            UserResource targetObject = null, string taskName = null)
        {
            return LaunchPersonCampaign(campaign, taskDueOn, targetObject, taskName);
        }

        public static IEnumerable<UserSurveyTask> Launch(this SurveyTargetCampaign campaign, DateTime? taskDueOn = null,
            string taskName = null)
        {
            return LaunchTargetCampaign(campaign, taskDueOn, taskName);
        }

        public static SurveyResponse CreateSurveyResponse(this SurveyPersonCampaign campaign, Person recipient, UserResource targetObject = null)
        {
            return CreateSurveyResponse(campaign.As<SurveyCampaign>(), recipient, targetObject);
        }

        public static SurveyResponse CreateSurveyResponse(this SurveyTargetCampaign campaign, Person recipient, UserResource targetObject = null)
        {
            return CreateSurveyResponse(campaign.As<SurveyCampaign>(), recipient, targetObject);
        }

        public static UserSurveyTask CreateTaskForResponse(this SurveyPersonCampaign campaign, SurveyResponse result)
        {
            return CreateTaskForResponse(campaign.As<SurveyCampaign>(), result);
        }

        public static UserSurveyTask CreateTaskForResponse(this SurveyTargetCampaign campaign, SurveyResponse result)
        {
            var task = CreateTaskForResponse(campaign.As<SurveyCampaign>(), result);
            task.UserSurveyTaskAllowTargetEdit = false;
            return task;
        }

        private static IEnumerable<UserSurveyTask> LaunchCampaign(SurveyCampaign campaign, DateTime? taskDueOn, UserResource targetObject, string taskName)
        {
            // If the campaign is directed at people then process accordingly
            var personCampaign = campaign.As<SurveyPersonCampaign>();
            if (personCampaign != null)
            {
                return personCampaign.Launch(taskDueOn, targetObject, taskName);
            }

            // otherwise the campaign is targeting objects
            var targetCampaign = campaign.As<SurveyTargetCampaign>();
            if (targetCampaign != null)
            {
                return targetCampaign.Launch(taskDueOn, taskName);
            }

            return Enumerable.Empty<UserSurveyTask>();
        }

        private static IEnumerable<UserSurveyTask> LaunchPersonCampaign(SurveyPersonCampaign campaign, DateTime? taskDueOn, UserResource targetObject, string taskName)
        {
            var tasks = new List<UserSurveyTask>();

            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var survey = campaign.SurveyForCampaign;

            if (survey == null)
                throw new InvalidCampaignException("Survey person campaign must be part of an existing survey.");

            if (!campaign.CampaignPersonRecipients.Any())
                throw new InvalidCampaignException("Survey person campaign does not have any recipients.");

            campaign = campaign.AsWritable<SurveyPersonCampaign>();

            foreach (var person in campaign.CampaignPersonRecipients)
            {
                var result = campaign.CreateSurveyResponse(person, targetObject);
                campaign.SurveyResponses.Add(result);

                var userSurveyTask = campaign.CreateTaskForResponse(result);

                if (!string.IsNullOrWhiteSpace(taskName))
                    userSurveyTask.Name = taskName;

                if (taskDueOn != null)
                    userSurveyTask.UserTaskDueOn = taskDueOn;

                result.UserSurveyTaskForResults.Add(userSurveyTask);

                tasks.Add(userSurveyTask);
            }

            return tasks;
        }

        private static IEnumerable<UserSurveyTask> LaunchTargetCampaign(SurveyTargetCampaign campaign, DateTime? taskDueOn, string taskName)
        {
            var tasks = new List<UserSurveyTask>();

            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var survey = campaign.SurveyForCampaign;

            if (survey == null)
                throw new InvalidCampaignException("Survey target campaign must be part of an existing survey.");

            if (!campaign.CampaignTargetTargets.Any())
                throw new InvalidCampaignException("Survey target campaign does not have any targets.");

            if (campaign.CampaignTargetRelationship == null)
                throw new InvalidCampaignException("Survey target campaign does not provide a relationship to survey taker.");

            campaign = campaign.AsWritable<SurveyTargetCampaign>();

            foreach (var target in campaign.CampaignTargetTargets)
            {
                var direction = campaign.CampaignTargetRelationshipDirection_Enum == DirectionEnum_Enumeration.Reverse ? Direction.Reverse : Direction.Forward;

                var recipient = target.GetRelationships<Person>(new EntityRef(campaign.CampaignTargetRelationship), direction).FirstOrDefault();

                if (recipient != null)
                {
                    var result = campaign.CreateSurveyResponse(recipient, target);
                    campaign.SurveyResponses.Add(result);

                    var userSurveyTask = campaign.CreateTaskForResponse(result);

                    if (!string.IsNullOrWhiteSpace(taskName))
                        userSurveyTask.Name = taskName;

                    if (taskDueOn != null)
                        userSurveyTask.UserTaskDueOn = taskDueOn;

                    result.UserSurveyTaskForResults.Add(userSurveyTask);

                    tasks.Add(userSurveyTask);
                }
            }

            return tasks;
        }

        private static SurveyResponse CreateSurveyResponse(SurveyCampaign campaign, Person recipient, UserResource targetObject = null)
        {
            var survey = campaign.SurveyForCampaign;

            if (survey == null)
                throw new InvalidCampaignException("Survey campaign must be part of an existing survey.");

            var result = Entity.Create<SurveyResponse>();

            result.CampaignForResults = campaign;
            result.SurveyTaker = recipient;
            result.SurveyAnswers.AddRange(survey.SurveySections.SelectMany(section => section.SurveyQuestions.Select(q => CreateQuestionResults(section, q))));
            result.SurveyTarget = targetObject;
            result.SurveyStatus_Enum = SurveyStatusEnum_Enumeration.SseNotStarted;
            
            return result;
        }

        private static UserSurveyTask CreateTaskForResponse(SurveyCampaign campaign, SurveyResponse result)
        {
            var task = Entity.Create<UserSurveyTask>();

            task.AssignedToUser = result.SurveyTaker;
            task.UserSurveyTaskSurveyResponse = result;
            task.TaskPriority_Enum = EventEmailPriorityEnum_Enumeration.NormalPriority;
            task.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusNotStarted;

            var target = campaign.CampaignTarget;

            task.UserTaskDueOn = campaign.SurveyClosesOn;
            task.Name = campaign.SurveyForCampaign.Name;
            task.UserSurveyTaskTargetDefinition = target;
            task.UserSurveyTaskAllowTargetEdit = target != null;

            // set help text for the survey based on the description
            if (campaign.SurveyForCampaign.ShowSurveyHelpText == true)
            {
                task.UserSurveyTaskHelp = campaign.SurveyForCampaign.Description;
            }

            return task;
        }
        
        private static SurveyAnswer CreateQuestionResults(SurveySection section, SurveyQuestion question)
        {
            var answer = Entity.Create<SurveyAnswer>();
            answer.SurveyAnswerOriginalQuestionText = question.Name;
            answer.QuestionBeingAnswered = question;
            answer.QuestionInSection = section;
            return answer;
        }
    }
}
