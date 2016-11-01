// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Diagnostics.CodeAnalysis;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Defines an interface for any object that can be used to point to an entity.
	/// </summary>
	/// <remarks>
	///     The class must be able to: resolve the entity's Id, and resolve the entity itself.
	///     Note that IEntity itself satisfies this interface, because an entity knows about itself.
	///     The purpose of this interface is to allow a single set of APIs to be defined, which can then be provided with
	///     different implementations on this interface depending on what the user has at hand, whether an entity, an alias, an ID, a guid.
	/// </remarks>
	public interface IEntityRef
	{
		/// <summary>
		///     Gets the alias.
		/// </summary>
		[SuppressMessage( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords" )]
		string Alias
		{
			get;
		}

		/// <summary>
		///     The Entity being referenced.
		/// </summary>
		IEntity Entity
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has entity.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has entity; otherwise, <c>false</c>.
		/// </value>
		bool HasEntity
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has id.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has id; otherwise, <c>false</c>.
		/// </value>
		bool HasId
		{
			get;
		}

		/// <summary>
		///     The Id of the entity being referenced.
		/// </summary>
		long Id
		{
			get;
		}

		/// <summary>
		///     Gets the namespace.
		/// </summary>
		[SuppressMessage( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords" )]
		string Namespace
		{
			get;
		}
	}
}