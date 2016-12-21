// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.Activities.Engine.Events;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Core;
using EDC.SoftwarePlatform.Activities.Notify;
using EDC.SoftwarePlatform.Activities.Approval;
using EDC.Security;

namespace EDC.SoftwarePlatform.Activities
{

    public class DisplayFormImplementation : ResumableActivityImplementationBase
    {
        private const string timeoutAlias = "core:displayFormTimeout";
        private const string DefaultTitle = "Form to complete";
        private const decimal DefaultTimeOutMins = 0;     // no time out
      
        public override bool OnStart(IRunState context, ActivityInputs inputs)
        {
            var assignedTo = GetArgumentEntity<Person>(inputs, "inDisplayFormForUser");
            var recordToPresent = GetArgumentEntity<UserResource>(inputs, "inDisplayFormResource");
            var form = GetArgumentEntity<CustomEditForm>(inputs, "inDisplayFormForm");
            var timeoutDays = GetArgumentValue<decimal>(inputs, "inDisplayFormTimeOut", DefaultTimeOutMins);
            var priority = GetArgumentEntity<EventEmailPriorityEnum>(inputs, "inDisplayFormPriority");
            var activityInstanceAs = ActivityInstance.Cast<DisplayFormActivity>();
            var percentageCompleted = GetArgumentValue<decimal?>(inputs, "inDisplayFormPercentageCompleted", null);
            var waitForNext = GetArgumentValue<bool>(inputs, "inDisplayFormWaitForNext", false);
            var recordHistory = GetArgumentValue<bool>(inputs, "inDisplayFormRecordHistory", false);
            var hideComment = GetArgumentValue<bool>(inputs, "inHideComment", false);
            var openInEditMode = GetArgumentValue<bool>(inputs, "inOpenInEditMode", false);

            priority = priority ?? Entity.Get<EventEmailPriorityEnum>(new EntityRef("core", "normalPriority"));
            
            var workflowRun = context.WorkflowRun;
            

            var dueDate = DateTime.UtcNow.AddDays((double) timeoutDays);

            var userTask = new DisplayFormUserTask
            {
                Name = ActivityInstance.Name ?? DefaultTitle,
                RecordToPresent = recordToPresent,
                FormToUse = form,
                AvailableTransitions = GetAvailableUserTransitions(),
                AssignedToUser = assignedTo,
                TaskPriority = priority,
                TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusNotStarted,
                PercentageCompleted = percentageCompleted,
                WaitForNextTask = waitForNext,
                UserTaskDueOn = dueDate,
                HideComment = hideComment,
                OpenInEditMode = openInEditMode,
                DfutLinkToken = CryptoHelper.GetRandomPrintableString(8)
            };

            context.SetUserTask(userTask.Cast<BaseUserTask>());

            if (recordHistory)
            {
                CreateLogEntry(context, userTask);
            }

            SetTimeoutIfNeeded(context, timeoutDays);

            var tenantSetting = Entity.Get<TenantGeneralSettings>(WellKnownAliases.CurrentTenant.TenantGeneralSettingsInstance);

            if (Factory.FeatureSwitch.Get("enableWfUserActionNotify"))
            {
                // 
                // IN PROGRESS - Please leave
                // This code is in development and switched off until email and SMS approvals are required by PM.
                //
                Notifier notifier = null; // tenantSetting.UserActionNotifier;
                if (notifier != null)
                {
                    // TODO: Format correctly for where it is being sent SMS email etc. Move the decision out of here and make the notfier decide on the type of message
                    var generator = new HtmlGenerator();

                    string message = null;
                    if (notifier.Is<EmailNotifier>())           // TODO: This is wrong, it should be somehow tied to the Router
                    {
                        var transitionOptions = userTask.AvailableTransitions.Select(t => t.FromExitPoint.Name).Where(n => !String.IsNullOrEmpty(n));

                        if (transitionOptions.Any())
                            message = generator.GenerateSelectionPage(userTask);
                    }
                    else if (notifier.Is<TwilioNotifier>())
                    {
                        message = generator.GenerateSelectionPageUrl(userTask.DfutLinkToken);
                    }

                    if (message != null)
                    {
                        var userList = userTask.AssignedToUser.ToEnumerable();

                        var notification = new Notification { NMessage = message };

                        //TOOD: Add alternative text in the email with a link to the SMS page
                        NotificationRouter.Instance.Send(notifier, notification, userList, false);
                    }
                }
            }

            context.SetArgValue(ActivityInstance, GetArgumentKey("core:outDisplayFormUserTask"), userTask);
            context.SetArgValue(ActivityInstance, GetArgumentKey("core:dfaInternalKeepHistory"), recordHistory);

            return false;
        }


