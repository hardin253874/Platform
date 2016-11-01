// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Graphs.TenantsTrend
{
	/// <summary>
	///     TenantsTrendViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class TenantsTrendViewModel : ViewModelBase
	{
		private Dictionary<DateTime, List<TenantsData>> _active;

		private Dictionary<DateTime, List<TenantsData>> _disabled;

		private IPluginSettings _pluginSettings;

		private object _selectedItem;

		private List<TenantsTrendSeries> _series;

		/// <summary>
		///     The database manager
		/// </summary>
		protected DatabaseManager DatabaseManager;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantsTrendViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TenantsTrendViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( ( ) => Dispatcher.CurrentDispatcher.Invoke( Refresh ) );
			ReloadCommand = new DelegateCommand( ( ) => Dispatcher.CurrentDispatcher.Invoke( Reload ) );

			SelectedTenants = new ObservableCollection<TenantsData>( );

			Load( );
		}

		/// <summary>
		///     Gets the active.
		/// </summary>
		/// <value>
		///     The active.
		/// </value>
		public ObservableCollection<TenantsTrendData> Active
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the disabled.
		/// </summary>
		/// <value>
		///     The disabled.
		/// </value>
		public ObservableCollection<TenantsTrendData> Disabled
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		public IPluginSettings PluginSettings
		{
			get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				DatabaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );
			}
		}

		/// <summary>
		///     Gets or sets the refresh command.
		/// </summary>
		/// <value>
		///     The refresh command.
		/// </value>
		public ICommand RefreshCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the reload command.
		/// </summary>
		/// <value>
		///     The reload command.
		/// </value>
		public ICommand ReloadCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selected item.
		/// </summary>
		/// <value>
		///     The selected item.
		/// </value>
		public object SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				SetProperty( ref _selectedItem, value );

				SelectedTenants.Clear( );
				if ( _selectedItem != null )
				{
					var t = _selectedItem as TenantsTrendData;
					if ( t != null )
					{
						var key = t.Timestamp;
						if ( t.Disabled )
						{
							if ( _disabled.ContainsKey( key ) )
								_disabled[ key ].ForEach( d => SelectedTenants.Add( d ) );
						}
						else
						{
							if ( _active.ContainsKey( key ) )
								_active[ key ].ForEach( a => SelectedTenants.Add( a ) );
						}
					}
				}
			}
		}

		/// <summary>
		///     Gets the selected tenants.
		/// </summary>
		/// <value>
		///     The selected tenants.
		/// </value>
		public ObservableCollection<TenantsData> SelectedTenants
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the series.
		/// </summary>
		/// <value>
		///     The series.
		/// </value>
		public List<TenantsTrendSeries> Series
		{
			get
			{
				return _series;
			}
			set
			{
				SetProperty( ref _series, value );
			}
		}

		private void Load( )
		{
			try
			{
				_active = new Dictionary<DateTime, List<TenantsData>>( );
				_disabled = new Dictionary<DateTime, List<TenantsData>>( );

				const string sql = @"--ReadiMon - Load
SELECT i.[Timestamp], l.[TenantId], l.[Name], l.[Disabled] FROM [dbo].[Lic_Tenant] l JOIN [dbo].[Lic_Index] i ON l.IndexId = i.Id
WHERE i.[Timestamp] BETWEEN GETUTCDATE()-7 AND GETUTCDATE() ORDER BY i.[Timestamp] ASC";

				using ( var command = DatabaseManager.CreateCommand( sql ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							if ( !reader.IsDBNull( 0 ) )
							{
								var timestamp = reader.GetDateTime( 0 );

								var data = new TenantsData( );

								if ( !reader.IsDBNull( 1 ) )
								{
									data.Id = reader.GetInt64( 1 );
								}

								if ( !reader.IsDBNull( 2 ) )
								{
									data.Name = reader.GetString( 2 );
								}

								var key = timestamp.ToLocalTime( );

								if ( reader.IsDBNull( 3 ) || !reader.GetBoolean( 3 ) )
								{
									if ( _active.ContainsKey( key ) )
										_active[ key ].Add( data );
									else
										_active.Add( key, new List<TenantsData>
										{
											data
										} );
								}
								else
								{
									if ( _disabled.ContainsKey( key ) )
										_disabled[ key ].Add( data );
									else
										_disabled.Add( key, new List<TenantsData>
										{
											data
										} );
								}
							}
						}
					}
				}

				Active = new ObservableCollection<TenantsTrendData>( );
				Disabled = new ObservableCollection<TenantsTrendData>( );

				foreach ( var a in _active.OrderBy( o => o.Key ) )
				{
					Active.Add( new TenantsTrendData
					{
						Label = a.Key.ToString( "g" ),
						Timestamp = a.Key,
						Count = a.Value.Count,
						Disabled = false
					} );
				}

				foreach ( var d in _disabled.OrderBy( o => o.Key ) )
				{
					Disabled.Add( new TenantsTrendData
					{
						Label = d.Key.ToString( "g" ),
						Timestamp = d.Key,
						Count = d.Value.Count,
						Disabled = true
					} );
				}

				Refresh( );
			}
			catch ( Exception e )
			{
				PluginSettings.EventLog.WriteException( e );
			}
		}

		private void Refresh( )
		{
			Series = new List<TenantsTrendSeries>
			{
				new TenantsTrendSeries
				{
					DisplayName = "Active",
					Items = Active
				},
				new TenantsTrendSeries
				{
					DisplayName = "Disabled",
					Items = Disabled
				}
			};
		}

		private void Reload( )
		{
			Load( );
		}
	}
}