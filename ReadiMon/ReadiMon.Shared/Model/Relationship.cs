// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Input;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     Relationship class.
	/// </summary>
	public abstract class Relationship
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="Relationship" /> class.
		/// </summary>
		/// <param name="databaseManager">The database manager.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="typeUpgradeId">The type upgrade identifier.</param>
		/// <param name="type">The type.</param>
		/// <param name="typeDescription">The type description.</param>
		/// <param name="description">The description.</param>
		protected Relationship( DatabaseManager databaseManager, long tenantId, long typeId, Guid typeUpgradeId, string type, string typeDescription, string description )
		{
			DatabaseManager = databaseManager;

			TenantId = tenantId;
			TypeId = typeId;
			TypeUpgradeId = typeUpgradeId;
			Type = type;
			TypeDescription = typeDescription;
			Description = description;

			NavigateCommand = new DelegateCommand( NavigateClick );
		}

		/// <summary>
		///     Gets the database manager.
		/// </summary>
		/// <value>
		///     The database manager.
		/// </value>
		public DatabaseManager DatabaseManager
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the navigate command.
		/// </summary>
		/// <value>
		///     The navigate command.
		/// </value>
		public ICommand NavigateCommand
		{
			get;
			set;
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

		/// <summary>
		///     Gets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public string Type
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the type description.
		/// </summary>
		/// <value>
		///     The type description.
		/// </value>
		public string TypeDescription
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the type identifier.
		/// </summary>
		/// <value>
		///     The type identifier.
		/// </value>
		public long TypeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type upgrade identifier.
		/// </summary>
		/// <value>
		///     The type upgrade identifier.
		/// </value>
		public Guid TypeUpgradeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Navigates the click.
		/// </summary>
		private void NavigateClick( )
		{
			EntityNotificationService.NavigateTo( TypeId.ToString( CultureInfo.InvariantCulture ), TenantId );
		}
	}
}