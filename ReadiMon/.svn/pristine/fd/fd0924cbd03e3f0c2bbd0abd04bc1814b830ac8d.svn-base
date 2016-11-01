// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using Microsoft.Win32;
using System.IO;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     TestViewer view model.
	/// </summary>
	public class TestViewerViewModel : ViewModelBase
	{
        /// <summary>
        ///     The test
        /// </summary>
        private readonly DatabaseTest _test;

        /// <summary>
        ///     The test name
        /// </summary>
        private readonly string _testName;

		/// <summary>
		///     The test type
		/// </summary>
		private readonly string _testType;

        /// <summary>
        ///     The raw data
        /// </summary>
        private string _rawData;

        /// <summary>
        ///     The data
        /// </summary>
        private List<FailureRow> _data;

		/// <summary>
		/// The delete count
		/// </summary>
		private int _deleteCount;

		/// <summary>
		/// The delete count maximum
		/// </summary>
		private int _deleteCountMax;

		/// <summary>
		///     The deleting
		/// </summary>
		private bool _deleting;

		/// <summary>
		///     The _filtered data
		/// </summary>
		private List<FailureRow> _filteredData;

		/// <summary>
		///     The _row count
		/// </summary>
		private string _rowCount;

		/// <summary>
		///     The selected item
		/// </summary>
		private FailureRow _selectedItem;

		/// <summary>
		///     The _selected tenant
		/// </summary>
		private string _selectedTenant;

		/// <summary>
		///     The tenants
		/// </summary>
		private List<string> _tenants = new List<string>( );

		/// <summary>
		///     The _tenants visible
		/// </summary>
		private bool _tenantsVisible;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestViewerViewModel" /> class.
        /// </summary>
        /// <param name="test">The test.</param>
        public TestViewerViewModel( DatabaseTest test )
        {
            _test = test;
            _testName = test.Name;
            _testType = test.TestType;

            Resolution = test.Resolution;

            PluginSettings = test.PluginSettings;

            TenantColumn = test.TenantColumn;

            OkCommand = new DelegateCommand<Window>( OnOkClick );
            CopyCommand = new DelegateCommand<FailureRow>( OnCopyClick );
            DeleteCommand = new DelegateCommand( OnDeleteClick );
            SaveCommand = new DelegateCommand( OnSaveClick );
            RerunCommand = new DelegateCommand( OnRerunClick );

            Columns = new List<string>( );

            SetTestResult( test.FailureDetails );
        }

        /// <summary>
        ///     Set the test result data.
        /// </summary>
        /// <param name="data">The results</param>
        private void SetTestResult( string data )
        {
            _rawData = data;

            if ( string.IsNullOrEmpty( data ) )
            {
                Data?.Clear( );
                return;
            }
            Columns.Clear( );

            Data = new List<FailureRow>( );

			var rows = data.Split( new[ ]
			{
				"\r\n"
			}, StringSplitOptions.RemoveEmptyEntries );

			bool first = true;

            var hiddenColumns = _test.HiddenColumns;
            var entityColumns = _test.EntityColumns;
            var tenantColumn = _test.TenantColumn;

            HashSet<int> hiddenColumnSet = new HashSet<int>( );

			if ( !string.IsNullOrEmpty( hiddenColumns ) )
			{
				var hiddenColumnStrings = hiddenColumns.Split( ',' );

				foreach ( string hiddenColumnString in hiddenColumnStrings )
				{
					int hiddenColumn;

					if ( int.TryParse( hiddenColumnString, out hiddenColumn ) )
					{
						hiddenColumnSet.Add( hiddenColumn );
					}
				}
			}

			HashSet<string> tenants = new HashSet<string>( );

			foreach ( string row in rows )
			{
				string[ ] fields = row.Split( '\t' );

				if ( first )
				{
					int columnId = 0;
					foreach ( string column in fields )
					{
						if ( !hiddenColumnSet.Contains( columnId ) )
						{
							Columns.Add( column );
						}
						columnId++;
					}

					first = false;

					continue;
				}

				if ( tenantColumn != null )
				{
					tenants.Add( fields[ tenantColumn.Value ] );
				}

				var failureFields = new List<string>( fields );
				var allFields = new List<string>( fields );

				foreach ( int hiddenColumn in hiddenColumnSet.OrderByDescending( i => i ) )
				{
					if ( failureFields.Count > hiddenColumn )
					{
						failureFields.RemoveAt( hiddenColumn );
					}
				}

				var failureRow = new FailureRow( this, failureFields, allFields, entityColumns, Resolution, PluginSettings );

				Data.Add( failureRow );
			}

			List<string> tenantList = new List<string>
			{
				"All Tenants"
			};

			if ( tenants.Count > 0 )
			{
				tenantList.AddRange( tenants.OrderBy( s => s ) );
				TenantsVisible = true;
			}
			else
			{
				TenantsVisible = false;
			}

			Tenants = tenantList;

			SelectedTenant = "All Tenants";
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="TestViewerViewModel" /> is deleting.
		/// </summary>
		/// <value>
		///     <c>true</c> if deleting; otherwise, <c>false</c>.
		/// </value>
		public bool Deleting
		{
			get
			{
				return _deleting;
			}
			set
			{
				SetProperty( ref _deleting, value );
			}
		}

		/// <summary>
		///     Gets or sets the delete count.
		/// </summary>
		/// <value>
		///     The delete count.
		/// </value>
		public int DeleteCountMax
		{
			get
			{
				return _deleteCountMax;
			}
			set
			{
				SetProperty( ref _deleteCountMax, value );
			}
		}

		/// <summary>
		///     Gets or sets the delete count.
		/// </summary>
		/// <value>
		///     The delete count.
		/// </value>
		public int DeleteCount
		{
			get
			{
				return _deleteCount;
			}
			set
			{
				SetProperty( ref _deleteCount, value );
			}
		}

		/// <summary>
		///     Gets or sets the tenant column.
		/// </summary>
		/// <value>
		///     The tenant column.
		/// </value>
		private int? TenantColumn
		{
			get;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [tenants visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [tenants visible]; otherwise, <c>false</c>.
		/// </value>
		public bool TenantsVisible
		{
			get
			{
				return _tenantsVisible;
			}
			private set
			{
				SetProperty( ref _tenantsVisible, value );
			}
		}

		/// <summary>
		///     Gets or sets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public List<string> Tenants
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
		///     Gets or sets the selected tenant.
		/// </summary>
		/// <value>
		///     The selected tenant.
		/// </value>
		public string SelectedTenant
		{
			get
			{
				return _selectedTenant;
			}
			set
			{
				SetProperty( ref _selectedTenant, value );

				if ( value == "All Tenants" || string.IsNullOrEmpty( value ) || TenantColumn == null )
				{
					FilteredData = new List<FailureRow>( Data );
				}
				else
				{
					FilteredData = new List<FailureRow>( Data.Where( r => r.AllFields[ TenantColumn.Value ] == value ) );
				}

				RowCount = $"Count: {FilteredData.Count}";
			}
		}

		/// <summary>
		///     Gets or sets the resolution.
		/// </summary>
		/// <value>
		///     The resolution.
		/// </value>
		public string Resolution
		{
			get;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [delete enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [delete enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool DeleteEnabled
		{
			get
			{
				return Data.Any( r => r.RowSelected );
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
				OnPropertyChanged( );
			}
		}

		/// <summary>
		///     Gets a value indicating whether [delete visible].
		/// </summary>
		/// <value>
		///     <c>true</c> if [delete visible]; otherwise, <c>false</c>.
		/// </value>
		public bool DeleteVisible => !string.IsNullOrEmpty( Resolution );

		/// <summary>
		///     Gets or sets the row count.
		/// </summary>
		/// <value>
		///     The row count.
		/// </value>
		public string RowCount
		{
			get
			{
				return _rowCount;
			}
			set
			{
				SetProperty( ref _rowCount, value );
			}
		}

		/// <summary>
		///     Gets the columns.
		/// </summary>
		/// <value>
		///     The columns.
		/// </value>
		public List<string> Columns
		{
			get;
		}

		/// <summary>
		///     Gets or sets the copy command.
		/// </summary>
		/// <value>
		///     The copy command.
		/// </value>
		public ICommand CopyCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the delete command.
		/// </summary>
		/// <value>
		///     The delete command.
		/// </value>
		public ICommand DeleteCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		private List<FailureRow> Data
		{
			get
			{
				return _data;
			}
			set
			{
				SetProperty( ref _data, value );
			}
		}

		/// <summary>
		///     Gets the filtered data.
		/// </summary>
		/// <value>
		///     The filtered data.
		/// </value>
		public List<FailureRow> FilteredData
		{
			get
			{
				return _filteredData;
			}
			private set
			{
				SetProperty( ref _filteredData, value );
			}
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get
			{
				if ( _testType == "test" )
				{
					return "The following database rows have caused failures in the selected test.";
				}

				return "The following results have been obtained.";
			}
		}

		/// <summary>
		///     Gets the image source.
		/// </summary>
		/// <value>
		///     The image source.
		/// </value>
		public string ImageSource
		{
			get
			{
				if ( _testType == "test" )
				{
					return "pack://application:,,,/ReadiMon.Plugin.Database;component/Resources/failure.png";
				}
				return "pack://application:,,,/ReadiMon.Plugin.Database;component/Resources/info.png";
			}
		}

		/// <summary>
		///     Gets or sets the OK command.
		/// </summary>
		/// <value>
		///     The OK command.
		/// </value>
		public ICommand OkCommand
		{
			get;
			set;
        }

        /// <summary>
        ///     Gets or sets the Save command.
        /// </summary>
        /// <value>
        ///     The Save command.
        /// </value>
        public ICommand SaveCommand
        {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the Rerun command.
        /// </summary>
        /// <value>
        ///     The Rerun command.
        /// </value>
        public ICommand RerunCommand
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
		}

		/// <summary>
		///     Gets or sets the selected item.
		/// </summary>
		/// <value>
		///     The selected item.
		/// </value>
		public FailureRow SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				SetProperty( ref _selectedItem, value );
			}
		}

		/// <summary>
		///     Gets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string Title
		{
			get
			{
				if ( _testType == "test" )
				{
					return _testName + " - Failing Rows";
				}

				return _testName + " - Results";
			}
		}

		/// <summary>
		///     Gets or sets the title.
		/// </summary>
		/// <value>
		///     The title.
		/// </value>
		public string WindowTitle
		{
			get
			{
				if ( _testType == "test" )
				{
					return "Failing Database Rows";
				}

				return "Database Information";
			}
		}

		/// <summary>
		///     Called when copy has been clicked.
		/// </summary>
		/// <param name="data">The data.</param>
		private void OnCopyClick( FailureRow data )
		{
			var sb = new StringBuilder( );

			for ( int i = 0; i < Columns.Count; i++ )
			{
				sb.AppendFormat( "{0}: {1}\r\n", Columns[ i ], data.Fields[ i ] );
			}

			Clipboard.SetText( sb.ToString( ) );
		}

		/// <summary>
		///     Called when delete has been clicked.
		/// </summary>
		private void OnDeleteClick( )
		{
			if ( string.IsNullOrEmpty( Resolution ) )
			{
				return;
			}

			MessageBoxResult result = MessageBox.Show( $"Are you sure you wish to delete the {Data.Count( r => r.RowSelected )} selected row(s)?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning );

			if ( result == MessageBoxResult.Yes )
			{
				Deleting = true;
				Task task = new Task( PerformDelete, Tuple.Create( Dispatcher.CurrentDispatcher, FilteredData ) );
				task.ContinueWith( task1 => Deleting = false );
				task.Start( );
			}
		}

		/// <summary>
		/// Performs the delete.
		/// </summary>
		/// <param name="state">The state.</param>
		private void PerformDelete( object state )
		{
			Tuple<Dispatcher, List<FailureRow>> data = state as Tuple<Dispatcher, List<FailureRow>>;

			if ( data == null )
			{
				return;
			}

			DatabaseManager manager = new DatabaseManager( PluginSettings.DatabaseSettings );

			List<FailureRow> removedRows = new List<FailureRow>( );

			try
			{
				data.Item1.Invoke( ( ) => Mouse.OverrideCursor = Cursors.Wait );

				List<FailureRow> toDelete = data.Item2.Where( r => r.RowSelected ).ToList( );

				data.Item1.Invoke( ( ) =>
				{
					DeleteCountMax = toDelete.Count;
					DeleteCount = 0;
				} );

				foreach ( FailureRow row in toDelete )
				{
					if ( row.RowSelected )
					{
						string commandText = string.Format( Resolution, row.AllFields.Select( f => ( object ) f ).ToArray( ) );

						commandText = $@"
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'Database Tests->Resolve Test' )
SET CONTEXT_INFO @contextInfo

{commandText}";

						using ( var command = manager.CreateCommand( commandText ) )
						{
							command.ExecuteNonQuery( );

							removedRows.Add( row );
						}
					}

					data.Item1.Invoke( ( ) => DeleteCount++ );
				}

				foreach ( var row in removedRows )
				{
					Data.Remove( row );
				}

				Data = new List<FailureRow>( Data );

				SelectedTenant = SelectedTenant;
			}
			catch ( Exception exc )
			{
				MessageBox.Show( exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			finally
			{
				data.Item1.Invoke( ( ) => Mouse.OverrideCursor = null );
			}
		}

		/// <summary>
		///     Called when OK is clicked.
		/// </summary>
		private void OnOkClick( Window window )
		{
			window.Close( );
        }

        /// <summary>
        ///     Called when Save is clicked.
        /// </summary>
        private void OnSaveClick( )
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog( );
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            saveFileDialog.FileName = _testName + ".txt";
            if ( saveFileDialog.ShowDialog( ) == true )
            {
                File.WriteAllText( saveFileDialog.FileName, _rawData );
            }
        }

        /// <summary>
        ///     Called when Save is clicked.
        /// </summary>
        private void OnRerunClick( )
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _test.Rerun( );
            SetTestResult( _test.FailureDetails );
            Mouse.OverrideCursor = null;
        }
    }
}