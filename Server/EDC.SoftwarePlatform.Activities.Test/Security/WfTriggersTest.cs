// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.SoftwarePlatform.Activities.Test.Triggers;
using EDC.ReadiNow.Common.Workflow;

    

namespace EDC.SoftwarePlatform.Activities.Test.Security
{
    [TestFixture]
    //[Category("ExtendedTests")]
    [Category("WorkflowTests")]
    public class WfTriggersTest : TriggerTestBase
    {
        [Test]
        [RunAsDefaultTenant]
        [Description("This test tests that triggered WF can run as a non admin user.")]
        // CANNOT RUN THIS AS A TRANSACTION
        public void WfTriggersAsNonAdmin()
        {
            var myType = CreateType("CreateTriggerAddsAndRemovesTypeHook_type", UserResource.UserResource_Type);
            var myWorkflow = CreateWorkflow("CreateTriggerAddsAndRemovesTypeHook_workflow");
            var myTrigger = CreateTrigger("CreateTriggerAddsAndRemovesTypeHook_trigger", myType, myWorkflow);

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                myType.AsWritable();
                myType.Name = "Creatable";
                myType.IsOfType.Add(Resource.Resource_Type);
                myType.Save();

                var myUser = Entity.Create<UserAccount>();
                myUser.Name = "Bob" + DateTime.Now;
                myUser.Save();
                ToDelete.Add(myUser.Id);

                new AccessRuleFactory().AddAllowByQuery(
                myUser.As<Subject>(),
                Resource.Resource_Type.As<SecurableEntity>(),
                new EntityRef("core:create").ToEnumerable(),
                TestQueries.EntitiesWithName("Creatable").ToReport());

                using (new SetUser(myUser))
                {
                    var e = Entity.Create(myType).As<Resource>();
                    e.Name = "MyName";
                    e.CreatedDate = DateTime.Now;
                    e.CreatedBy = myUser;
                    e.SecurityOwner = myUser;
                    e.Save();
                    ToDelete.Add(e.Id);
                }
            }

            var wf = Entity.Get<Workflow>(myWorkflow);
            var failures = wf.RunningInstances.Count(r => r.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunCompleted);
            Assert.That(wf.RunningInstances.Count, Is.EqualTo(1), "It ran");
            Assert.That(wf.RunningInstances.First().WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted, "It ran without errors");
        }
    }
}
