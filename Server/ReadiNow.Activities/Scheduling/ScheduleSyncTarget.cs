// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using System.Linq;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    /// <summary>
    /// Target for entity messages used to syncronise the scheduling 
    /// </summary>
	public class ScheduleSyncTarget : IEntityEventSave, IEntityEventDelete
    {
        private const string idsKey = "ScheduledItemsIds";

		/// <summary>
		///     Called after deletion of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before delete and after delete callbacks.</param>
		public void OnAfterDelete( IEnumerable<long> entities, IDictionary<string, object> state )
		{
		    var itemsToUnschedule = (List<long>) state[idsKey];
			
		    foreach (var entityId in itemsToUnschedule)
		    {
                SchedulingSyncHelper.DeleteScheduledJob(entityId, SchedulingHelper.Instance);
		    }
		}

		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
		    if (entities == null)
		    {
		        return;
		    }

		    foreach (var entity in entities)
		    {
		        var schedule = entity.As<Schedule>();

		        if (schedule != null)
		        {
                    foreach (var trigger in schedule.TriggersForSchedule)
                        SchedulingSyncHelper.UpdateScheduledJob(trigger, SchedulingHelper.Instance); 
		        }
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
			var scheduledItemsIds = entities.Select( e => e.As<Schedule>()).SelectMany( sc => sc.TriggersForSchedule).Select(tr => tr.Id);
		    state[idsKey] = scheduledItemsIds.ToList();

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
			return false;
		}
    }
}
