// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IEntityCollection interface.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IEntityCollection<TEntity> : ICollection<TEntity>, IChangeTrackerAccessor<IMutableIdKey>
		where TEntity : class, IEntity
	{
		/// <summary>
		///     Gets or sets the entity at the specified index.
		/// </summary>
		/// <value>
		///     The entity.
		/// </value>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		TEntity this[ int index ]
		{
			get;
			set;
		}

		/// <summary>
		///     Adds the range.
		/// </summary>
		/// <param name="collection">The collection.</param>
		void AddRange( IEnumerable<TEntity> collection );

		/// <summary>
		///     Removes the range.
		/// </summary>
		/// <param name="collection">The collection.</param>
		void RemoveRange( IEnumerable<TEntity> collection );
	}
}