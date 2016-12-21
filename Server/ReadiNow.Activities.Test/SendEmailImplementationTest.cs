// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.IO;
using System.IO;
using EDC.Security;
using EDC.ReadiNow.Core;
using Autofac;
using Moq;
using EDC.ReadiNow.Email;
using System.Net.Mail;
using System.Text;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.IO;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
	public class SendEmailImplementationTest : TestBase
	{
		[TestFixtureSetUp]
		public void TextFixtureSetup( )
		{
			
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown( )
		{
			
		}

        bool ContainsAddress(MailAddressCollection addresses, string addressStr)
        {
            foreach(var str in addresses)
            {
                if (str.Address.ToLower() == addressStr.ToLower())
                    return true;
            }
            return false;
        }

		[Test]
        [Timeout(40000)]
	    public void SendMailUsingAddresses()
	    {
            string subject = "Subject " + Guid.NewGuid().ToString();
            string body = "Body " + Guid.NewGuid().ToString();

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(m => m.SendMessages(It.IsAny<IReadOnlyCollection<MailMessage>>()))
                .Callback<IReadOnlyCollection<MailMessage>>(x =>
                {
                    var emails = x.ToList();
                    Assert.True(emails.Count == 1);
                    Assert.True(emails[0].Subject == subject);
                    Assert.True(emails[0].Body == body);
                    Assert.True(emails[0].To.Count == 3);
                    Assert.True(ContainsAddress(emails[0].To, "email1@readinow.com"));
                    Assert.True(ContainsAddress(emails[0].To, "email2@readinow.com"));
                    Assert.True(ContainsAddress(emails[0].To, "email3@readinow.com"));
                    Assert.True(emails[0].CC.Count == 2);
                    Assert.True(ContainsAddress(emails[0].CC, "email4@readinow.com"));
                    Assert.True(ContainsAddress(emails[0].CC, "email5@readinow.com"));
                    Assert.True(emails[0].Bcc.Count == 1);
                    Assert.True(ContainsAddress(emails[0].Bcc, "email6@readinow.com"));
                    Assert.True(emails[0].Attachments.Count == 0);
                });

            Action<Workflow, WfActivity> configurationCallback = (wf, emailActionAs) =>
            {
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Email To", new EntityRef("core:sendEmailActivityRecipientsAddress"));
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "To Addresses", "'email1@readinow.com; email2@readinow.com;email3@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "CC Addresses", "'email4@readinow.com; email5@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "BCC Addresses", "'email6@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Subject", $"'{subject}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Body", $"'{body}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "No Reply", "true", false);
            };

            SendMailWithConfigAction(configurationCallback, emailSenderMock.Object);
	    }


        [Test]
        [Timeout(40000)]
        public void SendMailUsingAddressesFromInbox()
        {
            string subject = "Subject " + Guid.NewGuid().ToString();
            string body = "Body " + Guid.NewGuid().ToString();

            var inbox = new Inbox() { Name = "Test Inbox 1", InboxEmailAddress = "test12354@somewhere.com" };
            inbox.Save();

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(m => m.SendMessages(It.IsAny<IReadOnlyCollection<MailMessage>>()))
                .Callback<IReadOnlyCollection<MailMessage>>(x =>
                {
                    var emails = x.ToList();
                    Assert.True(emails.Count == 1);
                    Assert.True(emails[0].Subject == subject);
                    Assert.True(emails[0].Body == body);
                    Assert.True(emails[0].To.Count == 1);
                    Assert.True(ContainsAddress(emails[0].To, "email1@readinow.com"));
                    Assert.True(emails[0].CC.Count == 1);
                    Assert.True(ContainsAddress(emails[0].CC, "email2@readinow.com"));
                    Assert.True(emails[0].Bcc.Count == 1);
                    Assert.True(ContainsAddress(emails[0].Bcc, "email3@readinow.com"));
                    Assert.True(emails[0].Attachments.Count == 0);
                    Assert.True(emails[0].From.DisplayName == inbox.Name);
                    Assert.True(emails[0].From.Address == inbox.InboxEmailAddress);
                });

            Action<Workflow, WfActivity> configurationCallback = (wf, emailActionAs) =>
            {
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Email To", new EntityRef("core:sendEmailActivityRecipientsAddress"));
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "To Addresses", "'email1@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "CC Addresses", "'email2@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "BCC Addresses", "'email3@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Subject", $"'{subject}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Body", $"'{body}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "No Reply", "false", false);
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Reply to Inbox", new EntityRef(inbox));
            };

            SendMailWithConfigAction(configurationCallback, emailSenderMock.Object);

            inbox.Delete();
        }

        [Test]
        [Timeout(40000)]
        public void SendMailUsingAddressesFromInboxWithReplyAddress()
        {
            string subject = "Subject " + Guid.NewGuid().ToString();
            string body = "Body " + Guid.NewGuid().ToString();

            var inbox = new Inbox() { Name = "Test Inbox 2", InboxEmailAddress = "test7891@somewhere.com", InboxFromName = "TestFromName", InboxReplyAddress = "RepyAddress@somewhereelsecom" };
            inbox.Save();

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(m => m.SendMessages(It.IsAny<IReadOnlyCollection<MailMessage>>()))
                .Callback<IReadOnlyCollection<MailMessage>>(x =>
                {
                    var emails = x.ToList();
                    Assert.True(emails.Count == 1);
                    Assert.True(emails[0].Subject == subject);
                    Assert.True(emails[0].Body == body);
                    Assert.True(emails[0].To.Count == 1);
                    Assert.True(ContainsAddress(emails[0].To, "email1@readinow.com"));
                    Assert.True(emails[0].CC.Count == 1);
                    Assert.True(ContainsAddress(emails[0].CC, "email2@readinow.com"));
                    Assert.True(emails[0].Bcc.Count == 1);
                    Assert.True(ContainsAddress(emails[0].Bcc, "email3@readinow.com"));
                    Assert.True(emails[0].Attachments.Count == 0);
                    Assert.True(emails[0].From.DisplayName == inbox.InboxFromName);
                    Assert.True(emails[0].From.Address == inbox.InboxReplyAddress);
                });

            Action<Workflow, WfActivity> configurationCallback = (wf, emailActionAs) =>
            {
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Email To", new EntityRef("core:sendEmailActivityRecipientsAddress"));
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "To Addresses", "'email1@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "CC Addresses", "'email2@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "BCC Addresses", "'email3@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Subject", $"'{subject}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Body", $"'{body}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "No Reply", "false", false);
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Reply to Inbox", new EntityRef(inbox));

            };

            SendMailWithConfigAction(configurationCallback, emailSenderMock.Object);

            inbox.Delete();
        }

        [Test]
        [Timeout(40000)]
        public void SendMailUsingAddressesWithBadAddress()
        {
            string subject = "Subject " + Guid.NewGuid().ToString();
            string body = "Body " + Guid.NewGuid().ToString();

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(m => m.SendMessages(It.IsAny<IReadOnlyCollection<MailMessage>>()))
                .Callback<IReadOnlyCollection<MailMessage>>(x =>
                {
                    var emails = x.ToList();
                    Assert.True(emails.Count == 1);
                    Assert.True(emails[0].Subject == subject);
                    Assert.True(emails[0].Body == body);
                    Assert.True(emails[0].To.Count == 2);
                    Assert.True(ContainsAddress(emails[0].To, "email1@readinow.com"));
                    Assert.True(ContainsAddress(emails[0].To, "email2@readinow.com"));
                    Assert.True(emails[0].CC.Count == 0);
                    Assert.True(emails[0].Bcc.Count == 0);
                    Assert.True(emails[0].Attachments.Count == 0);
                });

            Action<Workflow, WfActivity> configurationCallback = (wf, emailActionAs) =>
            {
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Email To", new EntityRef("core:sendEmailActivityRecipientsAddress"));
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "To Addresses", "'email1@readinow.com; NotAnEmailAddress.com; email2@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Subject", $"'{subject}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Body", $"'{body}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "No Reply", "true", false);
            };

            SendMailWithConfigAction(configurationCallback, emailSenderMock.Object);
        }

        [Test]
        [Timeout(40000)]
        public void SendMailUsingRecipientList()
        {
            string subject = "Subject " + Guid.NewGuid().ToString();
            string body = "Body " + Guid.NewGuid().ToString();

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(m => m.SendMessages(It.IsAny<IReadOnlyCollection<MailMessage>>()))
                .Callback<IReadOnlyCollection<MailMessage>>(x =>
                {
                    var emails = x.ToList();
                    Assert.True(emails.Count == 1);
                    Assert.True(emails[0].Subject == subject);
                    Assert.True(emails[0].Body == body);
                    Assert.True(emails[0].To.Count > 1);
                    Assert.True(emails[0].CC.Count == 0);
                    Assert.True(emails[0].Bcc.Count == 0);
                    Assert.True(emails[0].Attachments.Count == 0);
                });

            Action<Workflow, WfActivity> configurationCallback = (wf, emailActionAs) =>
            {
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Email To", new EntityRef("core:sendEmailActivityRecipientsList"));
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Recipients List", "all(Employee_Test)", false);
                ActivityTestHelper.SetActivityArgumentToResource(wf, emailActionAs, "TO Address Field", Entity.Get<Resource>("oldshared:workEmail"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Subject", $"'{subject}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Body", $"'{body}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "No Reply", "true", false);
            };

            SendMailWithConfigAction(configurationCallback, emailSenderMock.Object);
        }

        
        [Test]
        [Timeout(40000)]
        public void SendMailWithAttachments()
        {
            string subject = "Subject " + Guid.NewGuid().ToString();
            string body = "Body " + Guid.NewGuid().ToString();

            string tempHash = null;
            var buff = Encoding.UTF8.GetBytes("Email Attachment File " + CryptoHelper.GetRandomPrintableString(10));
            using (MemoryStream stream = new MemoryStream(buff))
            {
                tempHash = Factory.DocumentFileRepository.Put(stream);
            }

            var file = Entity.Create<Document>();
            file.Name = "Email Attachment Test File";
            file.Description = "Send Email Attachment Test";
            file.FileDataHash = tempHash;
            file.Size = buff.Length;
            file.FileExtension = "txt";
            file.Save();
            
            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(m => m.SendMessages(It.IsAny<IReadOnlyCollection<MailMessage>>()))
                .Callback<IReadOnlyCollection<MailMessage>>(x =>
                {
                    var emails = x.ToList();
                    Assert.True(emails.Count == 1);
                    Assert.True(emails[0].To.Count == 2);
                    Assert.True(ContainsAddress(emails[0].To, "bobble1@readinow.com"));
                    Assert.True(ContainsAddress(emails[0].To, "bobble2@readinow.com"));
                    Assert.True(emails[0].CC.Count == 0);
                    Assert.True(emails[0].Bcc.Count == 0);
                    Assert.True(emails[0].Attachments.Count == 1);
                    Assert.True(emails[0].Attachments[0].ContentStream.Length == buff.Length);
                });


            Action<Workflow, WfActivity> configurationCallback = (wf, emailActionAs) =>
            {
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Email To", new EntityRef("core:sendEmailActivityRecipientsAddress"));
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "Send As", new EntityRef("core:sendEmailActivityGroupDistribution"));
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "To Addresses", "'bobble1@readinow.com; bobble2@readinow.com'");
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Subject", $"'{subject}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Body", $"'{body}'", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "No Reply", "true", false);
                ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Attachments", file.Cast<Resource>().ToEnumerable());
            };

            SendMailWithConfigAction(configurationCallback, emailSenderMock.Object);

            file.Delete();
        }


        public void SendMailWithConfigAction(Action<Workflow, WfActivity> configurationAction, IEmailSender emailSender)
		{

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var emailAction = new SendEmailActivity( );
                
                var emailActionAs = emailAction.Cast<WfActivity>( );

			    var wf = new Workflow
				    {
					    Name = "Wf"
				    };
			    wf.AddDefaultExitPoint( );
			    wf.ContainedActivities.Add( emailActionAs );

			    wf.FirstActivity = emailActionAs;
		        configurationAction(wf, emailActionAs);
			    ActivityTestHelper.AddTermination( wf, emailActionAs );
			    wf.Save( );
                ToDelete.Add( wf.Id );
                
                using (var scope = Factory.Current.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance<IEmailSender>(emailSender);

                }))
                using (Factory.SetCurrentScope(scope))
                {

                    var wfRun = TestBase.RunWorkflow(wf);
                    Assert.That(wfRun.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
                }

                
            }
		}

	}
}