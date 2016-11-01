// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     Forward Relationship.
	/// </summary>
	public class ForwardRelationship : Relationship
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ForwardRelationship" /> class.
		/// </summary>
		/// <param name="databaseManager">The database manager.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="typeUpgradeId">The type upgrade identifier.</param>
		/// <param name="type">The type.</param>
		/// <param name="toId">To identifier.</param>
		/// <param name="toUpgradeId">To upgrade identifier.</param>
		/// <param name="to">To.</param>
		/// <param name="typeDescription">The type description.</param>
		/// <param name="description">The description.</param>
		public ForwardRelationship( DatabaseManager databaseManager, long tenantId, long typeId, Guid typeUpgradeId, string type, long toId, Guid toUpgradeId, string to, string typeDescription, string description )
			: base( databaseManager, tenantId, typeId, typeUpgradeId, type, typeDescription, description )
		{
			ToId = toId;
			ToUpgradeId = toUpgradeId;
			To = to;


			NavigateToCommand = new DelegateCommand( NavigateToClick );
		}

		/// <summary>
		///     Gets or sets the navigate command.
		/// </summary>
		/// <value>
		///     The navigate command.
		/// </value>
		public ICommand NavigateToCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets to.
		/// </summary>
		/// <value>
		///     To.
		/// </value>
		public string To
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets to identifier.
		/// </summary>
		/// <value>
		///     To identifier.
		/// </value>
		public long ToId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets to upgrade identifier.
		/// </summary>
		/// <value>
		///     To upgrade identifier.
		/// </value>
		public Guid ToUpgradeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tool tip.
		/// </summary>
		/// <value>
		///     The tool tip.
		/// </value>
		public string Tooltip
		{
			get
			{
				if ( string.IsNullOrEmpty( Description ) )
				{
					return string.Format( "Tenant Id: {0}\nType Id: {1}\nTo Id: {2}", TenantId, TypeId, ToId );
				}

				return string.Format( "{0}\n\nTenant Id: {1}\nType Id: {2}\nTo Id: {3}", Description, TenantId, TypeId, ToId );
			}
		}

		/// <summary>
		///     Gets the type tool tip.
		/// </summary>
		/// <value>
		///     The type tool tip.
		/// </value>
		public string TypeTooltip
		{
			get
			{
				if ( string.IsNullOrEmpty( TypeDescription ) )
				{
					return string.Format( "Tenant Id: {0}\nType Id: {1}\nTo Id: {2}", TenantId, TypeId, ToId );
				}

				return string.Format( "{0}\n\nTenant Id: {1}\nType Id: {2}\nTo Id: {3}", TypeDescription, TenantId, TypeId, ToId );
			}
		}

		/// <summary>
		///     Navigates the click.
		/// </summary>
		private void NavigateToClick( )
		{
			EntityNotificationService.NavigateTo( ToId.ToString( CultureInfo.InvariantCulture ), TenantId );
		}
	}
}