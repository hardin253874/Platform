// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     IEntity interface.
	/// </summary>
	public interface IEntity : IEntityRef, IEntityGeneric<IEntityRef>, IEntityGeneric<long>, IEntityGeneric<string>, IDisposable
	{
		/// <summary>
		///     Gets the types that the entity implements.
		/// </summary>
		/// <value>
		///     The entities types.
		/// </value>
		IEnumerable<IEntity> EntityTypes
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		bool IsReadOnly
		{
			get;
		}

		/// <summary>
		///     Gets the tenant unique identifier.
		/// </summary>
		/// <value>
		///     The tenant unique identifier.
		/// </value>
		long TenantId
		{
			get;
		}

		/// <summary>
		///     Gets the entity type identifiers.
		/// </summary>
		IEnumerable<long> TypeIds
		{
			get;
		}

		/// <summary>
		///     Converts the current entity to the specified type if possible.
		/// </summary>
		/// <typeparam name="T">Destination type to convert to.</typeparam>
		/// <returns>
		///     The converted entity if allowed, null otherwise.
		/// </returns>
		[SuppressMessage( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords" )]
		T As<T>( ) where T : class, IEntity;

		/// <summary>
		///     Gets a writable version of the current instance.
		/// </summary>
		/// <typeparam name="T">The expected type of the returned entity.</typeparam>
		/// <returns>
		///     A writable version of the current instance.
		/// </returns>
		T AsWritable<T>( ) where T : class, IEntity;

		/// <summary>
		///     Gets a writable version of the current instance.
		/// </summary>
		/// <returns>
		///     A writable version of the current instance.
		/// </returns>
		IEntity AsWritable( );

		/// <summary>
		///     Casts this instance.
		/// </summary>
		/// <typeparam name="T">Destination type to convert to.</typeparam>
		/// <returns>
		///     The converted entity if possible, throws an exception otherwise.
		/// </returns>
		T Cast<T>( ) where T : class, IEntity;

		/// <summary>
		///     Clones this instance (Deep copy).
		/// </summary>
		/// <returns>
		///     An in memory clone of the current entity.
		/// </returns>
		IEntity Clone( );

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <param name="cloneOption">The option.</param>
		/// <returns>
		///     An in memory clone of the current entity.
		/// </returns>
		IEntity Clone( CloneOption cloneOption );

		/// <summary>
		///     Clones this instance (Deep copy).
		/// </summary>
		/// <typeparam name="T">The expected type of the cloned entity.</typeparam>
		/// <returns>
		///     A cloned instance of the current entity.
		/// </returns>
		T Clone<T>( ) where T : class, IEntity;

		/// <summary>
		///     Clones this instance.
		/// </summary>
		/// <typeparam name="T">The expected type of the cloned entity.</typeparam>
		/// <param name="cloneOption">The option.</param>
		/// <returns>
		///     A cloned instance of the current entity.
		/// </returns>
		T Clone<T>( CloneOption cloneOption ) where T : class, IEntity;

		/// <summary>
		///     Deletes this instance.
		/// </summary>
		void Delete( );

		/// <summary>
		///     Determines whether the current entity instance is of the specified type T.
		/// </summary>
		/// <typeparam name="T">The entity type to check.</typeparam>
		/// <returns>
		///     <c>true</c> if [is]; otherwise, <c>false</c>.
		/// </returns>
		[SuppressMessage( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords" )]
		[SuppressMessage( "Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter" )]
		bool Is<T>( ) where T : class, IEntity;

		/// <summary>
		///     Sets this instance.
		/// </summary>
        IDictionary<long, long> Save();

        /// <summary>
		///     Reverts the changes made to this writable entity.
		/// </summary>
		void Undo( );

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
        ///     Gets the Upgrade ID GUID for this entity.
        /// </summary>
        /// <value>The upgrade ID.</value>
        Guid UpgradeId
        {
            get;
        }

        /// <summary>
		///     Has the entity changed?
		/// </summary>
		/// <param name="fieldsAndRelsFilter">An options filter of fields or relationships to be tested for changes. If null, all are checked.</param>
		/// <returns>
		///     <c>true</c> if the specified token has changes; otherwise, <c>false</c>.
		/// </returns>
		bool HasChanges( IEnumerable<IEntityRef> fieldsAndRelsFilter );
	}
}