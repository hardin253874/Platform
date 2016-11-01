// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Engine
{
    public interface IWorkflowMetadataFactory
    {
        WorkflowMetadata Create(Workflow wf);
    }
}
