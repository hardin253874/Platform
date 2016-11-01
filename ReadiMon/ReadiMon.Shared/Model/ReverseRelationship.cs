// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     Reverse Relationship.
	/// </summary>
	public class ReverseRelationship : Relationship
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ReverseRelationship" /> class.
		/// </summary>
		/// <param name="databaseManager">The database manager.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="typeUpgradeId">The type upgrade identifier.</param>
		/// <param name="type">The type.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="fromUpgradeId">From upgrade identifier.</param>
		/// <param name="from">From.</param>
		/// <param name="typeDescription">The type description.</param>
		/// <param name="description">The description.</param>
		public ReverseRelationship( DatabaseManager databaseManager, long tenantId, long typeId, Guid typeUpgradeId, string type, long fromId, Guid fromUpgradeId, string from, string typeDescription, string description )
			: base( databaseManager, tenantId, typeId, typeUpgradeId, type, typeDescription, description )
		{
			FromId = fromId;
			FromUpgradeId = fromUpgradeId;
			From = from;

			NavigateFromCommand = new DelegateCommand( NavigateFromClick );
		}

		/// <summary>
		///     Gets from.
		/// </summary>
		/// <value>
		///     From.
		/// </value>
		public string From
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets from identifier.
		/// </summary>
		/// <value>
		///     From identifier.
		/// </value>
		public long FromId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets from upgrade identifier.
		/// </summary>
		/// <value>
		///     From upgrade identifier.
		/// </value>
		public Guid FromUpgradeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the navigate command.
		/// </summary>
		/// <value>
		///     The navigate command.
		/// </value>
		public ICommand NavigateFromCommand
		{
			get;
			set;
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
					return string.Format( "Tenant Id: {0}\nType Id: {1}\nFrom Id: {2}", TenantId, TypeId, FromId );
				}

				return string.Format( "{0}\n\nTenant Id: {1}\nType Id: {2}\nFrom Id: {3}", Description, TenantId, TypeId, FromId );
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
					return string.Format( "Tenant Id: {0}\nType Id: {1}\nFrom Id: {2}", TenantId, TypeId, FromId );
				}

				return string.Format( "{0}\n\nTenant Id: {1}\nType Id: {2}\nFrom Id: {3}", TypeDescription, TenantId, TypeId, FromId );
			}
		}

		/// <summary>
		///     Navigates the click.
		/// </summary>
		private void NavigateFromClick( )
		{
			EntityNotificationService.NavigateTo( FromId.ToString( CultureInfo.InvariantCulture ), TenantId );
		}
	}
}