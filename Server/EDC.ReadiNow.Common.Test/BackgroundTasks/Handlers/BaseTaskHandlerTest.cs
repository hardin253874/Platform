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

namespace EDC.ReadiNow.Test.BackgroundTasks.Handlers
{
    [TestFixture]
    [RunAsDefaultTenant]

    public class BaseTaskHandlerTest
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

    }

    public class DummyHandler : TaskHandler<DummyParams>
    {
        public Action<DummyParams> OnHandleTask { get; set; }
        public DummyHandler() : base("Dummy", false)
        {
        }

        protected override void HandleTask(DummyParams taskData)
        {
            if (OnHandleTask != null)
                OnHandleTask(taskData);
        }
    }

    [ProtoContract]
    public class DummyParams: IWorkflowQueuedEvent
    {
        [ProtoMember(1)]
        public string S { get; set; }

        public DummyParams()
        {

        }
    }


}