        public override bool OnResume(IRunState context, IWorkflowEvent resumeEvent)
        {
            var userTaskEntity = context.GetArgValue<IEntity>(ActivityInstance, GetArgumentKey("core:outDisplayFormUserTask"));
            var keepHistory = context.GetArgValue<bool>(ActivityInstance, GetArgumentKey("core:dfaInternalKeepHistory"));

            var userTask = userTaskEntity != null ? userTaskEntity.As<DisplayFormUserTask>() : null;        // We could have a combination of a deleted user task and a time-out


            if (!HandleTimeout(context, resumeEvent) )      // handle timeout will set the exit point
            {
                HandleResumeTransition(context, (IWorkflowUserTransitionEvent) resumeEvent);

                if (!(userTask.HideComment ?? false))
                    context.SetArgValue(ActivityInstance, GetArgumentKey("core:outTaskComment"), userTask.TaskComment);

                if (keepHistory)
                {
                    var writableTask = userTask.AsWritable<DisplayFormUserTask>();
                    writableTask.TaskStatus_Enum = TaskStatusEnum_Enumeration.TaskStatusCompleted;
                    writableTask.Save();
                }
            }

            if (userTask != null)
            {
                if (keepHistory)
                {
                    if (userTask.LogEntryForUserAction != null)
                    {
                        UpdateLogEntry(userTask); 
                    }
                }
                else
                {
                    context.SetArgValue(ActivityInstance, GetArgumentKey("core:outDisplayFormUserTask"), null);
                    Entity.Delete(userTask);
                }
            }

            return true;
        }

        void CreateLogEntry(IRunState context, DisplayFormUserTask userTask)
        {
            var actingPersonName = userTask.AssignedToUser != null ? userTask.AssignedToUser.Name : null;
            // Create the log entry
            context.Log(new WorkflowUserActionLogEntry
            {
                Name = ActivityInstance.Name,
                LogEventTime = DateTime.UtcNow,
                UserActionBeingLogged = userTask,
                WuleDueDate = userTask.UserTaskDueOn,
                ActingPersonReferencedInLog = userTask.AssignedToUser,
                ActingPersonName = actingPersonName,
                ObjectReferencedInLog = userTask.RecordToPresent,
                Description = string.Format("Assigned to: {0}, Due: {1}", actingPersonName ?? "", userTask.UserTaskDueOn)
            });
        }

        void UpdateLogEntry(DisplayFormUserTask userTask)
        {
            var logEntry = userTask.LogEntryForUserAction;

            var exitPoint = userTask.UserResponse.FromExitPoint;
            var actionSummary = !string.IsNullOrWhiteSpace(exitPoint.ExitPointActionSummary) ? exitPoint.ExitPointActionSummary: exitPoint.Name;

            var writable = logEntry.AsWritable<WorkflowUserActionLogEntry>();
            writable.WuleCompletedDate = userTask.UserTaskCompletedOn;
            writable.WuleUserComment = userTask.TaskComment;
            writable.WuleActionSummary = actionSummary;
            writable.Description += string.Format(", Completed: {0}, Action: {1}", userTask.UserTaskCompletedOn, actionSummary);

            writable.Save();
        }
    }
}
