// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.Test.Triggers
{
    public class TriggerTestBase: TestBase
    {
        protected EntityType CreateType(string name, EntityType inherits = null)
        {
            var myType = new EntityType() { Name = name };
            if (inherits != null) 
                myType.Inherits.Add(inherits);

            myType.Save();
            ToDelete.Add(myType.Id);
            return myType;
        }

        protected ResourceTriggerFilterDef CreateTrigger(string name, EntityType triggeredOn, Workflow wf, bool enabled = true)
        {
            var trigger = new WfTriggerUserUpdatesResource() 
            { Name = name, 
                TriggerEnabled = enabled, WorkflowToRun = wf, TriggeringCondition = Entity.Get<TriggeredOnEnum>("core:triggeredOnEnumCreateUpdate")};

            if (triggeredOn != null)
                trigger.TriggeredOnType = triggeredOn;

            trigger.Save();
            ToDelete.Add(trigger.Id);

            return trigger.Cast< ResourceTriggerFilterDef>();
        }

        protected Workflow CreateWorkflow(string name)
        {
            var myWorkflow = (new Workflow() { Name = "name" }).AddWfExitPoint("Exit", true).AddLog("Log","Log");
            myWorkflow.Save();
            ToDelete.Add(myWorkflow.Id);
            return myWorkflow;
        }
    }
}