// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     EntityInfoEventArgs class.
	/// </summary>
	public class EntityInfoEventArgs : EventArgs
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityInfoEventArgs" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		public EntityInfoEventArgs( string id, long tenantId )
		{
			Id = id;
			TenantId = tenantId;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public string Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}
	}
}