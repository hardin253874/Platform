// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    /// <summary>
    /// This Sychronises the scheduling system with and saved or updated ScheduledItems
    /// </summary>
    public class ScheduledItemSyncTarget : IEntityEventSave, IEntityEventDelete, IEntityEventDeploy, IEntityEventUpgrade
    {
        const string ChangedScheduleKey = "ChangedSchedule";

        /// <summary>
        ///     Called after deletion of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnAfterDelete( IEnumerable<long> entities, IDictionary<string, object> state )
        {
            if ( entities == null )
            {
                return;
            }

            foreach (var entityId in entities)
                SchedulingSyncHelper.DeleteScheduledJob(entityId, SchedulingHelper.Instance);
        }

        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
        {
            var changedSchedules = (List<ScheduledItem>)state[ChangedScheduleKey];

            foreach (var triggerOnSch in changedSchedules)
            {
                SchedulingSyncHelper.UpdateScheduledJob(triggerOnSch, SchedulingHelper.Instance); 
            }
        }

        /// <summary>
        ///     Called before deleting an enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <returns>
        ///     True to cancel the delete operation; false otherwise.
        /// </returns>
        public bool OnBeforeDelete( IEnumerable<IEntity> entities, IDictionary<string, object> state )
        {
            return false;
        }

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
        {
            var schedulingFields = new List<IEntity> { ScheduledItem.TriggerEnabled_Field, ScheduledItem.ScheduleForTrigger_Field };

            var changedSchedules = entities.Select(e =>
            {
                var scheduledItem = e.As<ScheduledItem>();

                if (scheduledItem == null)
                    return null;

                if (scheduledItem.IsTemporaryId && (scheduledItem.TriggerEnabled ?? false) && scheduledItem.ScheduleForTrigger != null)
                    return scheduledItem;

                if (!scheduledItem.IsTemporaryId && scheduledItem.HasChanges(schedulingFields))
                    return scheduledItem;

                return null;
            }).Where(si => si != null).ToList();

            state.Add(ChangedScheduleKey, changedSchedules);

            return false;
        }

        public void OnAfterDeploy(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            // we need to create our own scheduler so we can shut it down after deployment. If we don't do this then during installation
            // error messages will appear in the log.
            var scheduler = SchedulingHelper.Instance;   
            
            var tenant = RequestContext.GetContext().Tenant;
            var tenantName = tenant.Name;
            
            ISet<string> solutionAliases = new HashSet<string>();

            foreach (IEntity entity in entities)
            {
                var solution = entity.Cast<Solution>();

                if (solution != null && 
                    !string.IsNullOrWhiteSpace(solution.Alias))
                {
                    solutionAliases.Add(solution.Alias);
                }

                foreach (var schedItem in Entity.GetInstancesOfType<ScheduledItem>(true, "inSolution.isOfType.id")) // ok to use this rather than a report as there are unlikely to be many inboxes in the system
                {
                    if ( solution != null && schedItem.InSolution != null && schedItem.InSolution.Id == solution.Id )
                    {
                        EventLog.Application.WriteTrace(string.Format("Post install updating updating ScheduledItems '{0}' in Tenant '{1}'.", schedItem.Name, tenantName));
                        SchedulingSyncHelper.UpdateScheduledJob(schedItem, scheduler);
                    }
                }
            }

            ResyncSchedulesForGlobalTenant(solutionAliases, scheduler);

            scheduler.Shutdown(true);       // We need to shutdown the scheduler otherwise deployment won't complete
        }


        /// <summary>
        /// Resync the schedules for the global tenant.
        /// </summary>
        /// <param name="solutionAliases"></param>
        /// <param name="scheduler"></param>
        private void ResyncSchedulesForGlobalTenant(ISet<string> solutionAliases, Quartz.IScheduler scheduler)
        {
            if (solutionAliases.Count == 0)
            {
                return;
            }

            using (new GlobalAdministratorContext())
            {
                foreach (var schedItem in Entity.GetInstancesOfType<ScheduledItem>(true, "inSolution.{isOfType.id, alias}, isOfType.{systemTenantOnly}"))
                {
                    var itemSolutions = new List<string>();

                    if (schedItem.InSolution != null)
                    {
                        itemSolutions.Add(schedItem.InSolution.Alias);
                    }

                    if (solutionAliases.Overlaps(itemSolutions) &&
                        schedItem.IsOfType.Any(t => t.SystemTenantOnly ?? false))
                    {
                        EventLog.Application.WriteTrace(string.Format("Post install updating updating ScheduledItems '{0}' in Global Tenant.", schedItem.Name));
                        SchedulingSyncHelper.UpdateScheduledJob(schedItem, scheduler);
                    }
                }                
            }
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