// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.Common.Workflow;
using NUnit.Framework;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;


namespace EDC.SoftwarePlatform.Activities.Test.Triggers
{
    /// <summary>
    /// Test to ensure that creating, deleting and modifying triggers adds the appropriate hooks to the types.
    /// </summary>
    [TestFixture]
    public class RecursionLimit : TriggerTestBase 
    {
        [Test]
        [RunAsDefaultTenant]
        [MaxTime(60000)]
        public void RecurionsLimited()
        {
            var wf = CreateEditNameWorkflow();
            wf.Save();
            ToDelete.Add(wf.Id);

            using (new WorkflowRunContext {RunTriggersInCurrentThread = true})
            {
                var myType = CreateType("recursionLimit_type", null);
                ToDelete.Add(myType.Id);

                //var myType = Entity.Get<EntityType>("test:employee");

                var myTrigger = CreateTrigger("recursionLimit_trigger", myType, wf);
                ToDelete.Add(myTrigger.Id);

                //Thread.Sleep(3000);

                using (new WorkflowRunContext(true))
                {
                    var newEntity = Entity.Create(myType);
                    newEntity.Save();
                    ToDelete.Add(newEntity.Id);
                }
               // Thread.Sleep(20000);

                //myTrigger.Delete(); // make sure the trigger is dead.
                myType.Delete();
            }

            Assert.AreEqual(WorkflowTriggerHelper.MaxTriggerDepth + 1, wf.RunningInstances.Count(), "Trigger depth should not be exceeded.");
            Assert.AreEqual(WorkflowTriggerHelper.MaxTriggerDepth, wf.RunningInstances.Count(ri => ri.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunCompleted), "Maximum allowed Workflows completed successfully.");
            Assert.AreEqual(1, wf.RunningInstances.Count(ri => ri.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunFailed), "Exceeded workflow should fail.");
        }

        [Test]
        [RunAsDefaultTenant]
        public void ForeachTriggerDepth_27722()
        {
            var myType = new EntityType { Name = "myType" + DateTime.Now.Ticks };
            myType.Save();
            ToDelete.Add(myType.Id);
                
            for (var i = 0; i<10; i++)
            {
                Entity.Create(myType.Id).Save();
            }

            var child = CreateLoggingWorkflow();
            child.Save();
            ToDelete.Add(child.Id);

            var foreachWf = CreateForeachWorkflow(myType, child);
            foreachWf.Save();
            ToDelete.Add(foreachWf.Id);

            var run = RunWorkflow(foreachWf);

            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));

            var childRuns = run.ChildRuns;

            Assert.That(childRuns.Count, Is.GreaterThan(0));

            foreach (var cr in childRuns)
            {
                Assert.That(cr.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));
                Assert.That(cr.TriggerDepth, Is.EqualTo(1));
            }
        }


        protected Workflow CreateForeachWorkflow(EntityType myType, Workflow child)
        {
            var workflow = new Workflow
            {
                Name = "Foreach" +  DateTime.Now
            };

            workflow.AddDefaultExitPoint()
                .AddForEach("Foreach", "all('" + myType.Name + "')", ActivityType.ActivityType_Type)
                .AddWorkflowProxy("proxy", child, "Foreach", "Loop")
                .AddTransition("proxy", "Foreach");

            return workflow;
        }
    }
}
