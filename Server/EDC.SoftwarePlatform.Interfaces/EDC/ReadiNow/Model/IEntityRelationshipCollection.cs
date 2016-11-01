// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Covariant interface representing an entity relationship collection.
	/// </summary>
	[SuppressMessage( "Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix" )]
	public interface IEntityRelationshipCollection : IChangeTrackerAccessor<IMutableIdKey>
	{
	}

	/// <summary>
	///     IEntityRelationshipCollection interface.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	public interface IEntityRelationshipCollection<TEntity> : ICollection<TEntity>, IEntityRelationshipCollection
		where TEntity : class, IEntity
	{
		/// <summary>
		///     Gets the entities.
		/// </summary>
		/// <value>
		///     The entities.
		/// </value>
		IEntityCollection<TEntity> Entities
		{
			get;
		}
	}
}