// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// Keep the change markers on the client cachable objects up to date when ever a save or deploy ofccurs.
    /// </summary>
	public class ClientCachableEventTarget : IEntityEventSave
	{
		

		/// <summary>
		///     Update all the change markers on all the forms that are saved
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
            UpdateChangeMarkers(entities);
			return false;
		}

	

		/// <summary>
		/// Update all the change markers after a deployment. This ensures that all cached forms on clients will be invalidated.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state.</param>
		public void OnAfterDeploy( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
            UpdateChangeMarkers(entities);
		}


        private void UpdateChangeMarkers(IEnumerable<IEntity> entities)
        {
            foreach (var cachable in entities.Select(e => e.As<ClientCachable>()))
            {
                cachable.CacheChangeMarker = Guid.NewGuid();
            }
        }


        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            throw new NotImplementedException();
        }
    }
}