// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Graphs.TableSizes
{
	/// <summary>
	///     TableSizesViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class TableSizesViewModel : ViewModelBase
	{
		private string _chartTitle;
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The database manager
		/// </summary>
		protected DatabaseManager DatabaseManager;

		/// <summary>
		///     Initializes a new instance of the <see cref="TableSizesViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public TableSizesViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( ( ) => Dispatcher.CurrentDispatcher.Invoke( Refresh ) );
			ReloadCommand = new DelegateCommand( ( ) => Dispatcher.CurrentDispatcher.Invoke( Reload ) );

			SeriesData = new ObservableCollection<TableSizeData>( );

			Load( );
		}

		/// <summary>
		///     Gets or sets the chart title.
		/// </summary>
		/// <value>
		///     The chart title.
		/// </value>
		public string ChartTitle
		{
			get
			{
				return _chartTitle;
			}
			set
			{
				SetProperty( ref _chartTitle, value );
			}
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
		///     Gets the series data.
		/// </summary>
		/// <value>
		///     The series data.
		/// </value>
		public ObservableCollection<TableSizeData> SeriesData
		{
			get;
			private set;
		}

		private void Load( )
		{
			try
			{
				IndexData index = null;

				ChartTitle = "";
				SeriesData.Clear( );

				using ( var cmd = DatabaseManager.CreateCommand( @"--ReadiMon - TableSizes (Load)
SELECT TOP 1 i.[Id], i.[Timestamp] FROM [dbo].[Lic_Index] i ORDER BY i.[Timestamp] DESC" ) )
				{
					using ( IDataReader reader = cmd.ExecuteReader( ) )
					{
						if ( reader.Read( ) )
						{
							index = new IndexData
							{
								Id = reader.GetInt64( 0 ),
								TimeStamp = reader.GetDateTime( 1 )
							};
						}
					}
				}

				if ( index == null )
					return;

				ChartTitle = index.TimeStamp.ToLocalTime( ).ToString( "F" );

				const string commandText = @"--ReadiMon - TableSizes (Load)
SELECT 
tn.[Name], t.[RowCount], t.[MinRowBytes], t.[MaxRowBytes], t.[AvgRowBytes]
FROM [dbo].[Lic_Table] t
JOIN [dbo].[Lic_Table_Name] tn ON t.TableId = tn.Id
WHERE t.IndexId = @id;";

				using ( var command = DatabaseManager.CreateCommand( commandText ) )
				{
					DatabaseManager.AddParameter( command, "@id", index.Id );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var data = new TableSizeData( );

							if ( !reader.IsDBNull( 0 ) )
							{
								data.Name = reader.GetString( 0 );
							}

							if ( !reader.IsDBNull( 1 ) )
							{
								data.RowCount = reader.GetInt32( 1 );
							}

							if ( !reader.IsDBNull( 2 ) )
							{
								data.MinRowBytes = reader.GetInt32( 2 );
							}

							if ( !reader.IsDBNull( 3 ) )
							{
								data.MaxRowBytes = reader.GetInt32( 3 );
							}

							if ( !reader.IsDBNull( 4 ) )
							{
								data.AvgRowBytes = reader.GetInt32( 4 );
							}

							data.Amount = Math.Round( ( double ) ( data.RowCount * data.AvgRowBytes ) / ( 1024 * 1024 ), 2 ); // megabytes

							SeriesData.Add( data );
						}
					}
				}
			}
			catch ( Exception e )
			{
				PluginSettings.EventLog.WriteException( e );
			}
		}

		private void Refresh( )
		{
			var hackItem = new TableSizeData
			{
				Name = "",
				Amount = 0
			};
			SeriesData.Add( hackItem );
			SeriesData.Remove( hackItem );
		}

		private void Reload( )
		{
			Load( );
		}
	}
}