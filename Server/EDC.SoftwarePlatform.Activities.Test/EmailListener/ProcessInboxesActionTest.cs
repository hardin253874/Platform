// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Email;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.EmailListener
{
    [TestFixture]
    public class ProcessInboxesActionTest
    {

        [Test]
        [RunAsDefaultTenant]
        public void TestMatchOnInboxEmailAddress()
        {
            // Create the inboxes that the emails will be matched to
            var inbox1 = new Inbox();
            inbox1.Name = "Test Inbox " + Guid.NewGuid().ToString("N");
            inbox1.InboxEnabled = true;
            inbox1.Save();

            var inbox2 = new Inbox();
            inbox2.Name = "Test Inbox " + Guid.NewGuid().ToString("N");
            inbox2.InboxEnabled = true;
            inbox2.Save();

            var inbox3 = new Inbox();
            inbox3.Name = "Test Inbox " + Guid.NewGuid().ToString("N");
            inbox3.InboxEnabled = true;
            inbox3.Save();

            // Create our test emails data
            var rndStr = Guid.NewGuid().ToString("N");
            var email1 = new MailMessage { Body = $"Body {rndStr}", Subject = $"Subject {rndStr}", From = new MailAddress($"{rndStr}@somewhere.com") };
            email1.To.Add(new MailAddress(inbox1.InboxEmailAddress));

            rndStr = Guid.NewGuid().ToString("N");
            var email2 = new MailMessage { Body = $"Body {rndStr}", Subject = $"Subject {rndStr}", From = new MailAddress($"{rndStr}@somewhere.com") };
            email2.To.Add(new MailAddress(inbox2.InboxEmailAddress.ToLower()));    // test case in-sensitive matching

            rndStr = Guid.NewGuid().ToString("N");
            var email3 = new MailMessage { Body = $"Body {rndStr}", Subject = $"Subject {rndStr}", From = new MailAddress($"{rndStr}@somewhere.com") };
            email3.To.Add(new MailAddress($"{Guid.NewGuid().ToString("N")}@somewhere.com"));

            // Create our Moq
            var emailReceiverMoq = new Mock<IEmailReceiver>();
            emailReceiverMoq
                .Setup(m => m.GetAllMessages(It.IsAny<List<string>>()))
                .Returns(new List<MailMessage> { email1, email2, email3 });

            var action = new EDC.SoftwarePlatform.Activities.EmailListener.ProcessInboxesAction(emailReceiverMoq.Object);
            action.Execute(null);

            inbox1 = Entity.Get<Inbox>(inbox1.Id);
            Assert.True(inbox1.ReceivedMessages.Count == 1, "Failed to match received emails on Inbox");
            Assert.True(inbox1.ReceivedMessages?[0].EmSubject == email1.Subject, "Email Subject does not match");
            Assert.True(inbox1.ReceivedMessages?[0].EmBody == email1.Body, "Email Body does not match");
            Assert.True(inbox1.ReceivedMessages?[0].EmBody == email1.Body, "Email Body does not match");

            inbox2 = Entity.Get<Inbox>(inbox2.Id);
            Assert.True(inbox2.ReceivedMessages.Count == 1, "Failed to match received emails on Inbox");
            Assert.True(inbox2.ReceivedMessages?[0].EmSubject == email2.Subject, "Email Subject does not match");
            Assert.True(inbox2.ReceivedMessages?[0].EmBody == email2.Body, "Email Body does not match");
            Assert.True(inbox2.ReceivedMessages?[0].EmBody == email2.Body, "Email Body does not match");

            inbox3 = Entity.Get<Inbox>(inbox3.Id);
            Assert.True(inbox3.ReceivedMessages.Count == 0, "Incorrectly matched email with Inbox");

            Entity.Delete(inbox1.Id);
            Entity.Delete(inbox2.Id);
            Entity.Delete(inbox3.Id);
        }


        [Test]
        [RunAsDefaultTenant]
        public void TestRecievedEmailForDisabledInbox()
        {
            var inbox = new Inbox();
            inbox.Name = "Test Inbox " + Guid.NewGuid().ToString("N");
            inbox.InboxEnabled = false;
            inbox.Save();

            var rndStr = Guid.NewGuid().ToString("N");
            var email1 = new MailMessage { Body = $"Body {rndStr}", Subject = $"Subject {rndStr}", From = new MailAddress($"{rndStr}@somewhere.com") };
            email1.To.Add(new MailAddress(inbox.InboxEmailAddress));

            var emailReceiverMoq = new Mock<IEmailReceiver>();
            emailReceiverMoq
                .Setup(m => m.GetAllMessages(It.IsAny<List<string>>()))
                .Returns(new List<MailMessage> { email1, });
            
            var action = new EDC.SoftwarePlatform.Activities.EmailListener.ProcessInboxesAction(emailReceiverMoq.Object);
            action.Execute(null);

            inbox = Entity.Get<Inbox>(inbox.Id);
            Assert.True(inbox.ReceivedMessages.Count == 0, "Email for Disabled Inbox should not have been processed");

            Entity.Delete(inbox.Id);
        }

        


    }
}
