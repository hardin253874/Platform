// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.BackgroundTasks;
using ProtoBuf;

namespace EDC.ReadiNow.Model.EventClasses
{

    /// <summary>
    ///     EventEmail event target.
    /// </summary>
    public class UserSurveyTaskEventTarget : IEntityEventSave
    {
        const string reviewTaskName = "Review: {0} : {1}";

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        bool IEntityEventSave.OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }

        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void IEntityEventSave.OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // We can't delete the tasks earlier just incase there are other hooks that will run. 
            var toDelete = new List<long>();
            var campaignsToCheck = new List<SurveyCampaign>();

            foreach (var entity in entities)
            {
                var task = entity.As<UserSurveyTask>();
                if (task.UserTaskIsComplete ?? false)
                {
                    UpdateAnswersInTask(task);

                    toDelete.Add(task.Id);

                    var surveyResponses = task.UserSurveyTaskSurveyResponse;
                    
                    if (surveyResponses != null)
                    {
                        var campaign = surveyResponses.CampaignForResults;
                        var survey = default(UserSurvey);

                        if (campaign != null)
                        {
                            campaignsToCheck.Add(campaign);
                            survey = campaign.SurveyForCampaign;
                        }

                        if (survey != null && survey.SurveyTriggerOnSurveyComplete != null)
                        {
                            TriggerSurveyCompleteWf(survey.SecurityOwner, survey.SurveyTriggerOnSurveyComplete, surveyResponses);
                        }

                        // Check if a workflow needs to be resumed
                        if (task.WorkflowRunForTask != null)
                        {
                            ResumeWf(task);
                        }
                    }
                }
            }

            foreach (var campaign in campaignsToCheck.Distinct<SurveyCampaign>(new EntityEqualityComparer()))
                UpdateCampaignIsComplete(campaign);

            if (toDelete.Count > 0)
                Entity.Delete(toDelete);

        }

        void UpdateAnswersInTask(UserSurveyTask task)
        {
            var toSave = new List<IEntity>();

            foreach (var answer in task.UserSurveyTaskSurveyResponse.SurveyAnswers)
            {
                var writableAnswer = answer.AsWritable<SurveyAnswer>();
                writableAnswer.SurveyAnswerCalculatedValue = answer.CalcAdjustedValue();
                writableAnswer.SurveyAnswerSummary = answer.GenerateTextSummary();
                toSave.Add(writableAnswer);
            }

            var writableResults = task.UserSurveyTaskSurveyResponse.AsWritable<SurveyResponse>();
            writableResults.SurveyCompletedOn = DateTime.UtcNow;
                                    
            UpdateSurveyResponses(writableResults);

            toSave.Add(writableResults);

            Entity.Save(toSave);
        }

        /// <summary>
        /// Update the campaigns completness based upon the results it has received.
        /// </summary>
        /// <param name="campaign"></param>
        void UpdateCampaignIsComplete(SurveyCampaign campaign)
        {
            if (!(campaign.CampaignIsComplete ?? false))
            {
                if (campaign.SurveyResponses.All(results => results.SurveyCompletedOn != null))
                {
                    var writableCampaign = campaign.AsWritable<SurveyCampaign>();
                    writableCampaign.CampaignIsComplete = true;
                    writableCampaign.Save();

                    var survey = campaign.SurveyForCampaign;
                    var completeWf = survey.SurveyTriggerOnCampaignComplete;
                    if (completeWf != null)
                    {
                        TriggerCampaignCompleteWf(survey.SecurityOwner, completeWf, campaign);
                    }
                }
            }
        }



        void UpdateSurveyResponses(SurveyResponse result)
        {
            if (result.UserSurveyTaskForResults == null)
                throw new ArgumentException("No survey for results");

            if (result.CampaignForResults != null)      // individual results are not accumulated
            {
                result.UpdateWeightedValueAndOutcomes();
            }
        }

        /// <summary>
        /// Create a task used to review someone elses answers.
        /// </summary>
        /// <param name="survey"></param>
        /// <param name="reviewer"></param>
        /// <param name="result"></param>
        UserSurveyTask CreateReviewTask( UserSurvey survey, Person reviewer, SurveyResponse result)
        {
            var task = Entity.Create<UserSurveyTask>();
            task.UserSurveyTaskForReview = true;
            task.AssignedToUser = reviewer;
            task.UserSurveyTaskSurveyResponse = result;
            task.UserTaskDueOn = null;                      // The review can be completed after the campaign finshes
            task.Name = string.Format(reviewTaskName, result.SurveyTaker.Name ?? "[Unnamed]", survey.Name ?? "[Unnamed]");

            return task;
        }

        void TriggerSurveyCompleteWf(UserAccount surveyOwner, Workflow wf, SurveyResponse result)
        {
            TriggerWf(surveyOwner, wf, result);
        }


        void TriggerCampaignCompleteWf(UserAccount surveyOwner, Workflow wf, SurveyCampaign campaign)
        {
            TriggerWf(surveyOwner, wf, campaign);
        }

        void TriggerWf(UserAccount triggeringAccount, Workflow wf, object inputObject)
        {
            if (triggeringAccount == null)
                throw new ArgumentException("triggeringAccount");

            var runner = Factory.Current.Resolve<IWorkflowRunner>();

            var args = new Dictionary<string, object>();
            args.Add("Input", inputObject);

            runner.RunWorkflowAsync(new WorkflowStartEvent(wf) { Arguments = args });
        }

        /// <summary>
        /// Resume a workflow that was started with a Survey Task
        /// </summary>
        /// <param name="task"></param>
        void ResumeWf(UserSurveyTask task)
        {
            var runner = Factory.Current.Resolve<IWorkflowRunner>();

            var resumeEvent = new ResumeSurveyEvent { CompletionStateId = task.UserResponse?.Id ?? 0 };

            runner.ResumeWorkflowAsync(task.WorkflowRunForTask, resumeEvent);
        }

        [ProtoContract]
        public class ResumeSurveyEvent : IWorkflowEvent, IWorkflowUserTransitionEvent, IWorkflowQueuedEvent
        {
            [ProtoMember(1)]
            public long CompletionStateId { get; set; }
          
            
        }
    }
}