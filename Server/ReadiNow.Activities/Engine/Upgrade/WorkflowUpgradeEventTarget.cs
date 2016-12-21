// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities.Engine.Upgrade
{
    /// <summary>
    /// Deals with workflow upgrades during an upgrade.
    /// </summary>
    public class WorkflowUpgradeEventTarget : IEntityEventUpgrade
    {
        string stateKeyClones = "WorkflowupgradeEventTarget_Clones";
        string stateKeyClonedIds = "WorkflowupgradeEventTarget_ClonedIds";
        string stateKeyHashes = "WorkflowupgradeEventTarget_Hashes";
        string stateKeyVersions = "WorkflowupgradeEventTarget_Versions";

        public bool OnBeforeUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            IEnumerable<Workflow> wfsToWorryAbout = new HashSet<Workflow>();
            var clones = new Dictionary<long, Workflow>();
            var updateHashes = new Dictionary<long, string>();
            var versions = new Dictionary<long, int>();

            var workflows = Entity.GetInstancesOfType<Workflow>(false);

            // find any paused workflows runs, clone the workflows just in case.
            foreach (var entity in entities)
            {
                var solution = entity.As<Solution>();

                var wfs = workflows.Where(w => 
						w.InSolution != null &&
						solution != null &&
                        w.InSolution.Id == solution.Id && 
                        w.RunningInstances.Any(r => r.WorkflowRunStatus_Enum == WorkflowRunState_Enumeration.WorkflowRunPaused));

                wfsToWorryAbout = wfsToWorryAbout.Union(wfs);
            }
            
            foreach (var wf in wfsToWorryAbout.Distinct())
            {
                var clone = wf.Clone<Workflow>();               // NEED TO USE WORKING CLONE

                clones.Add(wf.Id, clone);
            }

            updateHashes = workflows.ToDictionary(wf => wf.Id, wf => wf.WorkflowUpdateHash);
            versions = workflows.ToDictionary(wf => wf.Id, wf => wf.WorkflowVersion ?? 1);

            var clonedIds = Entity.Save(clones.Values);                                // We need to do a save to ensure all the information is copied across. We can't rely on the in memory version

            state[stateKeyClones] = clones;
            state[stateKeyClonedIds] = clonedIds;
            state[stateKeyHashes] = updateHashes;
            state[stateKeyVersions] = versions;

            return false;
        }

        public void OnAfterUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var clones = (Dictionary<long, Workflow>)state[stateKeyClones];
            var clonedIds = (Dictionary<long, long>)state[stateKeyClonedIds];
            var updateHashes = (Dictionary<long, string>)state[stateKeyHashes];
            var versions = (Dictionary<long, int>)state[stateKeyVersions];

            var toSave = new List<Workflow>();     // We need to keep these saves separately because otherwise the running instances relationship save gets confused.
            var toDelete = new List<Workflow>();

            foreach (var kvp in updateHashes)
            {
                var original = Entity.Get<Workflow>(kvp.Key);

                Workflow clone = null;
                clones.TryGetValue(kvp.Key, out clone);

                if (original.WorkflowUpdateHash != updateHashes[kvp.Key]) // we have an update
                {
                    var originalWritable = original.AsWritable<Workflow>();
                    originalWritable.WorkflowVersion = versions[kvp.Key] + 1;
                    toSave.Add(originalWritable);

                    if (clone != null) 
                    {
                        WorkflowRunHelper.MoveRuns(original, clone, clonedIds);

                        //originalWritable.RunningInstances.Clear();
                        originalWritable.WfOlderVersion = clone;

                        var running = original.RunningInstances.ToList();

                        var editableClone = clone.AsWritable<Workflow>();
                        editableClone.RunningInstances.AddRange(running);

                        toSave.Add(editableClone);
                    }
                }
                else // nothing has changed, clean up
                {
                    if (clone != null)
                        toDelete.Add(clone);
                }
            }

            // We need to save the originals and clones separately otherwise cascade deletes seem to blow away all the running instances.
			if ( toSave.Count > 0 )
            {
                Entity.Save(toSave);
            }

			if ( toDelete.Count > 0 )
            {
                Entity.Delete(toDelete.Select(e=>e.Id));
            }

            state.Remove(stateKeyClones);
            state.Remove(stateKeyHashes);

        }



        /// <summary>
        /// Called if a failure occurs deploying an application
        /// </summary>
        /// <param name="solutions">The solutions.</param>
        /// <param name="state">The state.</param>
        public void OnDeployFailed(IEnumerable<ISolutionDetails> solutions, IDictionary<string, object> state)
        {            
        }
    }
}
