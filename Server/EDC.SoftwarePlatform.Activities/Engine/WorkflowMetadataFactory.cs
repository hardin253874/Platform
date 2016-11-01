// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.Engine
{
    public class WorkflowMetadataFactory : IWorkflowMetadataFactory
    {
        public WorkflowMetadata Create(Workflow wf)
        {
            var result = new WorkflowMetadata(wf);

            var nextActivity = wf.Cast<WfActivity>().CreateWindowsActivity();
            nextActivity.Validate(result);

            return result;
        }
    }
}
