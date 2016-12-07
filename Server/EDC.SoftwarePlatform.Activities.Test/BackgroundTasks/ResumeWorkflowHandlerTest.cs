// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using NUnit.Framework;
using System.Linq;
using FluentAssertions;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities.Test.BackgroundTasks
{
    [TestFixture]
    public class ResumeWorkflowHandlerTest
    {
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void SuspendRestore()
        {
            using (new SecurityBypassContext())
            {
                var handler = new ResumeWorkflowHandler();

                var run = new WorkflowRun() { WorkflowRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunSuspended };
                run.Save();

                var task = ResumeWorkflowHandler.CreateBackgroundTask(run, new WorkflowRestoreEvent());

                var taskEntity = handler.CreateSuspendedTask(task).As<SuspendedRun>();
                taskEntity.Save();

                taskEntity.Should().NotBeNull();
                taskEntity.SrRun.Should().NotBeNull();
                taskEntity.SrRun.Id.Should().Be(run.Id);

                var bgTasks = handler.RestoreSuspendedTasks();

                bgTasks.Should().HaveCount(1);

                var runParam = bgTasks.First().GetData<ResumeWorkflowParams>();

                runParam.WorkflowRunId.ShouldBeEquivalentTo(run.Id);
                Assert.That(runParam.WorkflowRunId, Is.EqualTo(run.Id));
            }
        }

               
    }
}
