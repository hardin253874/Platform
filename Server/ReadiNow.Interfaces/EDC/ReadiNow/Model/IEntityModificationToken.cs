// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity Modification Token interface.
	/// </summary>
	public interface IEntityModificationToken
	{
		/// <summary>
		///     Gets or sets the entity id.
		/// </summary>
		/// <value>
		///     The entity id.
		/// </value>
		long EntityId
		{
			get;
		}

		/// <summary>
		///     Gets or sets the modification id.
		/// </summary>
		/// <value>
		///     The modification id.
		/// </value>
		Guid ModificationId
		{
			get;
		}
	}
}