// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    public class WorkflowRunHelperTest
    {
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void CancelRun()
        {
            var run = new WorkflowRun() { WorkflowRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunStarted, TaskId = Guid.NewGuid().ToString() };

            run.Save();

            Factory.WorkflowRunTaskManager.RegisterStart(run.TaskId);

            WorkflowRunHelper.CancelRun(run.Id);

            var run2 = Entity.Get<WorkflowRun>(run.Id);

            Assert.That(run2.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCancelled));

            Assert.That(DateTime.UtcNow - run2.RunCompletedAt, Is.LessThan(TimeSpan.FromMinutes(1)));

            Assert.That(Factory.WorkflowRunTaskManager.HasCompleted(run.TaskId), Is.True);
                
        }
    }
}
