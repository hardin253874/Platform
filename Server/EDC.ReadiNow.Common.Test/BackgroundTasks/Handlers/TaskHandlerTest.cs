// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.BackgroundTasks.Handlers;
using EDC.ReadiNow.Model.Interfaces;
using NUnit.Framework;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using Moq;
using System.Security;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Test.BackgroundTasks.Handlers
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]

    public class TaskHandlerTest
    {
        [Test]
        public void Test()
        {
            var handler = new DummyHandler();

            Assert.That(handler.TaskHandlerKey, Is.EqualTo("Dummy"));

            handler.OnHandleTask = (dp) =>
            {
                Assert.That(dp.S, Is.EqualTo("aaa"));
            };

            var bgTask = handler.ToBackgroundTask(new DummyParams { S = "aaa" });

            Assert.That(bgTask.HandlerKey, Is.EqualTo("Dummy"));

            handler.HandleTask(bgTask);
        }

        [Test]
        public void SuspendRestore()
        {
            var mockHandler = new Mock<DummyHandler>();

            var handler = new DummyHandler();

            var primaryUa = new UserAccount() { Name = Guid.NewGuid().ToString() };
            primaryUa.Save();
            var secondaryUa = new UserAccount() { Name = Guid.NewGuid().ToString() };
            secondaryUa.Save();

            var primaryId = new IdentityInfo(primaryUa.Id, primaryUa.Name);
            var secondaryId = new IdentityInfo(secondaryUa.Id, secondaryUa.Name);

            var contextData = new RequestContextData(primaryId, RequestContext.GetContext().Tenant, "XYZ", secondaryId) { TimeZone = "XYZTZ" };

            var oldContext = new RequestContextData(RequestContext.GetContext());
            try
            {
                RequestContext.SetContext(contextData);


                using (new SecurityBypassContext())
                {

                    // Suspend
                    IEntity suspendedtask;
                    BackgroundTask bgTask;


                    bgTask = handler.ToBackgroundTask(new DummyParams() { S = "SuspendRestore" });

                    suspendedtask = handler.CreateSuspendedTask(bgTask);
                    suspendedtask.Save();

                    Assert.That(handler.annotateSuspendedTask_calls, Is.EqualTo(1));

                    var castTask = suspendedtask.As<SuspendedTask>();
                    Assert.That(castTask.StCulture, Is.EqualTo("XYZ"));
                    Assert.That(castTask.StTimezone, Is.EqualTo("XYZTZ"));
                    Assert.That(castTask.StIdentity.Id, Is.EqualTo(primaryUa.Id));
                    Assert.That(castTask.StSecondaryIdentity.Id, Is.EqualTo(secondaryUa.Id));

                    IEnumerable<BackgroundTask> restoredTasks;

                    //Restore
                    restoredTasks = handler.RestoreSuspendedTasks();

                    Assert.That(handler.restoreTaskData_Calls, Is.EqualTo(1));
                    Assert.That(restoredTasks.Count(), Is.EqualTo(1));

                    var context = restoredTasks.First().Context;

                    Assert.That(context.Culture, Is.EqualTo("XYZ"));
                    Assert.That(context.TimeZone, Is.EqualTo("XYZTZ"));
                    Assert.That(context.Identity.Id, Is.EqualTo(primaryId.Id));
                    Assert.That(context.SecondaryIdentity.Id, Is.EqualTo(secondaryId.Id));

                    var parameter = restoredTasks.First().GetData<DummyParams>();
                    Assert.That(parameter.S, Is.EqualTo("restored"));

                }
            }
            finally
            {
                RequestContext.SetContext(oldContext);
            }
        }

        [Test]
        public void SuspendWithNoSecondaryId()
        {
            var oldContext = new RequestContextData(RequestContext.GetContext());
            try
            {

                var mockHandler = new Mock<DummyHandler>();

                var handler = new DummyHandler();

                var bgTask = handler.ToBackgroundTask(new DummyParams() { S = "SuspendRestore" });

                var primaryUa = new UserAccount() { Name = "primaryId" };

                var primaryId = new IdentityInfo(primaryUa.Id, primaryUa.Name);

                var contextData = new RequestContextData(primaryId, RequestContext.GetContext().Tenant, "XYZ", null) { TimeZone = "XYZTZ" };

                RequestContext.SetContext(contextData);

                using (new SecurityBypassContext())
                {
                    // Suspend
                    IEntity suspendedtask = handler.CreateSuspendedTask(bgTask);
                }
            }
            finally
            {
                RequestContext.SetContext(oldContext);
            }

        }





        public class DummyHandler : TaskHandler<DummyParams>
        {

            public Action<DummyParams> OnHandleTask { get; set; }

            protected override EntityType SuspendedTaskType { get; }

            public DummyHandler() : base("Dummy", false)
            {
                SuspendedTaskType = new EntityType() { Name = "DummyHandlerSuspendType" };
                SuspendedTaskType.Inherits.Add(SuspendedTask.SuspendedTask_Type);
                SuspendedTaskType.Save();
            }

            protected override void HandleTask(DummyParams taskData)
            {
                if (OnHandleTask != null)
                    OnHandleTask(taskData);
            }


            public int annotateSuspendedTask_calls = 0;
            protected override void AnnotateSuspendedTask(IEntity suspendedTask, DummyParams p)
            {
                annotateSuspendedTask_calls++;
            }


            public int restoreTaskData_Calls = 0;

            protected override DummyParams RestoreTaskData(IEntity suspendedTask)
            {
                restoreTaskData_Calls++;
                return new DummyParams { S = "restored" };
            }
        }

        [ProtoContract]
        public class DummyParams : IWorkflowQueuedEvent
        {
            [ProtoMember(1)]
            public string S { get; set; }

            public DummyParams()
            {

            }
        }

    }
}
