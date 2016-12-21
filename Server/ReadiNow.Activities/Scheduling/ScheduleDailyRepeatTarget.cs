// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities.Scheduling
{
    /// <summary>
    /// Target for entity messages used to syncronise the scheduling 
    /// </summary>
    public class ScheduleDailyRepeatTarget : IEntityEventSave
    {
        private const string idsKey = "ScheduledItemsIds";

        /// <summary>
        ///     Called after saving of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
        {
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
            var tz = RequestContext.GetContext().TimeZone;
            var timeZoneField = new EntityRef("core:sdrTimeZone");
            foreach (var entity in entities)
            {
                var field = entity.GetField(timeZoneField);

                if (field == null || (string) field == string.Empty )
                {
                    entity.SetField(timeZoneField, tz);
                }
            }

            return false;
        }
	}
}