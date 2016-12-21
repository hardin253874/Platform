// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Notify;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Notify
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class EmailRouterTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void SendTest(bool expectReply)
        {
            // 
            // IN PROGRESS - Please leave
            // This code is in development and switched off until email and SMS approvals are required by PM.
            //
            if (Factory.FeatureSwitch.Get("enableWfUserActionNotify"))
            {
                var notifier = new EmailNotifier
                {
                    Name = "Test email notifier",
                    EnEmailAddressExpression = "[Business email]",
                    EmailNotifierInbox = Entity.Get<Inbox>("core:approvalsInbox")
                };

                var person = new Person();
                person.SetField("shared:businessEmail", "testermctesty@readinow.com");

                var notification = new Notification { NMessage = "Test" }
;               EmailRouter.Instance.Send(notifier, notification, person.ToEnumerable(), expectReply);

                var result = Entity.Get<Notification>(notification.Id).SendRecords;
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.First().SrErrorMessage, Is.Null);
            }
        }
    }
}
