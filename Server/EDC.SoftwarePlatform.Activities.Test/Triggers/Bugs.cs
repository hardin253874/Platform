// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.IO;
using EDC.SoftwarePlatform.Activities.Triggers;
using System.Threading;
using EDC.Threading;
using EDC.Common.Threading;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Common.Workflow;



namespace EDC.SoftwarePlatform.Activities.Test.Triggers
{
    /// <summary>
    /// Bugs
    /// </summary>
    [TestFixture]
    public class Bugs : TriggerTestBase
    {

        [Test]
        [RunAsDefaultTenant]
        [Timeout(120 * 1000)]
        [Description("This test tests that triggered WF runs do not block each other and grind the server to a halt.")]
        // CANNOT RUN THIS AS A TRANSACTION
        public void Bug_GrindsToHalt_22822()
        {
            int createdEntities = 50;
            Workflow myWorkflow;

            using (new WorkflowRunContext() { RunTriggersInCurrentThread = true })
            {
                // **** Note to DQ ******
                // Make sure that WorkflowConfiguration.TriggerSettings.MaxConcurrency is set to 6 (or some number other than one) so that it is running more realistically.
                //

                var myType = CreateType("CreateTriggerAddsAndRemovesTypeHook_type", UserResource.UserResource_Type);
                myWorkflow = CreateWorkflow("CreateTriggerAddsAndRemovesTypeHook_workflow");
                var myTrigger = CreateTrigger("CreateTriggerAddsAndRemovesTypeHook_trigger", myType, myWorkflow);

                ToDelete.Add(myType.Id);
                ToDelete.Add(myWorkflow.Id);
                ToDelete.Add(myTrigger.Id);

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
                    for (int i = 0; i < createdEntities; i++)
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
            }

            var wf = Entity.Get<Workflow>(myWorkflow);
            var runningInstances = wf.RunningInstances;

            Assert.That(runningInstances.Count(), Is.EqualTo(createdEntities));

            var failures = runningInstances.Count(r => r.WorkflowRunStatus_Enum != WorkflowRunState_Enumeration.WorkflowRunCompleted);
            Assert.That(failures, Is.EqualTo(0), "No runs failed");
            
        }

       


    }
}
