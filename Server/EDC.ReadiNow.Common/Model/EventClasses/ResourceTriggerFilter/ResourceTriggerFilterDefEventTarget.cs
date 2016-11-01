// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Model.EventClasses.ResourceTriggerFilter
{
    /// <summary>
    /// Keep any resource trigger filters in sync
    /// </summary>
    public class ResourceTriggerFilterDefEventTarget : IEntityEventSave, IEntityEventDelete, IEntityEventDeploy
    {
        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }
        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            var policyCache = Factory.ResourceTriggerFilterPolicyCache;

            foreach (var entity in entities)
            {
                policyCache.RemovePolicy(entity);
            }
        }

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var policyCache = Factory.ResourceTriggerFilterPolicyCache;

            foreach (var entity in entities)
            {
                policyCache.UpdateOrAddPolicy(entity.Cast<ResourceTriggerFilterDef>());
            }
        }


        public void OnDeployFailed(IEnumerable<ISolutionDetails> solutions, IDictionary<string, object> state)
        {
        }

        public void OnAfterDeploy(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var policyCache = Factory.ResourceTriggerFilterPolicyCache;

            foreach (var entity in entities)
            {
                policyCache.UpdateOrAddPolicy(entity.Cast<ResourceTriggerFilterDef>());
            }
        }

    }
}
