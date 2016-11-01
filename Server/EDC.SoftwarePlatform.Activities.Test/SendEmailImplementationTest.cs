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

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
    [RunWithTransaction]
    [RunAsDefaultTenant]
	public class SendEmailImplementationTest : TestBase
	{
		private bool? _initialPostInDirectoryValue;

		[TestFixtureSetUp]
		public void TextFixtureSetup( )
		{
			using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
			{
				ImapEmailProvider provider = Entity.Get<ImapEmailProvider>( new EntityRef( "core:spInboxProvider" ), true );

				if ( provider != null )
				{
					_initialPostInDirectoryValue = provider.OaPostInDirectory;

					provider.OaPostInDirectory = true;
					provider.Save( );
				}
			}
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown( )
		{
			using ( new TenantAdministratorContext( RunAsDefaultTenant.DefaultTenantName ) )
			{
				ImapEmailProvider provider = Entity.Get<ImapEmailProvider>( new EntityRef( "core:spInboxProvider" ), true );

				if ( provider != null && provider.OaPostInDirectory != _initialPostInDirectoryValue )
				{
					provider.OaPostInDirectory = _initialPostInDirectoryValue;
					provider.Save( );
				}
			}
		}

		[Test]
        [Timeout(40000)]
	    public void SendMailUsingSingleRecipientAddress()
	    {
            SendMailWithConfigAction((wf, emailActionAs) => ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Recipient Address",
                                                                                                               "'bobble1@readinow.com'"));
	    }

        [Test]
        [Timeout(40000)]
        public void SendMailUsingMultipleRecipientAddresses()
        {
            SendMailWithConfigAction((wf, emailActionAs) => ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Recipient Address",
                                                                                                               "'bobble1@readinow.com, booble2@ereadinow.com'"));
        }

        [Test]
        [Timeout(40000)]
        public void SendMailUsingRecipientList()
        {
            SendMailWithConfigAction((wf, emailActionAs) =>
                {
                    ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Recipients",
                                                                   "all(Employee_Test)", false);
                    ActivityTestHelper.SetActivityArgumentToResource(wf, emailActionAs, "Email Field", Entity.Get<Resource>("oldshared:workEmail") );

                }
                );
        }


        [Test]
        [Timeout(40000)]
        public void SendMailWithAttachments()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes("My file" + CryptoHelper.GetRandomPrintableString(10));
                stream.Write(bytes, 0, bytes.Length);

                var fileHash = FileRepositoryHelper.AddTemporaryFile(stream);

                Document doc = new Document { Name = "MyFile", FileExtension = "txt", FileDataHash = fileHash };
                doc.Save();

                SendMailWithConfigAction((wf, emailActionAs) =>
                {
                    ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Recipient Address", "'bobble1@readinow.com'");
                    ActivityTestHelper.AddExpressionToActivityArgument(wf, emailActionAs, "Attachments", doc.Cast<Resource>().ToEnumerable());
                });

            }
        }


        public void SendMailWithConfigAction(Action<Workflow, WfActivity> configurationAction )
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                // Employee bob = new Employee() { Email = "shopwood@enterprisedata.com.au" };

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

			    ActivityTestHelper.AddExpressionToActivityArgument( wf, emailActionAs, "Subject", "'test subject'", false );
			    ActivityTestHelper.AddExpressionToActivityArgument( wf, emailActionAs, "Body", "'test body'", false );
                ActivityTestHelper.AddEntityExpressionToInputArgument(wf, emailActionAs, "From Inbox", new EntityRef("core:approvalsInbox"));

			    ActivityTestHelper.AddTermination( wf, emailActionAs );

			    wf.Save( );
			    ToDelete.Add( wf.Id );

                var wfRun = TestBase.RunWorkflow(wf);

                Assert.That(wfRun.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
            }
		}
	}
}