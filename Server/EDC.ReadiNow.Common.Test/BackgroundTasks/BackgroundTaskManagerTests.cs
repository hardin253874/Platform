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
	    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds( 30 );

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
		public void Runs( )
		{
			var edcTenantId = TenantHelper.GetTenantId( "EDC", true );

			//static string result = "";
			result = "";

			using ( CountdownEvent evt = new CountdownEvent( 1 ) )
			{
				Action<DummyParam> act = ( p ) =>
				{
					result += p.S;
					// ReSharper disable once AccessToDisposedClosure
					evt.Signal( );
				};


				var handler = new DummyHandler { Action = act };
				var qFactory = new RedisTenantQueueFactory( "BackgroundTaskManagerTests " + Guid.NewGuid( ) );
				var manager = new BackgroundTaskManager( qFactory, handlers: handler.ToEnumerable( ) ) { IsActive = true };

				try
				{

					manager.EnqueueTask( edcTenantId, BackgroundTask.Create( "DummyHandler", new DummyParam { S = "a" } ) );
					Thread.Sleep( 100 );
					Assert.That( result, Is.Empty );

					manager.Start( );

					evt.Wait( DefaultTimeout );
					evt.Reset( );

					Assert.That( result, Is.EqualTo( "a" ) );

					manager.EnqueueTask( edcTenantId, BackgroundTask.Create( "DummyHandler", new DummyParam { S = "b" } ) );

					evt.Wait( DefaultTimeout );
					evt.Reset( );

					Assert.That( result, Is.EqualTo( "ab" ) );

					manager.Stop( );

					manager.EnqueueTask( edcTenantId, BackgroundTask.Create( "DummyHandler", new DummyParam { S = "c" } ) );
					Thread.Sleep( 100 );

					Assert.That( result, Is.EqualTo( "ab" ) );      // c not processed

				}
				finally
				{
					manager.Stop( );
					var items = manager.EmptyQueue( edcTenantId );
					Assert.That( items.Count( ), Is.EqualTo( 1 ) );
				}
			}
		}

        [Test]
        [Ignore]
        public void SuspendRestore()
        {
            Assert.Fail("Not done yet");
        }

        public class DummyHandler : TaskHandler<DummyParam>
        {
            public DummyHandler() : base("DummyHandler", false)
            {
            }

            public Action<DummyParam> Action { get; set; }

            protected override EntityType SuspendedTaskType
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            protected override void AnnotateSuspendedTask(IEntity suspendedTask, DummyParam backgroundTask)
            {
                throw new NotImplementedException();
            }


            protected override void HandleTask(DummyParam taskData)
            {
                Action(taskData);
            }

            protected override DummyParam RestoreTaskData(IEntity suspendedTask)
            {
                throw new NotImplementedException();
            }
        }

        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        public class DummyParam: IWorkflowQueuedEvent
        {
            public string S { get; set; }

        }
      

    }
}
