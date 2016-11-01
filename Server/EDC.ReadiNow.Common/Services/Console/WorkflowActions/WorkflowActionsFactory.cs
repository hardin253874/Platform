// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Services.Console.WorkflowActions
{
    /// <summary>
    /// Factory to find all the workflows that can be applied as actions to this type.
    /// </summary>
    public class WorkflowActionsFactory: IWorkflowActionsFactory
    {
        /// <summary>
        /// Fetch the workflows that can be actions for the given types
        /// </summary>
        /// <param name="typeIDs"></param>
        /// <returns>The applicable workflows</returns>
        public IEnumerable<Workflow> Fetch(ISet<long> typeIDs)
        {
            var workflowType = Factory.EntityRepository.Get<EntityType>("core:workflow", ActionServiceHelpers.WorkflowRequest);

            IEnumerable<Workflow> workflows =
                (from instance in workflowType.InstancesOfType
                 let workflow = instance.As<Workflow>()
                 let arg = workflow.InputArgumentForAction
                 let typedArg = arg == null ? null : arg.As<TypedArgument>()
                 where typedArg != null
                 where typedArg.ConformsToType != null && typeIDs.Contains(typedArg.ConformsToType.Id)
                 where workflow.WfNewerVersion == null
                 select workflow
                ).Distinct()
                .OrderBy(wf => wf.Name);

            return workflows;
        }

    }
}
