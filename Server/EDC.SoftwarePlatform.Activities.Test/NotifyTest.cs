// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;

using NUnit.Framework;

using EDC.ReadiNow.Core;
using ReadiNow.Integration.Test.Sms;
using System.Linq;
using EDC.ReadiNow.Model.Interfaces;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Notify;
using Autofac;
using ReadiNow.Common;
using EDC.SoftwarePlatform.Activities.Test.Mocks;
using ReadiNow.Integration.Sms;
using System.Threading;
using EDC.SoftwarePlatform.Activities.Notify;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    [Category("WorkflowTests")]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class NotifyTest: TestBase
    {
        Random _rand = new Random();


        [RunWithoutTransaction]
        [TestCase(true, LoopbackApi.SucceedNumber, true, false, true)]
        [TestCase(true, LoopbackApi.SucceedNumber, false, true, false)]
        [TestCase(true, LoopbackApi.FailNumber, true, true, false)]
        public void Run(bool testMode, string toNumber, bool waitForReplies, bool linkToRecord, bool setReplyWorkflow)
        {
            TwilioNotifier notifier = null;
            Notification nr = null;
            IEntity record = null;
            Workflow replyWorkflow = null;

            try
            {

                var sendingNumber = _rand.Next().ToString();
                var fromNumber = _rand.Next().ToString();

                var person = Entity.Create(new EntityRef("core:person"));

                var mobilePhoneField = Factory.ScriptNameResolver.GetInstance("Mobile phone", StringField.StringField_Type.Id);

                person.SetField(mobilePhoneField, toNumber);
                person.Save();
                ToDelete.Add(person.Id);
                
                notifier = new TwilioNotifier();

                notifier.Name = "TwilioTest";
                notifier.TpAccountSid = TwilioTestHelper.TestAccountSid;
                notifier.TpAuthToken = TwilioTestHelper.TestAuthToken;
                notifier.TpSendingNumber = sendingNumber;
                notifier.TpEnableTestMode = testMode;

                notifier.Save();

                // add a record
                if (linkToRecord)
                {
                    record = Entity.Create(new EntityRef("test:vehicle"));
                    record.Save();
                }

                string replyWorkflowString = setReplyWorkflow ? "Reply" : null;
                replyWorkflow = CreateLoggingWorkflow();
                replyWorkflow.WorkflowRunAsOwner = true;
                replyWorkflow.SecurityOwner = Entity.Get<UserAccount>(new EntityRef("core:administratorUserAccount"));
                replyWorkflow.Save();

                nr = RunNotify(person, notifier, waitForReplies, record, replyWorkflowString, replyWorkflow);

                Assert.That(nr.NMessage, Is.EqualTo("Test"));
                Assert.That(nr.SendRecords.Count, Is.EqualTo(1));

                if (linkToRecord)
                {
                    Assert.That(nr.NRelatedRecord?.Id, Is.EqualTo(record.Id));
                } 
                else
                {
                    Assert.That(nr.NRelatedRecord, Is.Null);
                }

                if (setReplyWorkflow)
                {
                    Thread.Sleep(1000);      // give the async triggers a chance to complete
                    Assert.That(replyWorkflow.RunningInstances.Count, Is.EqualTo(1));
                }
            }
            finally
            {
                notifier?.Delete();
                if (nr!= null)
                    Entity.Delete(nr.Id);
                record?.Delete();
                replyWorkflow?.Delete();
            }
        }


        [Test]
        public void Workflow_GoldenPath()
        {
            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.RegisterType<DummyNotifier>().As<INotifier>();
            }))
            using (Factory.SetCurrentScope(scope))
            { 

                var wf = CreateNotifyWorkflow(true);

                var person = new Person { Name = "bob", };
                person.SetField("shared:businessEmail", "bob@bob.com");
                person.Save();

                var inputs = new Dictionary<string, object>
                {
                    { "People", person.ToEnumerable() },
                    { "Message", "Test"}
                };

                WorkflowRun run1;

                using (new TestWfRunContext())
                {
                    run1 = (RunWorkflow(wf, inputs));
                }

                Assert.That(run1.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunPaused));

                var notifications = person.GetRelationships(new EntityRef("core", "srToPerson"), Direction.Reverse);

                Assert.That(notifications.Count(), Is.EqualTo(1));

                var send = notifications.First().As<SendRecord>();

                Assert.That(send.SendToNotification, Is.Not.Null);
                Assert.That(send.SendToNotification.NMessage, Is.EqualTo("Test"));

                using (new TestWfRunContext())
                {
                    // reply
                    var reply = new ReplyRecord { RrToSend = send, RrReply = "replying", RrReplyDate = DateTime.UtcNow };
                    reply.Save();
                }

                var run2 = Entity.Get<WorkflowRun>(run1.Id);

                Assert.That(run2.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));

                IDictionary<string, object> outputs = run1.GetOutput();

                Assert.That(outputs.Keys, Has.Member("Notification Record"));
                var notification = (IEntity)outputs["Notification Record"];
                Assert.That(notification, Is.Not.Null);
            }
        }
            

       

        [Test]
        public void NotifyWithNoRecipientsProgressesImmediately()
        {
            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.RegisterType<DummyNotifier>().As<INotifier>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var wf = CreateNotifyWorkflow(true);

                var inputs = new Dictionary<string, object>
                {
                    { "People", Enumerable.Empty<IEntity>() },
                    { "Message", "Test"}
                };

                WorkflowRun run1;
                using (new TestWfRunContext())
                {
                    run1 = RunWorkflow(wf, inputs);
                }

                Assert.That(run1.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));

                IDictionary<string, object> outputs = run1.GetOutput();

                Assert.That(outputs.Keys, Has.Member("Notification Record"));
                var notification = (IEntity)outputs["Notification Record"];
                Assert.That(notification, Is.Not.Null);
            }
        }


        [Test]
        public void NotifyDelayedErrorProgressesWf()
        {
            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.RegisterType<DummyNotifier>().As<INotifier>();
            }))
            using (Factory.SetCurrentScope(scope))
            {

                var wf = CreateNotifyWorkflow(true);

                var person = new Person { Name = "bob", };
                person.SetField("shared:businessEmail", "bob@bob.com");
                person.Save();

                var inputs = new Dictionary<string, object>
                {
                    { "People", person.ToEnumerable() },
                    { "Message", "Test"}
                };

                WorkflowRun run1;

                using (new TestWfRunContext())
                {
                    run1 = (RunWorkflow(wf, inputs));
                }

                Assert.That(run1.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunPaused));

                var notifications = person.GetRelationships(new EntityRef("core", "srToPerson"), Direction.Reverse);
                var send = notifications.First().As<SendRecord>();

                using (new TestWfRunContext())
                {
                    // Delayed fail of the send
                    var writable = send.AsWritable<SendRecord>();
                    writable.SrErrorMessage = "Error";
                    writable.Save();
                }

                var run2 = Entity.Get<WorkflowRun>(run1.Id);

                Assert.That(run2.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
            }
        }


        [Test]
        public void NotifyWaitForReplies()
        {
            var dummyTimeoutHelper = new DummyTimeoutHelper();

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(ctx => dummyTimeoutHelper).As<ITimeoutActivityHelper>();
                builder.RegisterType<DummyNotifier>().As<INotifier>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                var wf = CreateNotifyWorkflow(true);

                var person = new Person { Name = "bob", };
                person.SetField("shared:businessEmail", "bob@bob.com");

                var inputs = new Dictionary<string, object>
                {
                    { "People", person.ToEnumerable() },
                    { "Message", "Test"}
                };

                var run1 = (RunWorkflow(wf, inputs));

                dummyTimeoutHelper.Timeout();

                var run2 = Entity.Get<WorkflowRun>(run1.Id);
                Assert.That(run2.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
            }
        }


        [Test]
        public void ReplyWorkflowIsRunAsOwmer()
        {
            var errors = ValidateReplyWorkflow(true);

            Assert.That(errors.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ReplyWorkflowNotRunAsOwmer()
        {
            var errors = ValidateReplyWorkflow(false);

            Assert.That(errors.Count(), Is.EqualTo(1));
            Assert.That(errors.First(), Contains.Substring("set to 'Run as owner'"));
        }



        public IEnumerable<string> ValidateReplyWorkflow(bool runAsOwner)
        {
            Workflow replyWorkflow = new Workflow { WorkflowRunAsOwner = runAsOwner };
            var replyMap = new Dictionary<string, Workflow> { { "reply", replyWorkflow } };

            Workflow wf = new Workflow();
            wf.AddDefaultExitPoint()
              .AddNotify("Notify", "all(Person)", "'Message'", false, replyMap);

            wf.Save();

            return wf.Validate();
        }


        Notification RunNotify(IEntity person, TwilioNotifier notifier, bool waitForReplies, IEntity linkTo = null, string replyWorkflowString = null, Workflow replyWorkflow = null)
        {
            var inputs = new Dictionary<string, object>
                {
                    { "People", person.ToEnumerable() },
                    { "LinkToRecord", linkTo},
                };

            var wf = new Workflow { Name = "RunNotify " + DateTime.UtcNow.ToString() };
            wf
                .AddDefaultExitPoint()
                .AddInput<ResourceListArgument>("People", Person.Person_Type)
                .AddInput<ResourceArgument>("LinkToRecord", UserResource.UserResource_Type)
                .AddOutput<ResourceArgument>("NotificationRecord", Notification.Notification_Type)
                .AddNotify(
                    "Notify", "People", "'Test'",
                    waitForReplies,
                    linkToRecordExpression: "LinkToRecord",
                    replyMap: replyWorkflowString == null ? null :
                        new Dictionary<string, Workflow> { { replyWorkflowString, replyWorkflow } })
                .AddAssignToVar("Assign", "[Notify_Notification Record]", "NotificationRecord");

            wf.Save();
            ToDelete.Add(wf.Id);            

            using (var scope = Factory.Current.BeginLifetimeScope(builder =>
            {
                builder.Register(c => new TenantSmsNotifier(notifier.Id)).As<INotifier>();
            }))
            using (Factory.SetCurrentScope(scope))
            {
                WorkflowRun run = null;

                using (new TestWfRunContext())
                {
                    run = RunWorkflow(wf, inputs);
                    run.Save();
                }

                Thread.Sleep(1000);

                Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
                
                var results = run.GetOutput();

                Assert.That(results.Keys, Has.Member("NotificationRecord"));

                var entity = (IEntity)results["NotificationRecord"];
                return entity.As<Notification>();
            }
        }


        Workflow CreateNotifyWorkflow(bool waitForReplies)
        {


            var wf = new Workflow() { Name = "GoldenPath" + DateTime.Now };

            wf.AddDefaultExitPoint();

            wf.InputArguments.Add((new ResourceListArgument { Name = "People", ConformsToType = Person.Person_Type }).As<ActivityArgument>());
            wf.InputArguments.Add((new StringArgument { Name = "Message" }).As<ActivityArgument>());
            wf.OutputArguments.Add((new ResourceArgument { Name = "Notification Record", ConformsToType = Notification.Notification_Type }).As<ActivityArgument>());

            wf.AddNotify("Notify", "[People]", "[Message]", waitForReplies);
            wf.AddAssignToVar("Assign", "[Notify_Notification Record]", "Notification Record");

            wf.Save();

            return wf;
        }

        public class DummyNotifier : INotifier
        {
            public void Send(Notification notification, IEnumerable<IEntity> people, bool expectReply)
            {
                
                foreach (var p in people)
                {                
                    var record = new SendRecord { SrToPerson = p.As<Person>(), SrSendDate = DateTime.Now, SendToNotification = notification };
                    record.Save();
                }
            }

            public bool IsConfigured { get { return true; } }
        }
    }

}