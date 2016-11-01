// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Model.PartialClasses;

namespace EDC.SoftwarePlatform.Activities
{

    public class ReviewSurveyImplementation : ResumableActivityImplementationBase
    {
        private const string timeoutAlias = "core:reviewSurveyTimeout";
        private const string DefaultTitle = "Form to complete";
        private const decimal DefaultTimeOutMins = 0;     // no time out

        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var surveyResponse = GetArgumentEntity<SurveyResponse>(inputs, "inReviewSurveyResponse");
            var reviewer = GetArgumentEntity<Person>(inputs, "inReviewSurveyReviewer");
            var entryState = GetArgumentEntity<SurveyStatusEnum>(inputs, "inReviewSurveyEntryState");
            var dueInDays = GetArgumentValue<decimal>(inputs, "inReviewSurveyDueInDays", DefaultTimeOutMins);
            var allowComments = GetArgumentValueStruct<bool>(inputs, "inReviewSurveyAllowComments");
            var taskName = GetArgumentValue<string>(inputs, "inReviewSurveyTaskName");


            var task = ReviewSurvey(surveyResponse, reviewer, entryState, dueInDays, allowComments, taskName);

            task.AvailableTransitions.AddRange(GetAvailableUserTransitions());

            context.SetUserTask(task.Cast<BaseUserTask>()); 
            //task.NotifyUser();

            SetTimeoutIfNeeded(context, dueInDays);

            // Note that there is no need to set an exit point if we are not pausing as the default exit point will be used.

            return false;
        }


        /// <summary>
        /// Create the results and task for a survey
        /// </summary>
        /// <param name="survey">The survey</param>
        /// <param name="recipient">The recipient</param>
        /// <param name="dueInDays">Task due, zero indicates no due date</param>
        /// <param name="taskName">If not null or empty, use as task name</param>
        /// <returns></returns>
        public static UserSurveyTask ReviewSurvey(SurveyResponse surveyResponse, Person reviewer, SurveyStatusEnum entryState, decimal dueInDays, bool allowComments, string taskName)
        {
            if (reviewer == null)
                throw new ArgumentException("reviewer");

            var task = Entity.Create<UserSurveyTask>();

            task.AssignedToUser = reviewer;
            task.UserSurveyTaskSurveyResponse = surveyResponse;
            task.UserSurveyTaskAllowComments = allowComments;
            task.UserSurveyTaskForReview = true;
            task.UserSurveyTaskAllowTargetEdit = false;

            if (dueInDays > 0)
                task.UserTaskDueOn = DateTime.UtcNow.AddDays(Decimal.ToDouble(dueInDays));

            // use the task name or fall back to the survey name. (Note we always assume the survey exits.
            if (!String.IsNullOrWhiteSpace(taskName))
                task.Name = taskName;
            else
            {
                if (surveyResponse.CampaignForResults.SurveyForCampaign == null)
                    throw new ArgumentException("Unable to find a survey to name the task");

                task.Name = surveyResponse.CampaignForResults.SurveyForCampaign.Name;
            }

            if (task.Name == null)
                throw new ArgumentException("Expected to have a task name or the name of a survey");

            if (entryState != null)
            {
                var writableResult = surveyResponse.AsWritable<SurveyResponse>();
                writableResult.SurveyStatus = entryState;
                writableResult.Save();
            }

            return task;
        }




        public override bool OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            if (!HandleTimeout(context, resumeEvent))      // handle timeout will set the exit point
            {
                HandleResumeTransition(context, (IWorkflowUserTransitionEvent)resumeEvent);

                //if (!(userTask.HideComment ?? false))
                //    context.SetArgValue(ActivityInstance, GetArgumentKey("core:outTaskComment"), userTask.TaskComment);
            }

            return true;
        }
    }
}
