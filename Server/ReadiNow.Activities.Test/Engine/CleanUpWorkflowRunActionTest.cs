// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System.Linq;
using EDC.SoftwarePlatform.Activities.Engine;
using System;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
	[TestFixture]
    public class CleanUpWorkflowRunActionTest
	{
		[Test]
        //[RunWithTransaction]
		[RunAsDefaultTenant]
		public void EnsureCleanupRuns( )
		{
            var action = new CleanUpWorkflowRunsAction();

            action.Execute(null);
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunCompleted, 6, true)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunCompleted, 1, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunFailed, 6, true)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunFailed, 1, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunPaused, 6, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunPaused, 1, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunStarted, 6, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunStarted, 1, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunQueued, 6, false)]
        [TestCase(WorkflowRunState_Enumeration.WorkflowRunQueued, 1, false)]
        public void IsRunCleanedUp(WorkflowRunState_Enumeration state, int age, bool isCleaned)
        {
            var wf = new Workflow();
            var run = CreateRun(wf, DateTime.Now.AddDays(- age).ToUniversalTime(), state);
            wf.Save();

            (new CleanUpWorkflowRunsAction()).RemoveOldWorkflowRuns(DateTime.Now.AddDays(-5));

            var run2 = Entity.Get(run.Id);

            if (isCleaned)
                Assert.IsNull(run2);
            else
                Assert.IsNotNull(run2);
        }


        WorkflowRun CreateRun(Workflow wf, DateTime completedDate, WorkflowRunState_Enumeration status)
        {
            var runOld = Entity.Create<WorkflowRun>();
            runOld.RunCompletedAt = completedDate;
            runOld.WorkflowRunStatus_Enum = status;
            wf.RunningInstances.Add(runOld);

            return runOld;
        }




        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void SingleWf()
        {
            TestIt(new Workflow(), false);
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void WfWithRun()
        {
            var wf = new Workflow();
            var run = CreateRun(wf, DateTime.UtcNow, WorkflowRunState_Enumeration.WorkflowRunPaused);
            TestIt(wf, false);
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void SuperceededWfNoRun()
        {
            var wf1 = new Workflow();
            var wf2 = new Workflow();
            wf2.WfNewerVersion = wf1;

            TestIt(wf2, true);
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void SuperceededWfRun()
        {
            var wf1 = new Workflow();
            var wf2 = new Workflow();
            wf2.WfNewerVersion = wf1;
            CreateRun(wf2, DateTime.UtcNow, WorkflowRunState_Enumeration.WorkflowRunPaused);

            TestIt(wf2, false);
        }

        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        public void CleanTraceLogsWithoutRuns()
        {
            var log = Entity.Create<RunTraceLogEntry>();
            log.Save();

            var id = log.Id;

            Assert.That(Entity.Get(id), Is.Not.Null);

            (new CleanUpWorkflowRunsAction()).RemoveUnreferencedWorkflowTraces();

            Assert.That(Entity.Get(id), Is.Null);

        }

        void TestIt(Workflow wf, bool isCleaned)
        {
            wf.Save();


            (new CleanUpWorkflowRunsAction()).RemoveOldWorkflowVersions();

            var wf2 = Entity.Get(wf.Id);

            if (isCleaned)
                Assert.IsNull(wf2);
            else
                Assert.IsNotNull(wf2);

        }
	}
}