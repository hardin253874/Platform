// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Messaging.Redis;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.BackgroundTasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;


namespace EDC.SoftwarePlatform.Activities.Test.BackgroundTasks
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class RunTriggersHandlerTest: TestBase
    {
        [Test]
        public void Registration()
        {
            var handlers = Factory.Current.Resolve<IEnumerable<ITaskHandler>>().Select(h => h.GetType());
            Assert.That(handlers, Has.Member(typeof(RunTriggersHandler)));
        }

        [Test]
        public void SerializeParams()
        {
            var p = new RunTriggersParams();

            BackgroundTask.Create<RunTriggersParams>("test", p);
        }

        [Test]
        [RunWithTransaction]
        public void Run()
        {
            var wf = CreateLoggingWorkflow();
            wf.Save();

            var trigger = new WfTriggerUserUpdatesResource { WorkflowToRun = wf };
            trigger.Save();

            var taskData = new RunTriggersParams { TriggerId = trigger.Id, EntityId = EntityType.EntityType_Type.Id };

            RunAndTestLog(taskData, (eventLogMonitor) =>
            {
                Assert.That(wf.RunningInstances.Count(), Is.EqualTo(1));
                Assert.That(wf.RunningInstances.First().WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
            });
        }


        [Test]
        public void MissingTrigger()
        {
            var taskData = new RunTriggersParams { TriggerId = 9999999, EntityId = EntityType.EntityType_Type.Id };

            RunAndTestLog(taskData, (eventLogMonitor) =>
            {
                Assert.That(eventLogMonitor.Entries.Any(l => l.Message.Contains("Trigger missing")));
            });
        }

        [Test]
        public void MissingEntity()
        {
            var trigger = new WfTriggerUserUpdatesResource();

            var taskData = new RunTriggersParams { TriggerId = trigger.Id, EntityId = 9999999 };

            RunAndTestLog(taskData, (eventLogMonitor) =>
            {
                Assert.That(eventLogMonitor.Entries.Any(l => l.Message.Contains("Entity missing")));
            });
        }


        void RunAndTestLog(RunTriggersParams p, Action<EventLogMonitor> action)
        {
            var handler = new RunTriggersHandler();

            using (var eventLogMonitor = new EventLogMonitor())
            {
                var bgTask = handler.ToBackgroundTask(p);
                handler.HandleTask(bgTask);

                action(eventLogMonitor);
            }
        }


        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void SuspendResume()
        {
            var handler = new RunTriggersHandler();

            var trigger = new WfTriggerUserUpdatesResource();
            trigger.Save();

            var resource = new Resource();
            resource.Save();

            var task = RunTriggersHandler.CreateBackgroundTask(trigger.Id, resource.Id, 5);

            var taskEntity = handler.CreateSuspendedTask(task);
            taskEntity.Save();

            var tasks = handler.RestoreSuspendedTasks();

            tasks.Should().HaveCount(1);

            var runParam = tasks.First().GetData<RunTriggersParams>();

            runParam.TriggerDepth.ShouldBeEquivalentTo(5);
            runParam.TriggerId.ShouldBeEquivalentTo(trigger.Id);
            runParam.EntityId.ShouldBeEquivalentTo(resource.Id);
        }

    }
}
