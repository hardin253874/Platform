// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.Activities.Api
{
    public class WorkflowRunDataDefinition
    {
        public long EntityId { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
    }
}
