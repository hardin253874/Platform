// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
	/// <summary>
	///     ResourceKeyEvent target.
	/// </summary>
	public class ResourceKeyEventTarget : IEntityEventSave
	{
		#region IEntityEventSave

		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state.</param>
		public void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
		}

		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		public bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state )
		{
			ResourceKeyHelper.SaveResourceKeyDataHashes( entities, ResourceKeyHelper.SaveContext.ResourceKey, state );

			return false;
		}

		#endregion IEntityEvent        
	}
}