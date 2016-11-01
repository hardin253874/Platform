// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Collections.Generic;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Resources;
using EDC.ReadiNow.Scheduling.iCalendar;
using EDC.ReadiNow.Scheduling.iCalendar.Email;

namespace EDC.ReadiNow.Model.EventClasses
{

	/// <summary>
	/// A abstract class that filters a target and only runs if the fields have changed.
	/// </summary>
	public abstract class FilteredTarget : IEntityEventSave
	{
        const string CHANGED_ENTITIES_KEY = "changed_entities";

        protected abstract HashSet<long> GetWatchedFields();
        protected abstract HashSet<long> GetWatchedForwardRels();
        protected abstract HashSet<long> GetWatchedReverseRels();

        /// <summary>
        ///     Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>
        ///     True to cancel the save operation; false otherwise.
        /// </returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            if (entities == null)
                return false;

            var changedEntities = entities.Where(e => HasUpdates(e)).ToList();
            state[CHANGED_ENTITIES_KEY] = changedEntities;

			if ( changedEntities.Count > 0 ) 
            {
                return FilteredOnBeforeSave(changedEntities, state);
            }
            else 
            {
                return false;
            }
        }

        protected abstract bool FilteredOnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state);

		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			if ( entities == null )
				return;

            var changedEntites = (IEnumerable<IEntity>) state[CHANGED_ENTITIES_KEY];

            FilteredOnAfterSave(changedEntites, state);
		}

        protected abstract void FilteredOnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state);


		/// <summary>
		///     Determines whether the entities watched fields and rels have changed.
		/// </summary>
		private bool HasUpdates( IEntity entity )
		{
            var entityInternal = (IEntityInternal) entity;

            IEntityFieldValues fields;
			IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships;
			IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships;

			/////
			// Get the changes to the event email.
			/////
			Entity.GetChanges( entityInternal.ModificationToken, out fields, out forwardRelationships, out reverseRelationships );

			if ( fields != null )
			{
                var watchedFields = GetWatchedFields();
                var original = Entity.Get<Resource>(entity.Id);      

				/////
				// Look for any field that is specified as one of the update fields and whose value differs from the original
				/////
                if (fields.GetPairs().Where(pair => watchedFields.Contains(pair.Key)).Any(pair => !Equals(original.GetField(pair.Key), pair.Value)))
				{
					return true;
				}
			}

			if ( forwardRelationships != null )
			{
                var forwardRelationshipsOfInterest = GetWatchedForwardRels();

				/////
				// Look for any relationship that is specified as one of the update relationships and whose
				// change tracker indicates there is an outstanding change.
				/////
				IEnumerable<long> forwardRelationshipKeys = forwardRelationships.Keys.Intersect( forwardRelationshipsOfInterest );

				if ( forwardRelationshipKeys.Any( forwardRelationshipKey => 
                {
                    var changeRecord =forwardRelationships[forwardRelationshipKey];
                    return changeRecord == null || changeRecord.IsChanged || changeRecord.Flushed;
                }) )
				{
					return true;
				}
			}

            if (reverseRelationships != null)
            {
                var reverseRelationshipsOfInterest = GetWatchedReverseRels();

                /////
                // Look for any relationship that is specified as one of the update relationships and whose
                // change tracker indicates there is an outstanding change.
                /////
                IEnumerable<long> reverseRelationshipKeys = reverseRelationships.Keys.Intersect(reverseRelationshipsOfInterest);

                if (reverseRelationshipKeys.Any(reverseRelationshipKey => 
                {
                    var changeRecord = reverseRelationships[reverseRelationshipKey];
                    return changeRecord == null || changeRecord.IsChanged || changeRecord.Flushed;
                } ))
                {
                    return true;
                }
            }

			return false;
		}
	}
}