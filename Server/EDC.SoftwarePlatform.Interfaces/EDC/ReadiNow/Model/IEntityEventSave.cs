// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Interface for specifying save events.
	/// </summary>
	public interface IEntityEventSave : IEntityEvent
	{
		/// <summary>
		///     Called after saving of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		void OnAfterSave( IEnumerable<IEntity> entities, IDictionary<string, object> state );

		/// <summary>
		///     Called before saving the enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before save and after save callbacks.</param>
		/// <returns>
		///     True to cancel the save operation; false otherwise.
		/// </returns>
		bool OnBeforeSave( IEnumerable<IEntity> entities, IDictionary<string, object> state );
	}
}