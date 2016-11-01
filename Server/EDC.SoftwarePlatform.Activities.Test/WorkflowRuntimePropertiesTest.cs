// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Common.Workflow;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    [Category("ExtendedTests")]
    [Category("WorkflowTests")]
    public class WorkflowRuntimePropertiesTest : TestBase
    {
        [Test]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        //[Ignore]        // Cant seem to get this test working in the auto test environment - don't know why
        public void CheckWorkflowOwnerAndTriggererSet()
		{
            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var wfName = "test wf" + DateTime.Now;
                var owner = new UserAccount
                {
                    Name = "TestOwner" + DateTime.Now
                };
                owner.Save();

                new AccessRuleFactory().AddAllowByQuery(
                    owner.As<Subject>(),
                    Workflow.Workflow_Type.As<SecurableEntity>(),
                    Permissions.Read.ToEnumerable(),
                    TestQueries.WorkflowWithName(wfName).ToReport());

                var triggerer = new UserAccount
                {
                    Name = "TestTriggerer" + DateTime.Now
                };
                triggerer.Save();

                new AccessRuleFactory().AddAllowByQuery(
                    triggerer.As<Subject>(),
                    UserAccount.UserAccount_Type.As<SecurableEntity>(),
                    new EntityRef("core:read").ToEnumerable(),
                    TestQueries.EntitiesWithName(owner.Name).ToReport());

                new AccessRuleFactory().AddAllowByQuery(
                    triggerer.As<Subject>(),
                    Workflow.Workflow_Type.As<SecurableEntity>(),
                    Permissions.Read.ToEnumerable(),
                    TestQueries.WorkflowWithName(wfName).ToReport());

                var currentContext = RequestContext.GetContext();

                var wf = new Workflow { Name = wfName };


                //var wfAs = wf.Cast<WfActivity>(); // why?

                wf
                    .AddDefaultExitPoint()
                    .AddOutput<ResourceArgument>("triggerman")
                    .AddOutput<ResourceArgument>("owner")
                    .AddAssignToVar("set out 1", "[Triggering User]", "triggerman")
                    .AddAssignToVar("set out 2", "[Workflow Owner]", "owner")
                    .AddLog("Log", "Owner: {{[Workflow Owner]}}    Triggerer: {{[Triggering User]}}");

                wf.SecurityOwner = owner;
                wf.Save();

                var input = new Dictionary<string, object> { { "wfInInt", 10 } };

                WorkflowRun run;

                using (CustomContext.SetContext(new IdentityInfo(triggerer.Id, owner.Name), currentContext.Tenant, currentContext.Culture))
                {
                    run = (RunWorkflow(wf, input));
                }

                var outputs = run.GetOutput();

                Assert.IsTrue(outputs.ContainsKey("owner"), "Owner was returned");
                Assert.IsTrue(outputs.ContainsKey("triggerman"), "Triggerer was returned");

                Assert.AreEqual(triggerer.Id, ((IEntity)outputs["triggerman"]).Id, "Ensure that the Triggering user is set to the id of the context.");
                Assert.AreEqual(owner.Id, ((IEntity)outputs["owner"]).Id, "The owner is set correctly.");
            }
		}
    }
}