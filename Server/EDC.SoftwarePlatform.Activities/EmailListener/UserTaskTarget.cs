// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EDC.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;

namespace EDC.SoftwarePlatform.Activities.EmailListener
{
    /// <summary>
    /// Updates mail boxes on the provider whenever the mail box is saved.
    /// </summary>
    public class UserTaskTarget : IEntityEventSave
    {
        private const string CompletedTasksKey = "UserTaskTarget.TriggerWorkflows";

        private static readonly IEnumerable<EntityRef> changesWeAreinteresteIn =
            new List<EntityRef>() {new EntityRef("core", "userTaskIsComplete")};



        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var completedTasks = new List<BaseUserTask>();
            state.Add(CompletedTasksKey, completedTasks);
            foreach (var entity in entities)
            {

                var task = entity.As<BaseUserTask>();

                if (task != null && task.HasChanges(changesWeAreinteresteIn))
                {
                    if (task.UserTaskIsComplete ?? false)
                    {
                        task.UserTaskCompletedOn = DateTime.UtcNow; // ignore whatever value was provided by the UI
                        completedTasks.Add(task);
                    }
                }
            }

            return false;
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var tasks = (List<BaseUserTask>)state[CompletedTasksKey];

            var actions = new List<Action>();
            var invoker = WorkflowInvoker.DefaultInvokerForThread;

            foreach (var task in tasks)
            {
                //TODO: Serialize the workflow runs like the trigger on save.
                if (task.WorkflowRunForTask != null &&
                    task.WorkflowRunForTask.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused)
                {
                    var resumeEvent = new UserCompletesTaskEvent() {CompletionState = task.UserResponse, UserTaskRef = task.Id };
                    var run = task.WorkflowRunForTask;
                    actions.Add(() => WorkflowRunHelper.ResumeWorkflow(run, resumeEvent, invoker));
                }
            }

            RunActionsInThread(actions);


        }

        private static void RunActionsInThread(List<Action> actions)
        {
            if (actions.Any())
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    foreach (var action in actions)
                    {
                        action();
                    }
                });
            }
        }
    }
}