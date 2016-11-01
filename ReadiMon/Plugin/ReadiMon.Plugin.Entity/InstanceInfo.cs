// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Instance Info
	/// </summary>
	public class InstanceInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="InstanceInfo" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="typeName">Name of the type.</param>
		/// <param name="createdDate">The created date.</param>
		/// <param name="modifiedDate">The modified date.</param>
		/// <param name="applications">The applications.</param>
		/// <param name="forwardRelationships">The forward relationships.</param>
		/// <param name="reverseRelationships">The reverse relationships.</param>
		/// <param name="settings">The settings.</param>
		public InstanceInfo( long id, long tenantId, string name, string description, string alias, string typeName, DateTime createdDate, DateTime modifiedDate, string applications, string forwardRelationships, string reverseRelationships, IPluginSettings settings )
		{
			Id = id;
			TenantId = tenantId;
			Name = name;
			Description = description;
			Alias = alias;
			TypeName = typeName;
			CreatedDate = createdDate == DateTime.MinValue ? string.Empty : createdDate.ToString( CultureInfo.InvariantCulture );
			ModifiedDate = modifiedDate == DateTime.MinValue ? string.Empty : modifiedDate.ToString( CultureInfo.InvariantCulture );

			if ( applications != null )
			{
				Applications = applications.Replace( ",", ", " );
			}

			ForwardRelationships = forwardRelationships != null ? new HashSet<long>( forwardRelationships.Split( ',' ).Select( long.Parse ) ) : new HashSet<long>( );
			ReverseRelationships = reverseRelationships != null ? new HashSet<long>( reverseRelationships.Split( ',' ).Select( long.Parse ) ) : new HashSet<long>( );

			NavigateCommand = new DelegateCommand( NavigateClick );
			PluginSettings = settings;
		}

		/// <summary>
		///     Gets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		public string Alias
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the applications.
		/// </summary>
		/// <value>
		///     The applications.
		/// </value>
		public string Applications
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the created date.
		/// </summary>
		/// <value>
		///     The created date.
		/// </value>
		public string CreatedDate
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the forward relationships.
		/// </summary>
		/// <value>
		///     The forward relationships.
		/// </value>
		public HashSet<long> ForwardRelationships
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the modified date.
		/// </summary>
		/// <value>
		///     The modified date.
		/// </value>
		public string ModifiedDate
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
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
		public ICommand NavigateCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		private IPluginSettings PluginSettings
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the reverse relationships.
		/// </summary>
		/// <value>
		///     The reverse relationships.
		/// </value>
		public HashSet<long> ReverseRelationships
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

		/// <summary>
		///     Gets the name of the type.
		/// </summary>
		/// <value>
		///     The name of the type.
		/// </value>
		public string TypeName
		{
			get;
			private set;
		}

		/// <summary>
		///     Navigates the click.
		/// </summary>
		private void NavigateClick( )
		{
			PluginSettings.Channel.SendMessage( new EntityBrowserMessage( Id.ToString( CultureInfo.InvariantCulture ) ).ToString( ) );
		}
	}
}