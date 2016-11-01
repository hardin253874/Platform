// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Database test.
	/// </summary>
	public class DatabaseTest : ViewModelBase
	{
		/// <summary>
		///     The updating flag.
		/// </summary>
		private static bool _updating;

		/// <summary>
		///     The description
		/// </summary>
		private string _description;

		/// <summary>
		///     The enabled.
		/// </summary>
		private bool _enabled = true;

		/// <summary>
		///     The _entity columns
		/// </summary>
		private string _entityColumns;

		/// <summary>
		///     The failed click
		/// </summary>
		private ICommand _failedClick;

		/// <summary>
		///     The failure details
		/// </summary>
		private string _failureDetails;

		/// <summary>
		///     The _hidden columns
		/// </summary>
		private string _hiddenColumns;

		/// <summary>
		///     The name
		/// </summary>
		private string _name;

		/// <summary>
		/// The tenant column
		/// </summary>
		private int? _tenantColumn;

		/// <summary>
		///     Is selected
		/// </summary>
		private bool _selected;

		/// <summary>
		///     The selected to run
		/// </summary>
		private bool _selectedToRun = true;

		/// <summary>
		///     The state
		/// </summary>
		private TestState _state;

		/// <summary>
		///     The test type
		/// </summary>
		private string _testType;

        /// <summary>
        ///     An action to rerun the test.
        /// </summary>
        public Action Rerun { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseTest" /> class.
        /// </summary>
        public DatabaseTest( IPluginSettings databaseSettings, List<DatabaseTest> testCollection )
		{
			FailedClick = new DelegateCommand( OnFailedClick );

			TestType = "test";

			PluginSettings = databaseSettings;
			TestCollection = testCollection;

			ShowEditorCommand = new DelegateCommand( ShowEditor );
		}

		/// <summary>
		///     Gets or sets the start time.
		/// </summary>
		/// <value>
		///     The start time.
		/// </value>
		public DateTime StartTime
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the end time.
		/// </summary>
		/// <value>
		///     The end time.
		/// </value>
		public DateTime EndTime
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the test collection.
		/// </summary>
		/// <value>
		///     The test collection.
		/// </value>
		private List<DatabaseTest> TestCollection
		{
			get;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				SetProperty( ref _description, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="DatabaseTest" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			private set
			{
				SetProperty( ref _enabled, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is selected.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelected
		{
			get
			{
				return _selected;
			}
			private set
			{
				SetProperty( ref _selected, value );
			}
		}

		/// <summary>
		///     Gets or sets the entity columns.
		/// </summary>
		/// <value>
		///     The entity columns.
		/// </value>
		public string EntityColumns
		{
			internal get
			{
				return _entityColumns;
			}
			set
			{
				SetProperty( ref _entityColumns, value );
			}
		}

		/// <summary>
		///     Gets or sets the hidden columns.
		/// </summary>
		/// <value>
		///     The hidden columns.
		/// </value>
		public string HiddenColumns
		{
			get
			{
				return _hiddenColumns;
			}
			set
			{
				SetProperty( ref _hiddenColumns, value );
			}
		}

		/// <summary>
		/// Gets or sets the tenant column.
		/// </summary>
		/// <value>
		/// The tenant column.
		/// </value>
		public int? TenantColumn
		{
			get
			{
				return _tenantColumn;
			}
			set
			{
				SetProperty( ref _tenantColumn, value );
			}
		}	

		/// <summary>
		///     Gets or sets the failed click.
		/// </summary>
		/// <value>
		///     The failed click.
		/// </value>
		public ICommand FailedClick
		{
			get
			{
				return _failedClick;
			}
			private set
			{
				SetProperty( ref _failedClick, value );
			}
		}

		/// <summary>
		///     Gets or sets the failure details.
		/// </summary>
		/// <value>
		///     The failure details.
		/// </value>
		public string FailureDetails
		{
			get
			{
				return _failureDetails;
			}
			private set
			{
				SetProperty( ref _failureDetails, value );
			}
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetProperty( ref _name, value );
			}
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		internal IPluginSettings PluginSettings
		{
			get;
		}

		/// <summary>
		///     Gets or sets the query.
		/// </summary>
		/// <value>
		///     The query.
		/// </value>
		public string Query
		{
			private get;
			set;
		}

		/// <summary>
		///     Gets or sets the resolution.
		/// </summary>
		/// <value>
		///     The resolution.
		/// </value>
		public string Resolution
		{
			internal get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [selected to run].
		/// </summary>
		/// <value>
		///     <c>true</c> if [selected to run]; otherwise, <c>false</c>.
		/// </value>
		public bool SelectedToRun
		{
			get
			{
				return _selectedToRun;
			}
			set
			{
				if ( !IsSelected && !_updating )
				{
					_updating = true;

					foreach ( DatabaseTest test in TestCollection )
					{
						if ( test != this && test.IsSelected )
						{
							test.IsSelected = false;
						}
					}

					IsSelected = true;

					_updating = false;
				}

				SetProperty( ref _selectedToRun, value );

				if ( !_updating )
				{
					_updating = true;

					foreach ( DatabaseTest test in TestCollection )
					{
						if ( test != this && test.IsSelected )
						{
							test.SelectedToRun = value;
						}
					}

					_updating = false;
				}
			}
		}

		/// <summary>
		///     Gets or sets the show editor command.
		/// </summary>
		/// <value>
		///     The show editor command.
		/// </value>
		public ICommand ShowEditorCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the state.
		/// </summary>
		/// <value>
		///     The state.
		/// </value>
		public TestState State
		{
			get
			{
				return _state;
			}
			private set
			{
				SetProperty( ref _state, value );
			}
		}

		/// <summary>
		///     Gets or sets the type of the test.
		/// </summary>
		/// <value>
		///     The type of the test.
		/// </value>
		public string TestType
		{
			get
			{
				return _testType;
			}
			set
			{
				SetProperty( ref _testType, value );
			}
		}

		/// <summary>
		///     Resets this instance.
		/// </summary>
		public void Reset( )
		{
			FailureDetails = null;
			State = TestState.Idle;
			Enabled = true;
		}

        /// <summary>
        ///     Runs this instance.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="token">The token.</param>
        /// <param name="tenantId">The tenant to run the test for, or -1 for all.</param>
        /// <returns></returns>
        public async Task Run( int timeout, SemaphoreSlim semaphore, CancellationToken token, long tenantId )
		{
            Rerun = ( ) =>
            {
                Task task = Task.Run( ( ) => Run( timeout, null, CancellationToken.None, tenantId ) );
                task.Wait( );
            };

			Enabled = false;
            bool skipped = false;

			await Task.Run( ( ) =>
			{
				StartTime = DateTime.UtcNow;
                FailureDetails = null;

                try
				{
					semaphore?.Wait( token );

					if ( token.IsCancellationRequested )
					{
						return;
					}

					State = TestState.Running;

					var manager = new DatabaseManager( PluginSettings.DatabaseSettings );

                    string sql = Query;

                    // Adjust query to filter tenant
                    if (tenantId >= 0 || !string.IsNullOrEmpty(PluginSettings.Tenants))
                    {
                        // test that check for invalid tenant are not applicable when a tenant has been selected
                        if (Name.Contains("is not a valid tenant"))
                        {
                            skipped = true;
                            return;
                        }

                        sql = ConstrainTenant(tenantId, sql);
                    }

                    using ( var command = manager.CreateCommand( sql, CommandType.Text, timeout ) )
					using ( token.Register( command.Cancel ) )
					{
                        using ( IDataReader reader = command.ExecuteReader( ) )
						{
							var sb = new StringBuilder( );

							bool first = true;

							while ( reader.Read( ) )
							{
								if ( first )
								{
									for ( int i = 0; i < reader.FieldCount; i++ )
									{
										sb.AppendFormat( "{0}\t", reader.GetName( i ) );
									}

									sb.Remove( sb.Length - 1, 1 );

									sb.AppendLine( );

									first = false;
								}

								for ( int i = 0; i < reader.FieldCount; i++ )
								{
									sb.AppendFormat( "{0}\t", reader.GetValue( i ).ToString( ).Replace( "\t", "    " ).Replace( "\r\n", "\n" ) );
								}

								sb.Remove( sb.Length - 1, 1 );

								sb.AppendLine( );
							}

							if ( sb.Length > 0 )
							{
								FailureDetails = sb.ToString( );
							}
						}
					}
				}
				catch ( Exception exc )
				{
					if ( !token.IsCancellationRequested )
					{
						FailureDetails = $"Exception Message(s)\r\n{exc}";
					}
				}
				finally
				{
					semaphore?.Release( );
					EndTime = DateTime.UtcNow;
				}
			}, token );

            if ( token.IsCancellationRequested )
			{
				State = TestState.Cancelled;
			}
            else if ( skipped )
            {
                State = TestState.Skipped;
            }
            else if ( string.IsNullOrEmpty( FailureDetails ) )
			{
				State = TestState.Passed;
			}
			else if ( TestType == "test" )
			{
				State = TestState.Failed;
			}
			else
			{
				State = TestState.Info;
			}

			Enabled = true;
		}

        private string ConstrainTenant(long tenantId, string sql)
        {
            sql = sql.Replace("_vTenant\r", "_vTenant t\r"); // ensure there is a table alias
            sql = sql.Replace("_vTenant\n", "_vTenant t\n");

            if (!string.IsNullOrEmpty(PluginSettings.Tenants))
            {
                string[] tenants = PluginSettings.Tenants.Split(',');
                string tenantNames = string.Join("', '", tenants.Select(name => name.ToLower().Replace("'", "''")));
                sql = sql.Replace("_vTenant", "(select Id, name from _vTenant where lower(name) in ('" + tenantNames + "'))");
                if (!tenants.Contains("Global"))
                {
                    sql = sql.Replace("'Global'", "'Global' where 1=2");
                }
            }
            else
            {
                sql = sql.Replace("_vTenant", "(select Id, name from _vTenant where Id=" + tenantId + ")");
                if (tenantId > 0)
                {
                    sql = sql.Replace("'Global'", "'Global' where 1=2");
                }
            }

            return sql;
        }

        /// <summary>
        ///     Called when [failed click].
        /// </summary>
        private void OnFailedClick( )
		{
			var viewer = new TestViewer( );

			var helper = new WindowInteropHelper( viewer );

            var model = new TestViewerViewModel( this );
			viewer.DataContext = model;

			helper.Owner = Process.GetCurrentProcess( ).MainWindowHandle;

			viewer.Show( );
		}

		/// <summary>
		///     Shows the editor.
		/// </summary>
		private void ShowEditor( )
		{
			var editor = new TestEditor( PluginSettings, Query );
			editor.ShowDialog( );
		}
	}
}