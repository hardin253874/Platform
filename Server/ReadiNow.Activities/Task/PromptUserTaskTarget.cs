// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;

namespace EDC.SoftwarePlatform.Activities.Tasks
{
    public class PromptUserTaskTarget : IEntityEventSave
    {
        private const string UserInputsKey = "USER_INPUTS_KEY";

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            IDictionary<long, object> promptUserInputsDict = new Dictionary<long, object>();

            foreach (var entity in entities)
            {
                IEnumerable<long> changedFields, changedRels, changedRevRels;

                entity.GetChanges(out changedFields, out changedRels, out changedRevRels);

                var task = entity.Cast<PromptUserTask>();

                ////
                // This should never run on first save of the task. The user will have had no opportunity to set the inputs at this point.
                ////
                if (task == null ||
                    task.IsTemporaryId ||
                    task.PromptForTaskArguments == null ||
                    changedRels.All(r => r != PromptUserTask.TaskStatus_Field.Id) ||
                    task.UserTaskIsComplete != true)
                    continue;

                promptUserInputsDict.Add(task.Id, task.PromptForTaskArguments);
            }

			if ( promptUserInputsDict.Count > 0 )
            {
                state[UserInputsKey] = promptUserInputsDict;
            }

            return false;
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var actions = new List<Action>();

            if (state.ContainsKey(UserInputsKey))
            {
                var inputsDict = (IDictionary<long, object>)state[UserInputsKey];

                foreach (var task in from entity in entities where entity.Is<PromptUserTask>() select entity.As<PromptUserTask>())
                {
                    object userInputs = null;

                    if (inputsDict.ContainsKey(task.Id))
                    {
                        userInputs = inputsDict[task.Id];
                    }

                    if (userInputs != null)
                    {
                        // The user inputs themselves, aren't actually required as they get set directly into the variables
                        // of the run, via the relationship.
                        ResumeWorkflowRun(task, actions);
                    }
                }
            }

			if ( actions.Count > 0 )
            {
                WorkflowRunContext.Current.QueueAction(() =>
                {
                    foreach (var action in actions)
                    {
                        action();
                    }
                });
            }
        }

        private void ResumeWorkflowRun(PromptUserTask task, List<Action> actions)
        {
            var workflowRun = task.WorkflowRunForTask;

            if ((task.UserTaskIsComplete ?? false) &&
                (workflowRun != null) &&
                (workflowRun.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused))
            {


                var resumeEvent = new PromptUserTaskCompletedEvent
                {
                    UserTaskId = task.Id
                };

                actions.Add(() =>
                {
 
                        using (new WorkflowRunContext(true) { RunTriggersInCurrentThread = true }) // need to ensure that all deferred saves occur before we register complete.
                        {
                            WorkflowRunner.Instance.ResumeWorkflow(workflowRun, resumeEvent);
                        }

                });
            }
        }
    }
}
