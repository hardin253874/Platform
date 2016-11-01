// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IEntity internal interface.
	/// </summary>
	/// <remarks>
	///     This interface is explicitly implemented in the Entity class to abstract away the
	///     advanced method calls from the base type.
	/// </remarks>
    public interface IEntityInternal : IEntityInternalGeneric<IEntityRef>, IEntityInternalGeneric<long>, IEntityInternalGeneric<string>
	{
		/// <summary>
		///     Gets or sets the clone option.
		/// </summary>
		/// <value>
		///     The clone option.
		/// </value>
		CloneOption CloneOption
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the clone source.
		/// </summary>
		/// <value>
		///     The clone source.
		/// </value>
		long? CloneSource
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether the entity is read-only or not.
		/// </summary>
		/// <value>
		///     <c>true</c> if the entity is read-only; otherwise, <c>false</c>.
		/// </value>
		bool IsReadOnly
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is temporary id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is temporary id; otherwise, <c>false</c>.
		/// </value>
		bool IsTemporaryId
		{
			get;
		}

		/// <summary>
		///     Gets or sets the entities modification token.
		/// </summary>
		/// <value>
		///     The modification token.
		/// </value>
		IEntityModificationToken ModificationToken
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the mutable id.
		/// </summary>
		IMutableIdKey MutableId
		{
			get;
			set;
		}

		/// <summary>
		///     Loads the current entity with the specified values.
		/// </summary>
		/// <param name="id">The entity identifier.</param>
		void Load( long id );        

		/// <summary>
		///     Set the type Ids and the isOfType Relationship
		/// </summary>
        void SetTypeIds( IEnumerable<long> typeIds, bool entityIsReadOnly );

        /// <summary>
        /// Get the invariant upgrade GUID.
        /// </summary>
        Guid UpgradeId { get; }
	}
}