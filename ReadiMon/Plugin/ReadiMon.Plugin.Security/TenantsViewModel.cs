// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     Tenants View Model.
	/// </summary>
	public class TenantsViewModel : ViewModelBase
	{
		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The _tenants
		/// </summary>
		private List<TenantInfo> _tenants;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantsViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TenantsViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( Refresh );
			CopyIdCommand = new DelegateCommand<TenantInfo>( CopyIdClick );
		}

		/// <summary>
		///     Gets or sets the copy identifier command.
		/// </summary>
		/// <value>
		///     The copy identifier command.
		/// </value>
		public ICommand CopyIdCommand
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
		public IPluginSettings PluginSettings
		{
			private get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				Refresh( );
			}
		}

		/// <summary>
		///     Gets the refresh command.
		/// </summary>
		/// <value>
		///     The refresh command.
		/// </value>
		public ICommand RefreshCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public List<TenantInfo> Tenants
		{
			get
			{
				return _tenants;
			}
			set
			{
				SetProperty( ref _tenants, value );
			}
		}

		/// <summary>
		///     Copies the identifier click.
		/// </summary>
		/// <param name="tenant">The application.</param>
		private void CopyIdClick( TenantInfo tenant )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, tenant.Id.ToString( ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Id copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Loads the tenants.
		/// </summary>
		private void LoadTenants( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - LoadTenants
SET NOCOUNT ON

DECLARE @isTenantDisabled bigint = dbo.fnAliasNsId( 'isTenantDisabled', 'core', DEFAULT )

SELECT DISTINCT TenantId Id, CASE WHEN TenantId = 0 THEN 'Global' ELSE 'Missing Tenant Entry' END [Name], 'N/A' [Description], NULL CreatedDate, NULL ModifiedDate, 'N/A' [Enabled] FROM Entity WHERE TenantId NOT IN ( SELECT Id FROM _vTenant )
UNION
SELECT Id, name, description, createdDate, modifiedDate, CASE WHEN b.Data = 1 THEN 'True' ELSE 'False' END FROM _vTenant t LEFT JOIN Data_Bit b ON t.Id = b.EntityId
";

			try
			{
				using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var tenants = new List<TenantInfo>( );

						while ( reader.Read( ) )
						{
							var id = reader.GetInt64( 0 );
							var name = reader.GetString( 1, "<Unnamed>" );
							var description = reader.GetString( 2, string.Empty );
							var createdDate = reader.GetDateTime( 3, DateTime.MinValue );
							var modifiedDate = reader.GetDateTime( 4, DateTime.MinValue );
							var enabled = reader.GetString( 5, "True" );

							var tenant = new TenantInfo( id, name, description, createdDate == DateTime.MinValue ? "" : createdDate.ToString( CultureInfo.CurrentCulture ), modifiedDate == DateTime.MinValue ? "" : modifiedDate.ToString( CultureInfo.CurrentCulture ), enabled ?? "N/A" );

							tenants.Add( tenant );
						}

						Tenants = tenants;
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Refreshes this instance.
		/// </summary>
		private void Refresh( )
		{
			LoadTenants( );
		}
	}
}