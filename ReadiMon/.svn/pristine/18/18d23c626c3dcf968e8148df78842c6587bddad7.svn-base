// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	/// Entity Notification Service class.
	/// </summary>
	public static class EntityNotificationService
	{
		/// <summary>
		///     Navigates the specified identifier.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		public static void NavigateTo( string id, long tenantId )
		{
			var evt = Navigate;

			if ( evt != null )
			{
				evt( null, new EntityInfoEventArgs( id, tenantId ) );
			}
		}

		/// <summary>
		///     Occurs when a navigation event occurs.
		/// </summary>
		public static event EventHandler<EntityInfoEventArgs> Navigate;
	}
}