// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.SoftwarePlatform.Activities.Engine.Context
{
    public class WorkflowMissingOwnerException : WorkflowRunException
    {
        public WorkflowMissingOwnerException()
            : base("Workflow is marked as run as owner but does not have an owner")
        { }
    }
}
