// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Services.Console.WorkflowActions
{
    public interface IWorkflowActionsFactory
    {
        IEnumerable<Workflow> Fetch(ISet<long> typeIds);
    }
}
