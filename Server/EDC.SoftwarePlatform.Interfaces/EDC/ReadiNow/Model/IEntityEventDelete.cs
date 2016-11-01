// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Interface for specifying delete events.
	/// </summary>
	public interface IEntityEventDelete : IEntityEvent
	{
		/// <summary>
		///     Called after deletion of the specified enumeration of entities has taken place.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before delete and after delete callbacks.</param>
		void OnAfterDelete( IEnumerable<long> entities, IDictionary<string, object> state );

		/// <summary>
		///     Called before deleting an enumeration of entities.
		/// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before delete and after delete callbacks.</param>
		/// <returns>
		///     True to cancel the delete operation; false otherwise.
		/// </returns>
		bool OnBeforeDelete( IEnumerable<IEntity> entities, IDictionary<string, object> state );
	}
}