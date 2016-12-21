// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using EDC.Collections.Generic;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Security;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.ReadiNow.BackgroundTasks;

namespace EDC.SoftwarePlatform.Activities.Tasks
{
    /// <summary>
    /// Updates mail boxes on the provider whenever the mail box is saved.
    /// </summary>
    public class DisplayFormTaskTarget : IEntityEventSave
    {
        /// <summary>
        ///     User responses key.
        /// </summary>
        private const string UserResponseKey = "USER_RESPONSE_KEY";
        
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            IDictionary<DisplayFormUserTask, TransitionStart> displayFormUserTaskDict = new Dictionary<DisplayFormUserTask, TransitionStart>();
            
            ////
            // Loop through the entities and obtain their modifications prior to save.
            // Filter so that only User Responses are acted on.
            ////
            var userReponseId = DisplayFormUserTask.UserResponse_Field.Id;

            foreach (IEntity entity in entities)
            {
                IEnumerable<long> changedFields, changedRels, changedRevRels;

                entity.GetChanges(out changedFields, out changedRels, out changedRevRels);

                var task = entity.Cast<DisplayFormUserTask>();

                if (task.UserResponse != null && changedRels.Any(id => id == userReponseId ))
                {
                    displayFormUserTaskDict.Add(task, task.UserResponse);
                }
            }

            ////
            // This should never run on first save of the task. The guard is that user response will be null
            // in this case, so no need to worry about temporary id mappings.
            ////
			if ( displayFormUserTaskDict.Count > 0 )
            {
                state[UserResponseKey] = displayFormUserTaskDict;
            }
            
            return false;
        }
        
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            using (new WorkflowRunContext())
            {
                var actions = new List<Action>();

                if (state.ContainsKey(UserResponseKey))
                {
                    var tasksDict = (IDictionary<DisplayFormUserTask, TransitionStart>)state[UserResponseKey];

                    foreach (var kvp in tasksDict)
                    {
                        var task = kvp.Key.Cast<DisplayFormUserTask>();
                        var response = kvp.Value;
                        ActOnTask(task, response, actions);
                    }
                }
            }
        }


        private void ActOnTask(DisplayFormUserTask task, TransitionStart userResponse, List<Action> actions)
        {
            //TODO: Serialize the workflow runs like the trigger on save.
            var workflowRun = task.WorkflowRunForTask;
            if ((task.UserTaskIsComplete ?? false) && (workflowRun != null) && (workflowRun.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused))
            {

                var resumeEvent = new UserCompletesTaskEvent
                {
                    CompletionStateId = userResponse.Id,
                    UserTaskId = task.Id
                };

                var runId = workflowRun.Id;
                var runInThread = WorkflowRunContext.Current.RunTriggersInCurrentThread;
                Action act;

                if (Factory.FeatureSwitch.Get("longRunningWorkflow"))
                {
                    act = () => ActionOnTask_new(runId, resumeEvent, runInThread);
                }
                else
                {
                    act = () => ActionOnTask_old(runId, resumeEvent, runInThread);
                }

                WorkflowRunContext.Current.DeferAction(act);
            }
        }

        void ActionOnTask_new(long runId, UserCompletesTaskEvent resumeEvent, bool runInThread)
        {
            BackgroundTask bgTask;

            using (new SecurityBypassContext())
            {
                var run = Entity.Get<WorkflowRun>(runId);                       // If we don't do this we get caching problems
                var wf = run.WorkflowBeingRun;

                bgTask = ResumeWorkflowHandler.CreateBackgroundTask(run, resumeEvent);
            }

            if (runInThread)
            {
                Factory.BackgroundTaskManager.ExecuteImmediately(bgTask);
            }
            else
            {
                Factory.BackgroundTaskManager.EnqueueTask(bgTask);
            }
        }

        void ActionOnTask_old(long runId, UserCompletesTaskEvent resumeEvent, bool runInThread)
        {
            Action act = () =>
            {
                using (new SecurityBypassContext())
                {
                    var run = Entity.Get<WorkflowRun>(runId);                       // If we don't do this we get caching problems


                    using (new WorkflowRunContext(true) { RunTriggersInCurrentThread = true })
                    {
                        WorkflowRunner.Instance.ResumeWorkflow(run, resumeEvent);
                    }
                }
            };
            
            if (runInThread)
            {
                act();
            }
            else
            {
                WorkflowRunContext.Current.QueueAction(act);
            }
        }

    }
}