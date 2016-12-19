// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Bugs
{
    [TestFixture]
    public class Bug_25679_noOwner : TestBase
    {
        [Test]
        [RunAsDefaultTenant, RunWithTransaction]
        [Category("WorkflowTests"), Category("Bug")]
        //[Category( "ExtendedTests" )]
        public void Test()
        {
            WorkflowRun run;

            using (new WorkflowRunContext() { RunTriggersInCurrentThread = true })
            {
                var wf = CreateLoggingWorkflow("Bug_25679_noOwner");
                wf.WorkflowRunAsOwner = true;
                wf.SecurityOwner = null;
                wf.Save();

                var owner = wf.SecurityOwner.AsWritable<UserAccount>();
                owner.SecurityOwnerOf.Remove(wf.As<Resource>());
                owner.Save();
                
                run = TestBase.RunWorkflow(wf);
            }

            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunFailed));
            Assert.That(run.ErrorLogEntry, Is.Not.Null);
            Assert.That(run.ErrorLogEntry.Description, Is.StringContaining("owner"));
        }
    }
}
