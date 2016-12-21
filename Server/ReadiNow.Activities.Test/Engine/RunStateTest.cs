// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Common.Workflow;

namespace EDC.SoftwarePlatform.Activities.Test.Engine
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class RunStateTest
    {
        [Test]
        public void GetWorkflowDescriptionTest_Normal()
        {

            var workflow = new Workflow() { Name = "wfName" };
            var workflowRun = new WorkflowRun() { WorkflowBeingRun = workflow };

            var runState = new TestRunState(new WorkflowMetadata(workflow), workflowRun);
            runState.CurrentActivity = new WfActivity() { Name = "activity" };

            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining("wfName"));
            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining(workflow.Id.ToString()));
            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining(workflowRun.Id.ToString()));
            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining("activity"));
            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining(runState.CurrentActivity.Id.ToString()));

        }

        [Test]
        public void GetWorkflowDescriptionTest_MissingWorkflow()
        {

            var workflowRun = new WorkflowRun() { };

            var runState = new TestRunState(new WorkflowMetadata(), workflowRun);

            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining(workflowRun.Id.ToString()));
        }


        [Test]
        public void GetWorkflowDescriptionTest_MissingActivity()
        {
            var workflow = new Workflow() { Name = "wfName" };
            var workflowRun = new WorkflowRun() { WorkflowBeingRun = workflow };

            var runState = new TestRunState(new WorkflowMetadata(workflow), workflowRun);

            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining(workflowRun.Id.ToString()));
        }

       [Test]
       public void GetWorkflowDescriptionTest_Empty()
        {
            var run = new WorkflowRun();
            var runState = new TestRunState(new WorkflowMetadata(), run);

            Assert.That(runState.GetSafeWorkflowDescription(), Is.StringContaining(run.Id.ToString()));
        }

        [Test]
        [RunWithTransaction]
        public void EffectiveUserIsTriggeringUser()
        {
            var triggerer = new UserAccount();
            triggerer.Name = "foo" + Guid.NewGuid().ToString();
            triggerer.Save();

            using (var setUser = new SetUser(triggerer))
            {
                var run = new WorkflowRunDeferred(new Workflow());
                var runState = new TestRunState(new WorkflowMetadata(), run);

                Assert.That(runState.EffectiveSecurityContext.Identity, Is.Not.Null);
                Assert.That(runState.EffectiveSecurityContext.Identity.Id, Is.EqualTo(triggerer.Id));
                Assert.That(runState.EffectiveSecurityContext.SecondaryIdentity, Is.Null);

            }
        }

        [Test]
        [RunWithTransaction]
        public void SecondaryIdentityIsTriggeringWhenWfIsRunAsOwner()
        {
            var triggerer = new UserAccount();
            triggerer.Name = "foo" + Guid.NewGuid().ToString();
            triggerer.Save();

            var owner = new UserAccount();
            owner.Name = "foo2" + Guid.NewGuid().ToString();
            owner.Save();

            var wf = new Workflow() { SecurityOwner = owner, WorkflowRunAsOwner = true };
            wf.Save();

            using (var setUser = new SetUser(triggerer))
            {
                using (new SecurityBypassContext())
                {
                    var metadata = new WorkflowMetadata(wf);

                    var run = new WorkflowRunDeferred(new Workflow());
                    var runState = DefaultRunStateFactory.Singleton.CreateRunState(metadata, run);
                    //var runState = new TestRunState(metadata, run);

                    Assert.That(runState.EffectiveSecurityContext.Identity, Is.Not.Null);
                    Assert.That(runState.EffectiveSecurityContext.Identity.Id, Is.EqualTo(owner.Id));
                    Assert.That(runState.EffectiveSecurityContext.SecondaryIdentity, Is.Not.Null);
                    Assert.That(runState.EffectiveSecurityContext.SecondaryIdentity.Id, Is.EqualTo(triggerer.Id));
                }
            }
        }
    }
}
