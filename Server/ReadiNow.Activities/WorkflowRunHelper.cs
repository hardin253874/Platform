// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics.Response;
using System.Diagnostics;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities
{
    public static class WorkflowRunHelper
    {

	    public static Dictionary<string, object> GetOutput(this WorkflowRun run)
        {
            var outputArgIds = run.WorkflowBeingRun.OutputArguments.Select(a => a.Id);

            var outputInfo = run.StateInfo.Where(si => outputArgIds.Contains(si.StateInfoArgument.Id));

            var result = new Dictionary<string, object>();

            foreach (var si in outputInfo)
            {
                var outputArg = si.StateInfoArgument;
                var valueArg = si.StateInfoValue;
                var value = ActivityArgumentHelper.GetArgParameterValue(valueArg);

                result.Add(outputArg.Name, value);
            }

            return result;
        }

        public static ExitPoint GetExitPoint(this WorkflowRun run)
        {
            return run.WorkflowRunExitPoint;
        }


        /// <summary>
        /// Move the workflows runs between two workflows runs between the clones and the originals, fixing up references as required.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="clone"></param>
        /// <param name="clonedIds"></param>
        public static void MoveRuns(Workflow original, Workflow clone, IDictionary<long, long> clonedIds)
        {
            // WorkflowRuns are "special". (#25246)
            // Detaching them from the original workflow somehow excludes them from our security view. This
            // results in them being completely orphaned after this method completes its outer transaction.
            // Putting the security bypass in place until its clear why this is or until they are re-examined.
            using (new ReadiNow.Security.SecurityBypassContext(true))
            {
                // move the running instances across to the clone, making sure that the
                // exit points and pending activities all point to the clone.

                var writeableOriginal = original.AsWritable<Workflow>();
                var writeableClone = clone.AsWritable<Workflow>();

                var runs = original.RunningInstances;


                writeableOriginal.RunningInstances.Clear();
                writeableOriginal.Save();

                writeableClone = writeableClone.AsWritable<Workflow>();
                writeableClone.RunningInstances.AddRange(runs);
                writeableClone.Save();


                foreach (var ri in writeableClone.RunningInstances)
                {
                    MoveRunningInstance(clonedIds, ri);
                }
            }
        }

        /// <summary>
        /// Move the running instance onto the clone.
        /// </summary>
        private static void MoveRunningInstance(IDictionary<long, long> clonedIds, WorkflowRun ri)
        {
            var writableRun = ri.AsWritable<WorkflowRun>();

            if (writableRun.PendingActivity != null)
                writableRun.PendingActivity = Entity.Get<WfActivity>(clonedIds[writableRun.PendingActivity.Id]);


            foreach (var si in writableRun.StateInfo)
            {
                var siWritable = si.AsWritable<StateInfoEntry>();

                if (si.StateInfoActivity != null)
                    siWritable.StateInfoActivity = Entity.Get<WfActivity>(clonedIds[si.StateInfoActivity.Id]);

                if (si.StateInfoArgument != null)
                {
                    var id = si.StateInfoArgument.Id;
                    if (clonedIds.ContainsKey(id))                  // if the argument is not part of the cloned entity it does not need to be redirected
                    {
                        siWritable.StateInfoArgument = Entity.Get<ActivityArgument>(clonedIds[id]);
                    }
                }
                siWritable.Save();
            }

            writableRun.Save();

            // update any tasks with references
            foreach (var task in writableRun.TaskWithinWorkflowRun)
            {
                ReassignTask(clonedIds, task);
            }
        }

        /// <summary>
        /// Reassign a task to point to the cloned workflow
        /// </summary>
        private static void ReassignTask(IDictionary<long, long> clonedIds, BaseUserTask task)
        {
            var displayFormTask = task.AsWritable<DisplayFormUserTask>();

            // The available transitions will have been cloned so remove the references to the old workfow
            if (displayFormTask != null)
            {
                var oldTransitions = displayFormTask.AvailableTransitions.Where(t => clonedIds.ContainsKey(t.Id));
                displayFormTask.AvailableTransitions.RemoveRange(oldTransitions);

                if (displayFormTask.UserResponse != null)
                    displayFormTask.UserResponse = Entity.Get<TransitionStart>(clonedIds[displayFormTask.UserResponse.Id]);

                displayFormTask.Save();
                return;
            }

            var promptUserTask = task.AsWritable<PromptUserTask>();

            // The arguments that the old task pointed to will have been cloned. Reposition them.
            if (promptUserTask != null)
            {
                var oldArguments = promptUserTask.PromptForTaskArguments.Where(a => clonedIds.ContainsKey(a.Id));
                promptUserTask.PromptForTaskArguments.RemoveRange(oldArguments);
                promptUserTask.Save();
            }
        }

        /// <summary>
        /// Cancel a running workflow
        /// </summary>
        /// <param name="runId"></param>
        public static void CancelRun(long runId)
        {
            var run = Entity.Get<WorkflowRun>(runId, true, WorkflowRun.TaskId_Field, WorkflowRun.WorkflowRunStatus_Field);

            if (run == null)
                throw new MissingRunException();

            if (run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCancelled ||
                run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted ||
                run.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunFailed)
                return;

            // Cancel the run
            run.WorkflowRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunCancelled;
            run.RunCompletedAt = DateTime.UtcNow;
            run.Save();

            // mark it as complete
            Factory.WorkflowRunTaskManager.RegisterCancelled(run.TaskId);

            // Update the diagnostics info
            var response = new WorkflowResponse
            {
                TenantName = RequestContext.GetContext().Tenant.Name,
                Id = run.Id,
                TaskId = run.TaskId,
                WorkflowName = run?.WorkflowBeingRun?.Name,
                WorkflowRunName = run?.Name,
                Status = WorkflowRunState_Enumeration.WorkflowRunCancelled.ToString(),
                Date = DateTime.Now,
                TriggeredBy = run.TriggeringUser != null ? run.TriggeringUser.Name : "Unknown",
                Server = Environment.MachineName,
                Process = Process.GetCurrentProcess().MainModule.ModuleName,
                StepCount = run.RunStepCounter ?? -1
            };

            ReadiNow.Diagnostics.DiagnosticChannel.Publish(response);
        }


        /// <summary>
        /// Resume any suspended runs, putting them back on the queue.
        /// Note that this does not deal with the queue already containing suspended runs.
        /// </summary>
        public static void ResumeSuspendedRuns()
        {
            var suspendedIds = Entity.GetCalculationMatchesAsIds("Status='Long running'", WorkflowRun.WorkflowRun_Type, false);

            if (suspendedIds.Any())
            {
                var runs = Entity.Get<WorkflowRun>(suspendedIds);

                foreach (var run in runs)
                {
                    var restoreTask = ResumeWorkflowHandler.CreateBackgroundTask(run, new WorkflowRestoreEvent());
                    Factory.BackgroundTaskManager.EnqueueTask(restoreTask);
                }
            }
        }


        /// <summary>
        /// Thrown when a run could not be found
        /// </summary>
        public class MissingRunException: ArgumentException
        {
        }
    }
}
