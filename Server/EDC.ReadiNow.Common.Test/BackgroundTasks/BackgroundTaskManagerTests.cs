// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.BackgroundTasks;
using EDC.ReadiNow.BackgroundTasks.Handlers;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Test.BackgroundTasks
{
    [TestFixture]
    public class BackgroundTaskManagerTests
    {
        [Test]
        [RunAsGlobalTenant]
        public void Registeration()
        {
            var manager = Factory.Current.Resolve<IBackgroundTaskManager>();

            Assert.That(manager, Is.Not.Null);
        }

        static string result = "";
        [Test]
        [RunAsGlobalTenant]
        public void Runs()
        {
            var edcTenantId = TenantHelper.GetTenantId("EDC", true);

            //static string result = "";
            result = "";

            Action<DummyParam> act = (p) => 
            {
                result += p.S;
            };


            var handler = new DummyHandler {Action = act};
            var qFactory = new RedisTenantQueueFactory("BackgroundTaskManagerTests " + Guid.NewGuid());
            var manager = new BackgroundTaskManager(qFactory, handlers: handler.ToEnumerable());

            try
            {

                manager.EnqueueTask(edcTenantId, BackgroundTask.Create("DummyHandler", new DummyParam { S = "a" }));
                Thread.Sleep(100);
                Assert.That(result, Is.Empty);

                manager.Start();
                Thread.Sleep(100);

                Assert.That(result, Is.EqualTo("a"));

                manager.EnqueueTask(edcTenantId, BackgroundTask.Create("DummyHandler", new DummyParam { S = "b" }));
                Thread.Sleep(100);

                Assert.That(result, Is.EqualTo("ab"));

                manager.Stop();

                manager.EnqueueTask(edcTenantId, BackgroundTask.Create("DummyHandler", new DummyParam { S = "c" }));
                Thread.Sleep(100);

                Assert.That(result, Is.EqualTo("ab"));      // c not processed
                
            }
            finally
            {
                manager.Stop();
                manager.EmptyQueue(edcTenantId);
            }
        }

        public class DummyHandler : TaskHandler<DummyParam>
        {
            public DummyHandler() : base("DummyHandler", false)
            {
            }

            public Action<DummyParam> Action { get; set; }



            protected override void HandleTask(DummyParam taskData)
            {
                Action(taskData);
            }
        }

        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        public class DummyParam: IWorkflowQueuedEvent
        {
            public string S { get; set; }

        }
      

    }
}
