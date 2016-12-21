// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Common.Workflow;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.Activities.Test.Triggers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Test.Bugs
{
    [TestFixture]
    public class Bug_25271_triggerFiringTooOften : TriggerTestBase
    {
        [Test]
        [RunAsDefaultTenant]
        public void MultipleTriggerProblem()
        {
            var wf = CreateLoggingWorkflow();
            wf.Save();
            ToDelete.Add(wf.Id);

            var myType = CreateType("triggerTooMany_type", Person.Person_Type);
            ToDelete.Add(myType.Id);

            var myTrigger = CreateTrigger("triggerTooMany_type", myType, wf);
            ToDelete.Add(myTrigger.Id);

            using (new WorkflowRunContext { RunTriggersInCurrentThread = true })
            {
                var newEntity = Entity.Create(myType);
                newEntity.Save();
                ToDelete.Add(newEntity.Id);
            }

            Assert.AreEqual(1, wf.RunningInstances.Count(), "Only one run should be triggered");
        }
    }
}
