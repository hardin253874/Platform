// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Updates mail boxes on the provider whenever the mail box is saved.
    /// </summary>
    public class InboxTarget : IEntityEventSave, IEntityEventDelete, IEntityEventDeploy, IEntityEventUpgrade
    {
        private const string DeleteListKey = "DeleteList";

        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // do nothing
        }

        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
             var tenant = RequestContext.GetContext().Tenant;
             var tenantName = tenant != null ? tenant.Name : SpecialStrings.GlobalTenant;

            // TODO: we need to deal with changing the name of the mail box or the provider.
            foreach (var entity in entities)
            {
                var inbox = entity.Cast<Inbox>();
                if (inbox.UsesInboxProvider != null)
                {
                    var inboxProviderHelper = inbox.UsesInboxProvider.GetHelper();
                    inbox.InboxEmailAddress = inboxProviderHelper.NameToEmailAddress(tenantName, inbox.Name);
                }
            }

            return false;
        }

        public void OnAfterDeploy(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
			var tenant = RequestContext.GetContext().Tenant;
            var tenantName = tenant.Name;
            var tenantId = tenant.Id;

            foreach (IEntity entity in entities)
            {
                var solution = entity.Cast<Solution>();

                foreach (var inbox in Entity.GetInstancesOfType<Inbox>(true, "inSolution.isOfType.id")) // ok to use this rather than a report as there are unlikely to be many inboxes in the system
                {
                    if (inbox.InSolution != null && inbox.InSolution.Id == solution.Id && inbox.UsesInboxProvider != null)
                    {
                        EventLog.Application.WriteTrace(string.Format("Post install updating updating inbox '{0}' for Tenant '{1}' ({2}).", inbox.Name, tenantName, tenantId));
                        var writableInbox = inbox.AsWritable<Inbox>();
                        var inboxProviderHelper = writableInbox.UsesInboxProvider.GetHelper();
                        inboxProviderHelper.CreateInbox(tenantName, writableInbox);
                        writableInbox.InboxEmailAddress = inboxProviderHelper.NameToEmailAddress(tenantName, writableInbox.Name);
                        writableInbox.Save();
                    }
                }
            }
        }

        public void OnCreate(IEntity entity)
        {
            
        }


        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            var tenant = RequestContext.GetContext().Tenant;
            var tenantName = tenant.Name;
            var tenantId = tenant.Id;

            foreach (Inbox inbox in entities.Cast<Inbox>())
            {
                if (inbox.UsesInboxProvider != null)
                {
                    try
                    {
                        var inboxProviderHelper = inbox.UsesInboxProvider.GetHelper();
                        inboxProviderHelper.DeleteInbox(tenantName, inbox);
                    }
                    catch (Exception ex)
                    {
                        EventLog.Application.WriteError(string.Format("Failed to delete inbox '{0}' for Tenant '{1}' ({2}).\nException:\n{3}", inbox.Name, tenantName, tenantId, ex.Message));                        
                    }
                    
                }
            }

            return false;
        }

        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {
            // do nothing
        }

        public void OnAfterUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            OnAfterDeploy(entities, state);
        }

        public bool OnBeforeUpgrade(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
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
