// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.FeatureSwitch;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using EDC.SoftwarePlatform.Activities.Engine;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    public class SuspendRestoreTest
    {
        [Test]
        [Timeout(30000)]
        [RunAsDefaultTenant]
        public void Test()
        {
            if (!Factory.FeatureSwitch.Get("longRunningWorkflow"))
                return;

            var myRunner = new WorkflowRunner(new CachingWorkflowMetadataFactory());
            myRunner.SuspendTimeoutMs = 100;

            var mockTaskManager =  new Mock<IBackgroundTaskManager>(MockBehavior.Strict);
            mockTaskManager.Setup(a => a.EnqueueTask(It.IsAny<BackgroundTask>()));          // Make sure the suspended task is put back on the queue

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => myRunner).As<IWorkflowRunner>();
                builder.Register(ctx => mockTaskManager.Object).As<IBackgroundTaskManager>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                Workflow wf = null;
                try
                {
                    wf = CreateLoopWf();
                    wf.Save();

                    WorkflowRun run1 = null;

                    using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                    {
                        run1 = WorkflowRunner.Instance.RunWorkflow(new WorkflowStartEvent(wf));
                    }

                    var runStep1 = run1.RunStepCounter;
                    var runTime1 = run1.TotalTimeMs;

                    Assert.That(run1.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunSuspended));
                    Assert.That(runStep1, Is.GreaterThan(0));
                    Assert.That(runTime1, Is.GreaterThan(0));

                    WorkflowRun run2 = null;

                    using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                    {
                        run2 = WorkflowRunner.Instance.ResumeWorkflow(run1, new WorkflowRestoreEvent());
                    }

                    var runStep2 = run2.RunStepCounter;
                    var runTime2 = run2.TotalTimeMs;

                    Assert.That(run2.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunSuspended));
                    Assert.That(runStep2, Is.GreaterThan(runStep1));
                    Assert.That(runTime2, Is.GreaterThan(runTime1));

                    WorkflowRun run3 = null;

                    using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
                    {
                        run3 = WorkflowRunner.Instance.ResumeWorkflow(run1, new WorkflowRestoreEvent());
                    }

                    Assert.That(run3.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunSuspended));
                    Assert.That(run3.RunStepCounter, Is.GreaterThan(runStep2));
                    Assert.That(run3.TotalTimeMs, Is.GreaterThan(runTime2));
                }
                finally
                {
                    wf?.Delete();
                }
            }

            mockTaskManager.VerifyAll();
        }




        Workflow CreateLoopWf()
        {
            Workflow wf = new Workflow() { Name = "SuspendRestoreTest " + DateTime.UtcNow };
            wf.AddVariable<IntegerArgument>("i", "0");
            wf.AddDefaultExitPoint();
            wf.AddAssignToVar("Assign", "i + 1", "i");
            wf.AddTransition("Assign", "Assign");
            return wf;
        }
    }

   

}
