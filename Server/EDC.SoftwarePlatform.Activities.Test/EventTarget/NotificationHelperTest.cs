// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.EventTarget;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.EventTarget
{
    [Category("WorkflowTests")]
    [RunWithTransaction]
    [RunAsDefaultTenant]
    public class NotificationHelperTest
    {
        [Test]
        public void NotifierCompleted_NoSends()
        {
            var notification = new Notification();
            Assert.That(notification.Complete(), Is.True);
        }

        [Test]
        public void NotifierCompleted_SendNoReply()
        {
            var notification = new Notification();
            var send = new SendRecord();
            notification.SendRecords.Add(send);

            Assert.That(notification.Complete(), Is.False);
        }

        [Test]
        public void NotifierCompleted_SendWithError()
        {
            var notification = new Notification();
            var send = new SendRecord();
            notification.SendRecords.Add(send);

            send.SrErrorMessage = "Failed";

            Assert.That(notification.Complete(), Is.True);
        }

        [Test]
        public void NotifierCompleted_SendWithJustReply()
        {
            var notification = new Notification();
            var send = new SendRecord();
            notification.SendRecords.Add(send);

            send.SrToReply.Add(new ReplyRecord { RrReply = "Reply" });

            Assert.That(notification.Complete(), Is.True);
        }

        [Test]
        public void NotifierCompleted_SendWithJustReplyDate()
        {
            var notification = new Notification();
            var send = new SendRecord();
            notification.SendRecords.Add(send);

            send.SrToReply.Add(new ReplyRecord { RrReplyDate = DateTime.Now });

            Assert.That(notification.Complete(), Is.True);
        }
    }
}
