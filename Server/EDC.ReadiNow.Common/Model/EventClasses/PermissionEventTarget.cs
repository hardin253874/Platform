// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Model.EventClasses
{
	/// <summary>
	///     Permission events.
	/// </summary>
	public class PermissionEventTarget : IEntityEventSave
	{
		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			if ( entities == null )
			{
				return;
			}
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