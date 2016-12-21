// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.ReadiNow.Diagnostics.ActivityLog;
using Moq;
using System;
using System.Linq.Expressions;
//using System.Activities;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class LogActivityTest : TestBase
	{
	    public void LogMessage(string message, IEntity referencedObject = null )
		{
			var logActivity = new LogActivity
				{
					Name = "Log",
				};
			logActivity.Save( );
			ToDelete.Add( logActivity.Id );

			var nextActivity = ( LogActivityImplementation ) logActivity.As<WfActivity>( ).CreateWindowsActivity( );

			var inputs = new Dictionary<string, object>
				{
					{
						"Message", message
                    },
                    {
                        "Object", referencedObject
                    }
                };

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                RunActivity(nextActivity, inputs);
            }

		}

        [Test]
        public void LogMessage()
        {
            Mock<IActivityLogWriter> mockLogWriter;
            List<TenantLogEntry> log;

            var mockRepository = new MockRepository(MockBehavior.Strict);

            CreateMockedLog(mockRepository, out mockLogWriter, out log);

            var msg = "LogMessage" + DateTime.UtcNow;

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockLogWriter.Object).As<IActivityLogWriter>();

            }))
            using (Factory.SetCurrentScope(scope))
            {
                LogMessage(msg);
            }

            Assert.That(log.Count, Is.EqualTo(1));

            var message = log[0].Cast<LogActivityLogEntry>();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.Description, Is.EqualTo(msg));
        }

        [Test]
        public void LogResource()
        {
            Mock<IActivityLogWriter> mockLogWriter;
            List<TenantLogEntry> log;

            var mockRepository = new MockRepository(MockBehavior.Strict);

            CreateMockedLog(mockRepository, out mockLogWriter, out log);

            var msg = "LogResource" + DateTime.UtcNow;
            var p = new Person { Name = "Bob" };
            p.Save();

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => mockLogWriter.Object).As<IActivityLogWriter>();

            }))
            using (Factory.SetCurrentScope(scope))
            {

                LogMessage(msg, p);
            }

            Assert.That(log.Count, Is.EqualTo(1));

            var message = log[0].Cast<LogActivityResourceLogEntry>();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.Description, Is.EqualTo(msg));
            Assert.That(message.ObjectReferencedInLog, Is.Not.Null);
            Assert.That(message.ReferencedObjectName, Is.EqualTo("Bob"));
            Assert.That(message.ObjectReferencedInLog.Id, Is.EqualTo(p.Id));
        }

        void CreateMockedLog(MockRepository mockRepo, out Mock<IActivityLogWriter> mockLogWriter, out List<TenantLogEntry> loggedMessages)
        {
            mockLogWriter = mockRepo.Create<IActivityLogWriter>(MockBehavior.Loose);

            var log = new List<TenantLogEntry>();

            Expression<Action<IActivityLogWriter>>  logAction = (lw) => lw.WriteLogEntry(It.IsAny<TenantLogEntry>());

            mockLogWriter.Setup(logAction).Callback<TenantLogEntry>(tle => log.Add(tle));
            loggedMessages = log;
        }
    }
}