// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Globalization;
using EDC.Exceptions;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Provide additional functionality for dealing with superseded workflows
    /// </summary>
    public static class WorkflowUpdateHelper
    {
        static Random _randGen = new Random();

        /// <summary>
        /// Update a workflow using a update action. This will take care of handling paused instances by cloning.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="updateAction"></param>
        /// <returns>True if the workflow needed to be clones</returns>
        public static bool Update(long workflowId, Action updateAction)
        {
            using (new SecurityBypassContext(true))
            {
                Workflow clone = null;
                var original = Entity.Get<Workflow>(workflowId);

                if (original == null)
                    throw new WebArgumentException("The provided Workflow Id is not a workflow");

                if (original.WfNewerVersion != null)
                    throw new WebArgumentException("This Workflow is not the latest version so cannot be updated");

                using (var databaseContext = DatabaseContext.GetContext(true))
                {
                    var havePausedInstances = original.RunningInstances.Any(r => r.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused);
                    IDictionary<long, long> clonedIds = null;

                    if (havePausedInstances)
                    {
                        // clone the workflow (Need to save because the clone occurs in the DB)
                        clone = original.Clone<Workflow>();
                        clonedIds = clone.Save();

                        // There seems to be a bug in entity clone that means you can't remove solution till after it has been saved.
                        clone = clone.AsWritable<Workflow>();
                        clone.InSolution = null;

                        // Fix up the version linking - again we can't seem to do it as a single save.
                        var originalWfOlderVersionId = original.WfOlderVersion != null ? original.WfOlderVersion.Id : -1;

                        original = original.AsWritable<Workflow>();
                        original.WfOlderVersion = clone;
                        original.Save();

                        if (originalWfOlderVersionId != -1)
                            clone.WfOlderVersion = Entity.Get<Workflow>(originalWfOlderVersionId);

                        clone.Save();
                    }
                    
                    SecurityBypassContext.RunAsUser(updateAction);

                    original = original.AsWritable<Workflow>();

                    original.WorkflowUpdateHash = _randGen.Next().ToString(CultureInfo.InvariantCulture);     // ideally this should be a hash of only the important aspects of the workflow
                    original.WorkflowVersion = (original.WorkflowVersion ?? 1) + 1;

                    if (havePausedInstances)
                    {
                        original.WfOlderVersion = clone;
                        WorkflowRunHelper.MoveRuns(original, clone, clonedIds);
                    }

                    original.Save();    // This will also save the close

                    databaseContext.CommitTransaction();

                    return havePausedInstances;
                }
            }

        }

       
    }
}
