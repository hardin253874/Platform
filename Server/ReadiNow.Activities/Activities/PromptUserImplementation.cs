// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.Model.Interfaces;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Activity that will prompt the user to fill out any missing values of the configured variables.
    /// </summary>
    public class PromptUserImplementation : ResumableActivityImplementationBase
    {
        /// <summary>
        /// Start the activity running
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused. Along with a sequence number of if it is paused</returns>
        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var assignedTo = GetArgumentEntity<Person>(inputs, "inPromptUserForPerson");
            var promptMessage = GetArgumentValue<string>(inputs, "inPromptUserMessage");
            var activityInstanceAs = ActivityInstance.Cast<PromptUserActivity>();

            var userTask = new PromptUserTask
            {
                Name = ActivityInstance.Name ?? "User input required",
                AssignedToUser = assignedTo,
                TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusNotStarted,
                UserTaskDueOn = DateTime.UtcNow, // dueDate
                KeepAfterCompletion = false, // keepTask
                WaitForNextTask = true, // waitForNext
                PromptForTaskMessage = promptMessage,
                PromptForTaskArguments = new EntityCollection<ActivityPrompt>(activityInstanceAs.PromptForArguments)
            };

            // move current state information on to the task for pre-population of any arguments for prompt
            var argValues = context.GetArgValues().ToList();
            foreach (var arg in activityInstanceAs.PromptForArguments)
            {
                var argValue = argValues.FirstOrDefault(a => a.Item2.Id == arg.ActivityPromptArgument.Id);
                if (argValue != null)
                {
                    var valueArg = ActivityArgumentHelper.ConvertArgInstValue(argValue.Item1, argValue.Item2, argValue.Item3);
                    userTask.PromptForTaskStateInfo.Add(new StateInfoEntry
                    {
                        Name = valueArg.Name,
                        StateInfoActivity = ActivityInstance,
                        StateInfoArgument = argValue.Item2,
                        StateInfoValue = valueArg
                    });
                    
                    continue;
                }

                var emptyArg = ActivityArgumentHelper.ConvertArgInstValue(ActivityInstance, arg.ActivityPromptArgument, null);
                userTask.PromptForTaskStateInfo.Add(new StateInfoEntry
                {
                    Name = emptyArg.Name,
                    StateInfoActivity = ActivityInstance,
                    StateInfoArgument = arg.ActivityPromptArgument,
                    StateInfoValue = emptyArg
                });
            }
            
            context.SetUserTask(userTask.Cast<BaseUserTask>());

            return false;
        }

        /// <summary>
        /// Continue a paused activity
        /// </summary>
        /// <returns>True if the activity has completed, false if it is paused.</returns>
        public override bool OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            var userCompletesEvent = resumeEvent as PromptUserTaskCompletedEvent;

            if (userCompletesEvent != null)
            {
                context.ExitPointId = new EntityRef("promptUserCompleted");

                var userTaskId = userCompletesEvent.UserTaskId;

                var userTask = Entity.Get<PromptUserTask>(userTaskId, PromptUserTask.PromptForTaskStateInfo_Field);
                if (userTask != null)
                {
                    // load the state information in the task back into the workflow run context
                    foreach (var arg in userTask.PromptForTaskArguments)
                    {
                        var input = userTask.PromptForTaskStateInfo.FirstOrDefault(si => si.StateInfoArgument.Id == arg.ActivityPromptArgument.Id);
                        if (input == null)
                            continue;

                        context.SetArgValue(input.StateInfoArgument, ActivityArgumentHelper.GetArgParameterValue(input.StateInfoValue));
                    }

                    Entity.Delete(userTask);
                }
            }

            return true;
        }
    }
}
