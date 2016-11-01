// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using EDC.Diagnostics;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.IO;
using LogViewer.Common;
using LogViewer.Properties;
using Application = System.Windows.Application;
using DataGrid = System.Windows.Controls.DataGrid;
using EventLogEntry = EDC.Diagnostics.EventLogEntry;
using EventLogEntryCollection = EDC.Diagnostics.EventLogEntryCollection;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Threading.Timer;
using System.ComponentModel;

namespace LogViewer.ViewModels
{
	/// <summary>
	/// </summary>
	internal class MainWindowViewModel : ObservableObject
	{
		#region Constructor

		/// <summary>
		///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
		/// </summary>
		public MainWindowViewModel( )
		{
			try
			{
				JumpToNextCommand = new RelayCommand( ExecuteJumpToNextCommand );
				ExitCommand = new RelayCommand( ExecuteExitCommand );
				JumpToPreviousCommand = new RelayCommand( ExecuteJumpToPreviousCommand );
				SearchToNextCommand = new RelayCommand( ExecuteSearchToNextCommand );
				SearchToPreviousCommand = new RelayCommand( ExecuteSearchToPreviousCommand );
				LogEntriesGridLoadedCommand = new RelayCommand<DataGrid>( ExecuteLogEntriesGridLoadedCommand );
				ClearLogEntriesCommand = new RelayCommand( ExecuteClearLogEntriesCommand );
				ReloadLogEntriesCommand = new RelayCommand( ExecuteReloadLogEntriesCommand );
				StartMonitoringCommand = new RelayCommand( ExecuteStartMonitoringCommand, CanExecuteStartMonitoringCommand );
				StopMonitoringCommand = new RelayCommand( ExecuteStopMonitoringCommand, CanExecuteStopMonitoringCommand );
				FiltersCommand = new RelayCommand( ExecuteFiltersCommand );
				OpenFileCommand = new RelayCommand( ExecuteOpenFileCommand );
				SaveAsCommand = new RelayCommand( ExecuteSaveAsCommand, CanSaveAsCommand );
				OpenFolderCommand = new RelayCommand( ExecuteOpenFolderCommand );
				LogEntryDetailsMouseMoveCommand = new RelayCommand<TextBox>( ExecuteLogEntryDetailsMouseMoveCommand );
				SearchOnlineCommand = new RelayCommand<TextBox>( ExecuteSearchOnlineCommand, CanExecuteSearchOnlineCommand );

				_logPath = SpecialFolder.GetSpecialFolderPath( SpecialMachineFolders.Log );
				if ( String.IsNullOrEmpty( _logPath ) )
				{
					_logPath = Directory.GetCurrentDirectory( );
				}
			    SetTitlePath( _logPath );

                _logEntries.Source = _logEntriesSortedListFiltered.Values;
				//logEntries.Filter += new FilterEventHandler(logEntries_Filter);

				_logFolderWatcher = new FileSystemWatcher( _logPath, "*.xml" );
				_logFolderWatcher.Changed += logFolderWatcher_Changed;
				_logFolderWatcher.Created += logFolderWatcher_Created;
				_logFolderWatcher.Renamed += logFolderWatcher_Renamed;
				_logFolderWatcher.EnableRaisingEvents = true;
				MonitorStatusText = "Monitoring";
				UpdateCountLogEntriesText( );
				UpdateQuickSearchFilterHintText( );

				LoadSettingsFromConfiguration( );

				var refreshThread = new Thread( OnRefreshThread )
				{
					IsBackground = true
				};
				refreshThread.Start( );
			}
			catch ( Exception ex )
			{
				Trace.TraceError( "An error occurred creating the MainWindowViewModel. Error {0}.", ex.ToString( ) );
			}
		}

		#endregion

		#region Properties   

		private readonly CollectionViewSource _logEntries = new CollectionViewSource( );
		private bool _autoscroll = true;
		private string _countLogEntriesText;
		private string _filterText;
		private Dictionary<Guid, string> _guidMap;


		private string _highlightText;
		private bool _isFilterTextInverse;
		private bool _isFilterTextRegex;


		private bool _isHighlightTextRegex;


		private bool _isSearchTextRegex;
		private bool _jumpToError = true;
		private bool _jumpToInformation;
		private bool _jumpToTrace;
		private bool _jumpToWarning;
		private string _monitorStatusText;
		private bool _popupIsOpen;
		private string _popupText;
		private string _quickSearchFilterHintText;
		private string _searchText;


		private string _selectedLogEntryText;
		private EventLogEntryInfo _selectedValue;


		private bool _showErrors = true;


		private bool _showInformation = true;


		private bool _showTrace;
		private bool _showWarnings = true;

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="MainWindowViewModel" /> is autoscroll.
		/// </summary>
		/// <value>
		///     <c>true</c> if autoscroll; otherwise, <c>false</c>.
		/// </value>
		public bool Autoscroll
		{
			get
			{
				return _autoscroll;
			}
			set
			{
				if ( _autoscroll != value )
				{
					_autoscroll = value;
					if ( _autoscroll )
					{
						ScrollGridToEnd( );
					}
					RaisePropertyChanged( ( ) => Autoscroll );
				}
			}
		}

		/// <summary>
		///     Gets or sets the clear log entries command.
		/// </summary>
		/// <value>
		///     The clear log entries command.
		/// </value>
		public ICommand ClearLogEntriesCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the count log entries text.
		/// </summary>
		/// <value>
		///     The count log entries text.
		/// </value>
		public string CountLogEntriesText
		{
			get
			{
				return _countLogEntriesText;
			}
			set
			{
				if ( _countLogEntriesText != value )
				{
					_countLogEntriesText = value;
					RaisePropertyChanged( ( ) => CountLogEntriesText );
				}
			}
		}

        /// <summary>
        /// True to resolve guids, false otherwise
        /// </summary>
        public bool ResolveGuids
        {
            get
            {
                return Settings.Default.ResolveGuids;
            }
            set
            {
                if (Settings.Default.ResolveGuids != value)
                {                    
                    Settings.Default.ResolveGuids = value;
                    Settings.Default.Save();
                    RaisePropertyChanged(() => ResolveGuids);
                }
            }
        }


        /// <summary>
        ///     Gets or sets the exit command.
        /// </summary>
        /// <value>
        ///     The exit command.
        /// </value>
        public ICommand ExitCommand
		{
			get;
			set;
        }

        /// <summary>
        ///     Gets the log path
        /// </summary>
        public string WindowTitle { get; set; } = "Log Viewer";

        /// <summary>
        ///     Sets the path to be shown in the window
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
	    private void SetTitlePath( string path )
        {
            WindowTitle = $"Log Viewer - {path}";
            OnPropertyChanged( new PropertyChangedEventArgs( "WindowTitle" ) );
        }

        /// <summary>
        ///     Gets or sets the filter text.
        /// </summary>
        /// <value>
        ///     The filter text.
        /// </value>
        public string FilterText
		{
			get
			{
				return _filterText;
			}
			set
			{
				if ( _filterText != value )
				{
					_filterText = value;
					if ( IsFilterTextRegex &&
					     !string.IsNullOrEmpty( FilterText ) )
					{
						_filterTextRegex = new Regex( FilterText );
					}
					else
					{
						_filterTextRegex = null;
					}
					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 1000, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 1000, Timeout.Infinite );
						}
					}
					Settings.Default.FilterText = FilterText;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => FilterText );
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the filters command.
		/// </summary>
		/// <value>
		///     The filters command.
		/// </value>
		public ICommand FiltersCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the unique identifier map.
		/// </summary>
		/// <value>
		///     The unique identifier map.
		/// </value>
		public Dictionary<Guid, string> GuidMap
		{
			get
			{
				if ( _guidMap == null )
				{
					lock ( _syncRoot )
					{
						if ( _guidMap == null )
						{
							_guidMap = new Dictionary<Guid, string>( );
						}
					}
				}

				return _guidMap;
			}
		}

		/// <summary>
		///     Gets or sets the highlight text.
		/// </summary>
		/// <value>
		///     The highlight text.
		/// </value>
		public string HighlightText
		{
			get
			{
				return _highlightText;
			}
			set
			{
				if ( _highlightText != value )
				{
					_highlightText = value;
					if ( IsHighlightTextRegex &&
					     !string.IsNullOrEmpty( _highlightText ) )
					{
						_highlightTextRegex = new Regex( _highlightText );
					}
					else
					{
						_highlightTextRegex = null;
					}
					lock ( _syncRoot )
					{
						if ( _highlightTextChangedTimer == null )
						{
							_highlightTextChangedTimer = new Timer( OnHighlightTextChangedTimerCallback, null, 1000, Timeout.Infinite );
						}
						else
						{
							_highlightTextChangedTimer.Change( 1000, Timeout.Infinite );
						}
					}
					Settings.Default.HighlightText = HighlightText;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => HighlightText );
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is filter text inverse.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is filter text inverse; otherwise, <c>false</c>.
		/// </value>
		public bool IsFilterTextInverse
		{
			get
			{
				return _isFilterTextInverse;
			}
			set
			{
				if ( _isFilterTextInverse != value )
				{
					_isFilterTextInverse = value;
					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 1000, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 1000, Timeout.Infinite );
						}
					}
					Settings.Default.IsFilterTextInverse = IsFilterTextInverse;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => IsFilterTextInverse );
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is filter text regex.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is filter text regex; otherwise, <c>false</c>.
		/// </value>
		public bool IsFilterTextRegex
		{
			get
			{
				return _isFilterTextRegex;
			}
			set
			{
				if ( _isFilterTextRegex != value )
				{
					_isFilterTextRegex = value;
					if ( _isFilterTextRegex &&
					     !string.IsNullOrEmpty( FilterText ) )
					{
						_filterTextRegex = new Regex( FilterText );
					}
					else
					{
						_filterTextRegex = null;
						_isFilterTextRegex = false;
					}
					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 1000, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 1000, Timeout.Infinite );
						}
					}
					Settings.Default.IsFilterTextRegex = IsFilterTextRegex;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => IsFilterTextRegex );
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is highlight text regex.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is highlight text regex; otherwise, <c>false</c>.
		/// </value>
		public bool IsHighlightTextRegex
		{
			get
			{
				return _isHighlightTextRegex;
			}
			set
			{
				if ( _isHighlightTextRegex != value )
				{
					_isHighlightTextRegex = value;
					if ( _isHighlightTextRegex &&
					     !string.IsNullOrEmpty( HighlightText ) )
					{
						_highlightTextRegex = new Regex( HighlightText );
					}
					else
					{
						_isHighlightTextRegex = false;
						_highlightTextRegex = null;
					}
					lock ( _syncRoot )
					{
						if ( _highlightTextChangedTimer == null )
						{
							_highlightTextChangedTimer = new Timer( OnHighlightTextChangedTimerCallback, null, 1000, Timeout.Infinite );
						}
						else
						{
							_highlightTextChangedTimer.Change( 1000, Timeout.Infinite );
						}
					}
					Settings.Default.IsHighlightTextRegex = IsHighlightTextRegex;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => IsHighlightTextRegex );
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is search text regex.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is search text regex; otherwise, <c>false</c>.
		/// </value>
		public bool IsSearchTextRegex
		{
			get
			{
				return _isSearchTextRegex;
			}
			set
			{
				if ( _isSearchTextRegex != value )
				{
					_isSearchTextRegex = value;
					if ( _isSearchTextRegex &&
					     !string.IsNullOrEmpty( SearchText ) )
					{
						_searchTextRegex = new Regex( SearchText );
					}
					else
					{
						_isSearchTextRegex = false;
						_searchTextRegex = null;
					}
					Settings.Default.IsSearchTextRegex = IsSearchTextRegex;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => IsSearchTextRegex );
				}
			}
		}


		/// <summary>
		///     Gets or sets a value indicating whether to jump to error.
		/// </summary>
		/// <value>
		///     <c>true</c> to jump to error; otherwise, <c>false</c>.
		/// </value>
		public bool JumpToError
		{
			get
			{
				return _jumpToError;
			}
			set
			{
				if ( _jumpToError != value )
				{
					_jumpToError = value;
					RaisePropertyChanged( ( ) => JumpToError );
				}
			}
		}


		/// <summary>
		///     Gets or sets a value indicating whether to jump to information.
		/// </summary>
		/// <value>
		///     <c>true</c> to jump to information; otherwise, <c>false</c>.
		/// </value>
		public bool JumpToInformation
		{
			get
			{
				return _jumpToInformation;
			}
			set
			{
				if ( _jumpToInformation != value )
				{
					_jumpToInformation = value;
					RaisePropertyChanged( ( ) => JumpToInformation );
				}
			}
		}

		/// <summary>
		///     Gets or sets the jump to next command.
		/// </summary>
		/// <value>
		///     The jump to next command.
		/// </value>
		public ICommand JumpToNextCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the jump to previous command.
		/// </summary>
		/// <value>
		///     The jump to previous command.
		/// </value>
		public ICommand JumpToPreviousCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets a value indicating whether to jump to trace.
		/// </summary>
		/// <value>
		///     <c>true</c> to jump to trace; otherwise, <c>false</c>.
		/// </value>
		public bool JumpToTrace
		{
			get
			{
				return _jumpToTrace;
			}
			set
			{
				if ( _jumpToTrace != value )
				{
					_jumpToTrace = value;
					RaisePropertyChanged( ( ) => JumpToTrace );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to jump to warning.
		/// </summary>
		/// <value>
		///     <c>true</c> to jump to warning; otherwise, <c>false</c>.
		/// </value>
		public bool JumpToWarning
		{
			get
			{
				return _jumpToWarning;
			}
			set
			{
				if ( _jumpToWarning != value )
				{
					_jumpToWarning = value;
					RaisePropertyChanged( ( ) => JumpToWarning );
				}
			}
		}


		/// <summary>
		///     Gets the log entries.
		/// </summary>
		public CollectionViewSource LogEntries
		{
			get
			{
				return _logEntries;
			}
		}

		/// <summary>
		///     Gets or sets the log entries grid loaded command.
		/// </summary>
		/// <value>
		///     The log entries grid loaded command.
		/// </value>
		public ICommand LogEntriesGridLoadedCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the log entry details mouse move command.
		/// </summary>
		/// <value>
		///     The log entry details mouse move command.
		/// </value>
		public ICommand LogEntryDetailsMouseMoveCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the monitor status text.
		/// </summary>
		/// <value>
		///     The monitor status text.
		/// </value>
		public string MonitorStatusText
		{
			get
			{
				return _monitorStatusText;
			}
			set
			{
				if ( _monitorStatusText != value )
				{
					_monitorStatusText = value;
					RaisePropertyChanged( ( ) => MonitorStatusText );
				}
			}
		}

		/// <summary>
		///     Gets or sets the open file command.
		/// </summary>
		/// <value>
		///     The open file command.
		/// </value>
		public ICommand OpenFileCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the open folder command.
		/// </summary>
		/// <value>
		///     The open folder command.
		/// </value>
		public ICommand OpenFolderCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets a value indicating whether the popup is open.
		/// </summary>
		/// <value>
		///     <c>true</c> if the popup is open; otherwise, <c>false</c>.
		/// </value>
		public bool PopupIsOpen
		{
			get
			{
				return _popupIsOpen;
			}
			set
			{
				if ( _popupIsOpen != value )
				{
					_popupIsOpen = value;
					RaisePropertyChanged( ( ) => PopupIsOpen );
				}
			}
		}


		/// <summary>
		///     Gets or sets the popup text.
		/// </summary>
		/// <value>
		///     The popup text.
		/// </value>
		public string PopupText
		{
			get
			{
				return _popupText;
			}
			set
			{
				if ( _popupText != value )
				{
					_popupText = value;
					RaisePropertyChanged( ( ) => PopupText );
				}
			}
		}

		/// <summary>
		///     Gets or sets the quick search filter hint text.
		/// </summary>
		/// <value>
		///     The quick search filter hint text.
		/// </value>
		public string QuickSearchFilterHintText
		{
			get
			{
				return _quickSearchFilterHintText;
			}
			set
			{
				if ( _quickSearchFilterHintText != value )
				{
					_quickSearchFilterHintText = value;
					RaisePropertyChanged( ( ) => QuickSearchFilterHintText );
				}
			}
		}

		/// <summary>
		///     Gets or sets the reload log entries command.
		/// </summary>
		/// <value>
		///     The reload log entries command.
		/// </value>
		public ICommand ReloadLogEntriesCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the save as command.
		/// </summary>
		/// <value>
		///     The save as command.
		/// </value>
		public ICommand SaveAsCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the search online command.
		/// </summary>
		/// <value>
		///     The search online command.
		/// </value>
		public ICommand SearchOnlineCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the search text.
		/// </summary>
		/// <value>
		///     The search text.
		/// </value>
		public string SearchText
		{
			get
			{
				return _searchText;
			}
			set
			{
				if ( _searchText != value )
				{
					_searchText = value;
					if ( IsSearchTextRegex &&
					     !string.IsNullOrEmpty( _searchText ) )
					{
						_searchTextRegex = new Regex( _searchText );
					}
					else
					{
						_searchTextRegex = null;
					}
					Settings.Default.SearchText = SearchText;
					Settings.Default.Save( );
					RaisePropertyChanged( ( ) => SearchText );
				}
			}
		}

		/// <summary>
		///     Gets or sets the search to next command.
		/// </summary>
		/// <value>
		///     The search to next command.
		/// </value>
		public ICommand SearchToNextCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the search to previous command.
		/// </summary>
		/// <value>
		///     The search to previous command.
		/// </value>
		public ICommand SearchToPreviousCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the selected log entry text.
		/// </summary>
		/// <value>
		///     The selected log entry text.
		/// </value>
		public string SelectedLogEntryText
		{
			get
			{
				return _selectedLogEntryText;
			}
			set
			{
				if ( _selectedLogEntryText != value )
				{
					_selectedLogEntryText = value;
					RaisePropertyChanged( ( ) => SelectedLogEntryText );
				}
			}
		}

		/// <summary>
		///     The selected row value.
		/// </summary>
		/// <value>
		///     The selected value.
		/// </value>
		public EventLogEntryInfo SelectedValue
		{
			get
			{
				return _selectedValue;
			}
			set
			{
				if ( _selectedValue != value )
				{
					_selectedValue = value;
					RaisePropertyChanged( ( ) => SelectedValue );

					UpdateSelectedEventLogEntryText( _selectedValue );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show errors.
		/// </summary>
		/// <value>
		///     <c>true</c> to show errors; otherwise, <c>false</c>.
		/// </value>
		public bool ShowErrors
		{
			get
			{
				return _showErrors;
			}
			set
			{
				if ( _showErrors != value )
				{
					_showErrors = value;
					RaisePropertyChanged( ( ) => ShowErrors );

					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 100, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 100, Timeout.Infinite );
						}
					}
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show information.
		/// </summary>
		/// <value>
		///     <c>true</c> to show information; otherwise, <c>false</c>.
		/// </value>
		public bool ShowInformation
		{
			get
			{
				return _showInformation;
			}
			set
			{
				if ( _showInformation != value )
				{
					_showInformation = value;

					RaisePropertyChanged( ( ) => ShowInformation );

					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 100, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 100, Timeout.Infinite );
						}
					}
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show trace.
		/// </summary>
		/// <value>
		///     <c>true</c> to show trace; otherwise, <c>false</c>.
		/// </value>
		public bool ShowTrace
		{
			get
			{
				return _showTrace;
			}
			set
			{
				if ( _showTrace != value )
				{
					_showTrace = value;

					RaisePropertyChanged( ( ) => ShowTrace );

					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 100, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 100, Timeout.Infinite );
						}
					}
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether to show warnings.
		/// </summary>
		/// <value>
		///     <c>true</c> to show warnings; otherwise, <c>false</c>.
		/// </value>
		public bool ShowWarnings
		{
			get
			{
				return _showWarnings;
			}
			set
			{
				if ( _showWarnings != value )
				{
					_showWarnings = value;

					RaisePropertyChanged( ( ) => ShowWarnings );

					lock ( _syncRoot )
					{
						if ( _filterTextChangedTimer == null )
						{
							_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 100, Timeout.Infinite );
						}
						else
						{
							_filterTextChangedTimer.Change( 100, Timeout.Infinite );
						}
					}
					UpdateQuickSearchFilterHintText( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the start monitoring command.
		/// </summary>
		/// <value>
		///     The start monitoring command.
		/// </value>
		public ICommand StartMonitoringCommand
		{
			get;
			set;
		}


		/// <summary>
		///     Gets or sets the stop monitoring command.
		/// </summary>
		/// <value>
		///     The stop monitoring command.
		/// </value>
		public ICommand StopMonitoringCommand
		{
			get;
			set;
		}

		#endregion

		#region Public Methods

		#endregion

		#region Non-Public Methods   

		/// <summary>
		///     Adds the changed file.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		private void AddChangedFile( string filePath )
		{
			lock ( _syncRoot )
			{
				_changedFiles.Add( filePath );
			}
		}

		/// <summary>
		///     Calculates the color of the background.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		public Brush CalculateBackgroundColor( EventLogEntryInfo entry )
		{
			Brush background = new SolidColorBrush( Colors.White );

			lock ( _syncRoot )
			{
				int index = _logEntriesSortedListFiltered.IndexOfKey( entry );
				if ( index % 2 != 0 )
				{
					background = new SolidColorBrush( Colors.Lavender );
				}
			}
			if ( !string.IsNullOrEmpty( HighlightText ) )
			{
				if ( _highlightTextRegex != null )
				{
					if ( _highlightTextRegex.IsMatch( entry.Message ) ||
					     _highlightTextRegex.IsMatch( entry.Process ) ||
					     _highlightTextRegex.IsMatch( entry.Source ) ||
					     _highlightTextRegex.IsMatch( entry.Machine ) ||
					     _highlightTextRegex.IsMatch( entry.TenantName ) ||
					     _highlightTextRegex.IsMatch( entry.UserName ) )
					{
						background = new SolidColorBrush( Colors.Yellow );
					}
				}
				else
				{
					if ( entry.Message.Contains( HighlightText ) ||
					     entry.Process.Contains( HighlightText ) ||
					     entry.Source.Contains( HighlightText ) ||
					     entry.Machine.Contains( HighlightText ) ||
					     entry.TenantName.Contains( HighlightText ) ||
					     entry.UserName.Contains( HighlightText ) )
					{
						background = new SolidColorBrush( Colors.Yellow );
					}
				}
			}

			return background;
		}

		/// <summary>
		///     Determines whether this instance [can execute search online command] the specified text box.
		/// </summary>
		/// <param name="textBox">The text box.</param>
		/// <returns>
		///     <c>true</c> if this instance [can execute search online command] the specified text box; otherwise, <c>false</c>.
		/// </returns>
		private bool CanExecuteSearchOnlineCommand( TextBox textBox )
		{
			bool canExecuteSearchOnlineCommand = false;

			if ( textBox != null )
			{
				canExecuteSearchOnlineCommand = !string.IsNullOrEmpty( textBox.SelectedText );
			}

			return canExecuteSearchOnlineCommand;
		}

		/// <summary>
		///     Determines whether this instance can execute start monitoring command.
		/// </summary>
		/// <returns>
		///     <c>true</c> if this instance can execute start monitoring command; otherwise, <c>false</c>.
		/// </returns>
		private bool CanExecuteStartMonitoringCommand( )
		{
			return ( _logFolderWatcher != null && !_logFolderWatcher.EnableRaisingEvents );
		}


		/// <summary>
		///     Determines whether this instance can execute stop monitoring command.
		/// </summary>
		/// <returns>
		///     <c>true</c> if this instance can execute stop monitoring command; otherwise, <c>false</c>.
		/// </returns>
		private bool CanExecuteStopMonitoringCommand( )
		{
			return ( _logFolderWatcher != null && _logFolderWatcher.EnableRaisingEvents );
		}

		/// <summary>
		///     Determines whether this instance [can save as command].
		/// </summary>
		/// <returns>
		///     <c>true</c> if this instance [can save as command]; otherwise, <c>false</c>.
		/// </returns>
		private bool CanSaveAsCommand( )
		{
			return SelectedValue != null;
		}


		/// <summary>
		///     Executes the clear log entries command.
		/// </summary>
		private void ExecuteClearLogEntriesCommand( )
		{
			lock ( _syncRoot )
			{
				_logEntriesDictionary.Clear( );
				_logEntriesSortedListFiltered.Clear( );
			}

			_minLogEntryDateTime = DateTime.Now;

			_logEntries.View.Refresh( );
		}


		/// <summary>
		///     Executes the exit command.
		/// </summary>
		private void ExecuteExitCommand( )
		{
			Application.Current.Shutdown( );
		}

		/// <summary>
		///     Executes the filters command.
		/// </summary>
		private void ExecuteFiltersCommand( )
		{
			var w = new FiltersWindow( );
			var filtersVm = new FiltersWindowViewModel( );
			filtersVm.SetCurrentFilters( _columnFilters );
			filtersVm.ColumnFiltersChanged += filtersVm_ColumnFiltersChanged;
			w.DataContext = filtersVm;
			w.Owner = Application.Current.MainWindow;
			w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			w.Show( );
		}


		/// <summary>
		///     Executes the jump to next command.
		/// </summary>
		private void ExecuteJumpToNextCommand( )
		{
			int startIndex;

			if ( _selectedRowIndex == -1 )
			{
				startIndex = 0;
			}
			else
			{
				startIndex = _selectedRowIndex + 1;
			}

			if ( startIndex < 0 )
			{
				startIndex = 0;
			}

			for ( int i = startIndex; i < _logEntriesGrid.Items.Count; i++ )
			{
				var info = _logEntriesGrid.Items[ i ] as EventLogEntryInfo;

				if ( info != null )
				{
					EventLogLevel level = info.Level;

					bool matchError = JumpToError && level == EventLogLevel.Error;
					bool matchWarning = JumpToWarning && level == EventLogLevel.Warning;
					bool matchInformation = JumpToInformation && level == EventLogLevel.Information;
					bool matchTrace = JumpToTrace && level == EventLogLevel.Trace;

					if ( matchError ||
					     matchWarning ||
					     matchInformation ||
					     matchTrace )
					{
						_selectedRowIndex = i;
						_logEntriesGrid.SelectedIndex = i;
						_logEntriesGrid.ScrollIntoView( _logEntriesGrid.Items[ i ] );
						UpdateSelectedEventLogEntryText( info );
						break;
					}
				}
			}
		}


		/// <summary>
		///     Executes the jump to previous command.
		/// </summary>
		private void ExecuteJumpToPreviousCommand( )
		{
			int startIndex;

			if ( _selectedRowIndex == -1 )
			{
				startIndex = _logEntriesGrid.Items.Count - 1;
			}
			else
			{
				startIndex = _selectedRowIndex - 1;
			}

			if ( startIndex < 0 )
			{
				startIndex = 0;
			}
			if ( startIndex >= _logEntriesGrid.Items.Count )
			{
				startIndex = _logEntriesGrid.Items.Count - 1;
			}

			for ( int i = startIndex; i >= 0; i-- )
			{
				var info = _logEntriesGrid.Items[ i ] as EventLogEntryInfo;

				if ( info != null )
				{
					EventLogLevel level = info.Level;

					bool matchError = JumpToError && level == EventLogLevel.Error;
					bool matchWarning = JumpToWarning && level == EventLogLevel.Warning;
					bool matchInformation = JumpToInformation && level == EventLogLevel.Information;
					bool matchTrace = JumpToTrace && level == EventLogLevel.Trace;

					if ( matchError ||
					     matchWarning ||
					     matchInformation ||
					     matchTrace )
					{
						_selectedRowIndex = i;
						_logEntriesGrid.SelectedIndex = i;
						_logEntriesGrid.ScrollIntoView( _logEntriesGrid.Items[ i ] );
						UpdateSelectedEventLogEntryText( info );
						break;
					}
				}
			}
		}

		/// <summary>
		///     Executes the log entries grid loaded command.
		/// </summary>
		/// <param name="grid">The grid.</param>
		private void ExecuteLogEntriesGridLoadedCommand( DataGrid grid )
		{
			_logEntriesGrid = grid;
		}

		/// <summary>
		///     Executes the log entry details mouse move command.
		/// </summary>
		/// <param name="textBox">The text box.</param>
		private void ExecuteLogEntryDetailsMouseMoveCommand( TextBox textBox )
		{
			PopupIsOpen = false;

			try
			{
				Point mousePosition = Mouse.GetPosition( textBox );

				Guid id = GetGuidAtCursorPosition( textBox, mousePosition );
				if ( id != Guid.Empty )
				{
					ObjectInfo objectInfo = IdResolver.ResolveId( id );
					if ( objectInfo != null )
					{
						PopupText = objectInfo.ToString( );
						PopupIsOpen = true;
					}
				}
			}
			catch ( Exception exc )
			{
				Trace.TraceError( "MainWindowViewModel.ExecuteLogEntryDetailsMouseMoveCommand failed. Error {0}.", exc.ToString( ) );
			}
		}

		/// <summary>
		///     Executes the open file command.
		/// </summary>
		private void ExecuteOpenFileCommand( )
		{
			var dialog = new OpenFileDialog
			{
				Title = @"Select a log file",
				Filter = @"*Log files (*.xml)|*.xml|All files (*.*)|*.*",
				CheckFileExists = true,
				Multiselect = true
			};
			if ( dialog.ShowDialog( ) == DialogResult.OK )
			{
				foreach ( string file in dialog.FileNames )
				{
					AddChangedFile( file );
				}
			    if ( dialog.FileNames.Count( ) > 1 )
			        SetTitlePath( "Multiple files" );
                else
                    SetTitlePath( dialog.FileNames[ 0 ] );

                _minLogEntryDateTime = DateTime.MinValue;

				_refreshEvent.Set( );
			}
		}


		/// <summary>
		///     Executes the open folder command.
		/// </summary>
		private void ExecuteOpenFolderCommand( )
		{
			var dialog = new FolderBrowserDialog
			{
				RootFolder = Environment.SpecialFolder.MyComputer
			};
			if ( dialog.ShowDialog( ) == DialogResult.OK )
			{
			    SetTitlePath( dialog.SelectedPath );

                string[ ] files = Directory.GetFiles( dialog.SelectedPath, "*.*" );
				foreach ( string file in files )
				{
					AddChangedFile( file );
				}

				_minLogEntryDateTime = DateTime.MinValue;

				_refreshEvent.Set( );
			}
		}

		/// <summary>
		///     Executes the reload log entries command.
		/// </summary>
		private void ExecuteReloadLogEntriesCommand( )
		{
			lock ( _syncRoot )
			{
				_loadExistingFiles = true;
			}

			_minLogEntryDateTime = DateTime.MinValue;

			_refreshEvent.Set( );
		}

		/// <summary>
		///     Executes the save as command.
		/// </summary>
		private void ExecuteSaveAsCommand( )
		{
			if ( SelectedValue == null )
			{
				return;
			}

			var dialog = new SaveFileDialog
			{
				Title = @"Save the current log file as",
				Filter = @"Log files (*.xml)|*.xml|All files (*.*)|*.*",
				OverwritePrompt = true
			};
			if ( dialog.ShowDialog( ) == DialogResult.OK )
			{
				try
				{
					File.Copy( SelectedValue.LogFilePath, dialog.FileName, true );
				}
				catch ( Exception ex )
				{
					Trace.TraceError( "MainWindowViewModel.ExecuteSaveAsCommand failed. Error {0}.", ex.ToString( ) );
				}
			}
		}

		/// <summary>
		///     Executes the search online command.
		/// </summary>
		/// <param name="textBox">The text box.</param>
		private void ExecuteSearchOnlineCommand( TextBox textBox )
		{
			if ( textBox != null &&
			     !string.IsNullOrEmpty( textBox.SelectedText ) )
			{
				var uri = new Uri( "http://www.google.com/search?q=" + textBox.SelectedText );
				Process.Start( uri.ToString( ) );
			}
		}


		/// <summary>
		///     Executes the search to next command.
		/// </summary>
		private void ExecuteSearchToNextCommand( )
		{
			int startIndex;

			if ( _selectedRowIndex == -1 )
			{
				startIndex = 0;
			}
			else
			{
				startIndex = _selectedRowIndex + 1;
			}

			if ( startIndex < 0 )
			{
				startIndex = 0;
			}

			if ( string.IsNullOrEmpty( SearchText ) )
			{
				return;
			}

			for ( int i = startIndex; i < _logEntriesGrid.Items.Count; i++ )
			{
				var info = _logEntriesGrid.Items[ i ] as EventLogEntryInfo;

				if ( _searchTextRegex != null )
				{
					if ( info != null && ( _searchTextRegex.IsMatch( info.Message ) ||
					                       _searchTextRegex.IsMatch( info.Process ) ||
					                       _searchTextRegex.IsMatch( info.Source ) ||
					                       _searchTextRegex.IsMatch( info.Machine ) ||
					                       _searchTextRegex.IsMatch( info.TenantName ) ||
					                       _searchTextRegex.IsMatch( info.UserName ) ) )
					{
						_selectedRowIndex = i;
						_logEntriesGrid.SelectedIndex = i;
						_logEntriesGrid.ScrollIntoView( _logEntriesGrid.Items[ i ] );
						UpdateSelectedEventLogEntryText( info );
						break;
					}
				}
				else
				{
					if ( info != null && ( info.Message.Contains( SearchText ) ||
					                       info.Process.Contains( SearchText ) ||
					                       info.Source.Contains( SearchText ) ||
					                       info.Machine.Contains( SearchText ) ||
					                       info.TenantName.Contains( SearchText ) ||
					                       info.UserName.Contains( SearchText ) ) )
					{
						_selectedRowIndex = i;
						_logEntriesGrid.SelectedIndex = i;
						_logEntriesGrid.ScrollIntoView( _logEntriesGrid.Items[ i ] );
						UpdateSelectedEventLogEntryText( info );
						break;
					}
				}
			}
		}


		/// <summary>
		///     Executes the search to previous command.
		/// </summary>
		private void ExecuteSearchToPreviousCommand( )
		{
			int startIndex;

			if ( _selectedRowIndex == -1 )
			{
				startIndex = _logEntriesGrid.Items.Count - 1;
			}
			else
			{
				startIndex = _selectedRowIndex - 1;
			}

			if ( startIndex < 0 )
			{
				startIndex = 0;
			}
			if ( startIndex >= _logEntriesGrid.Items.Count )
			{
				startIndex = _logEntriesGrid.Items.Count - 1;
			}

			if ( string.IsNullOrEmpty( SearchText ) )
			{
				return;
			}

			for ( int i = startIndex; i >= 0; i-- )
			{
				var info = _logEntriesGrid.Items[ i ] as EventLogEntryInfo;

				if ( _searchTextRegex != null )
				{
					if ( info != null && ( _searchTextRegex.IsMatch( info.Message ) ||
					                       _searchTextRegex.IsMatch( info.Process ) ||
					                       _searchTextRegex.IsMatch( info.Source ) ||
					                       _searchTextRegex.IsMatch( info.Machine ) ||
					                       _searchTextRegex.IsMatch( info.TenantName ) ||
					                       _searchTextRegex.IsMatch( info.UserName ) ) )
					{
						_selectedRowIndex = i;
						_logEntriesGrid.SelectedIndex = i;
						_logEntriesGrid.ScrollIntoView( _logEntriesGrid.Items[ i ] );
						UpdateSelectedEventLogEntryText( info );
						break;
					}
				}
				else
				{
					if ( info != null && ( info.Message.Contains( SearchText ) ||
					                       info.Process.Contains( SearchText ) ||
					                       info.Source.Contains( SearchText ) ||
					                       info.Machine.Contains( SearchText ) ||
					                       info.TenantName.Contains( SearchText ) ||
					                       info.UserName.Contains( SearchText ) ) )
					{
						_selectedRowIndex = i;
						_logEntriesGrid.SelectedIndex = i;
						_logEntriesGrid.ScrollIntoView( _logEntriesGrid.Items[ i ] );
						UpdateSelectedEventLogEntryText( info );
						break;
					}
				}
			}
		}

		/// <summary>
		///     Executes the start monitoring command.
		/// </summary>
		private void ExecuteStartMonitoringCommand( )
		{
			if ( _logFolderWatcher != null &&
			     !_logFolderWatcher.EnableRaisingEvents )
			{
				_logFolderWatcher.EnableRaisingEvents = true;
				MonitorStatusText = "Monitoring";
			}
		}

		/// <summary>
		///     Executes the stop monitoring command.
		/// </summary>
		private void ExecuteStopMonitoringCommand( )
		{
			if ( _logFolderWatcher != null &&
			     _logFolderWatcher.EnableRaisingEvents )
			{
				_logFolderWatcher.EnableRaisingEvents = false;
				MonitorStatusText = "Stopped";
			}
		}

		/// <summary>
		///     Fetches the guid's.
		/// </summary>
		/// <param name="guids">The guid's.</param>
		private void FetchGuids( IEnumerable<Guid> guids )
		{
			try
			{
				const string query = @"
SELECT Id = t.UpgradeId, Alias = a.Data, Name = n.Data FROM (
	SELECT
		Id = MIN( e.Id ), e.UpgradeId
	FROM
		Entity e
	WHERE
		e.UpgradeId IN ({0})
	GROUP BY
		e.UpgradeId
	) t
LEFT JOIN (
	SELECT
		a.EntityId, Data = a.[Namespace] + ':' + a.Data
	FROM
		Data_Alias a
	JOIN
		Data_Alias aa ON a.FieldId = aa.EntityId
	WHERE
		aa.Data = 'alias' AND
		aa.Namespace = 'core'
		) a ON t.Id = a.EntityId
LEFT JOIN (
	SELECT
		n.EntityId, n.Data
	FROM
		Data_NVarChar n
	JOIN
		Data_Alias aa ON n.FieldId = aa.EntityId
	WHERE
		aa.Data = 'name' AND
		aa.Namespace = 'core'
) n ON t.Id = n.EntityId

UNION

SELECT Id = t.EntityUid, Alias = a.Data, Name = n.Data
FROM (
	SELECT
		Id = MIN( e.Id ), e.EntityUid
	FROM
		AppEntity e
	WHERE
		e.EntityUid IN ({0})
	GROUP BY
		e.EntityUid
	) t
LEFT JOIN (
	SELECT
		a.EntityUid, Data = MIN(a.[Namespace] + ':' + a.Data)
	FROM
		AppData_Alias a
	JOIN
		AppData_Alias aa ON a.FieldUid = aa.EntityUid
	WHERE
		aa.Data = 'alias' AND
		aa.Namespace = 'core'
	GROUP BY
		a.EntityUid
		) a ON t.EntityUid = a.EntityUid
LEFT JOIN (
	SELECT
		n.EntityUid, Data = MIN(n.Data)
	FROM
		AppData_NVarChar n
	JOIN
		AppData_Alias aa ON n.FieldUid = aa.EntityUid
	WHERE
		aa.Data = 'name' AND
		aa.Namespace = 'core'
	GROUP BY n.EntityUid
) n ON t.EntityUid = n.EntityUid";

				string guidString = string.Join( ",", guids.Select( g => string.Format( "'{0}'", g ) ) );

				using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
				{
					using ( IDbCommand command = ctx.CreateCommand( string.Format( query, guidString ) ) )
					{
						using ( IDataReader reader = command.ExecuteReader( ) )
						{
							while ( reader.Read( ) )
							{
								Guid guid = reader.GetGuid( 0 );
								string alias = "<no alias>";
								string name = "<no name>";

								if ( ! reader.IsDBNull( 1 ) )
								{
									alias = reader.GetString( 1 );
								}

								if ( !reader.IsDBNull( 2 ) )
								{
									name = reader.GetString( 2 );
								}

								string substitution = string.Format( "{0} ({1} - {2})", name, alias, guid.ToString( "B" ) );

								GuidMap[ guid ] = substitution;
							}
						}
					}
				}
			}
			catch ( Exception exc )
			{
				Trace.TraceError( "MainWindowViewModel.FetchGuids error. Error {0}.", exc.ToString( ) );
			}
		}

		/// <summary>
		///     Gets the GUID at cursor position.
		/// </summary>
		/// <param name="textBox">The text box.</param>
		/// <param name="mousePosition">The mouse position.</param>
		/// <returns></returns>
		private Guid GetGuidAtCursorPosition( TextBox textBox, Point mousePosition )
		{
			Guid id = Guid.Empty;
			int charIndex = textBox.GetCharacterIndexFromPoint( mousePosition, false );
			if ( charIndex >= 0 )
			{
				int lineIndex = textBox.GetLineIndexFromCharacterIndex( charIndex );
				int charIndexForLine = textBox.GetCharacterIndexFromLineIndex( lineIndex );

				int offset = charIndex - charIndexForLine;

				string lineText = textBox.GetLineText( lineIndex );

				MatchCollection matches = Constants.GuidRegEx.Matches( lineText );
				if ( matches.Count > 0 )
				{
					string value = ( from Match m in matches
						where offset >= m.Index && offset < ( m.Index + m.Length )
						select m.Value ).FirstOrDefault( );
					if ( !string.IsNullOrEmpty( value ) )
					{
						Guid.TryParse( value, out id );
					}
				}
			}

			return id;
		}

		/// <summary>
		///     Gets the log entries from file.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		private EventLogEntryCollection GetLogEntriesFromFile( string path )
		{
			var fileLogEntries = new EventLogEntryCollection( );

			if ( !File.Exists( path ) )
			{
				return fileLogEntries;
			}

			fileLogEntries = EventLogHelper.GetEntries( path );

			return fileLogEntries;
		}

		/// <summary>
		///     Gets the log entries from folder.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="maxEntries">The max entries.</param>
		/// <returns></returns>
		private List<EventLogEntryCollection> GetLogEntriesFromFolder( string path, int maxEntries )
		{
			var fileLogEntriesList = new List<EventLogEntryCollection>( );

			if ( !Directory.Exists( path ) )
			{
				return fileLogEntriesList;
			}

			var sortedFiles = new SortedDictionary<DateTime, string>( new DateTimeComparer( true ) );

			string[ ] logFiles = Directory.GetFiles( path );
			foreach ( string logFile in logFiles )
			{
				DateTime writeTime = File.GetLastWriteTime( logFile );
				sortedFiles.Add( writeTime, logFile );
			}

			int countEntries = 0;

			foreach ( string logFile in sortedFiles.Values )
			{
				EventLogEntryCollection logEntries = GetLogEntriesFromFile( logFile );
				fileLogEntriesList.Add( logEntries );

				countEntries += logEntries.Count;

				if ( maxEntries > 0 &&
				     countEntries > maxEntries )
				{
					break;
				}
			}

			return fileLogEntriesList;
		}


		/// <summary>
		///     Determines whether the log entry is accepted.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <returns>
		///     <c>true</c> if the log entry is accepted; otherwise, <c>false</c>.
		/// </returns>
		private bool IsLogEntryAccepted( EventLogEntryInfo info )
		{
			bool isAccepted = true;

			switch ( info.Level )
			{
				case EventLogLevel.Error:
					isAccepted = ShowErrors;
					break;
				case EventLogLevel.Information:
					isAccepted = ShowInformation;
					break;
				case EventLogLevel.Warning:
					isAccepted = ShowWarnings;
					break;
				case EventLogLevel.Trace:
					isAccepted = ShowTrace;
					break;
			}
			if ( isAccepted )
			{
				if ( _filterTextRegex != null )
				{
					isAccepted = _filterTextRegex.IsMatch( info.Message ) ||
					             _filterTextRegex.IsMatch( info.Process ) ||
					             _filterTextRegex.IsMatch( info.Source ) ||
					             _filterTextRegex.IsMatch( info.Machine ) ||
					             _filterTextRegex.IsMatch( info.TenantName ) ||
					             _filterTextRegex.IsMatch( info.UserName );
				}
				else
				{
					if ( !string.IsNullOrEmpty( FilterText ) )
					{
						isAccepted = info.Message.Contains( FilterText ) ||
						             info.Process.Contains( FilterText ) ||
						             info.Source.Contains( FilterText ) ||
						             info.Machine.Contains( FilterText ) ||
						             info.TenantName.Contains( FilterText ) ||
						             info.UserName.Contains( FilterText );
					}
				}
				if ( IsFilterTextInverse )
				{
					isAccepted = !isAccepted;
				}
			}

			if ( isAccepted )
			{
				isAccepted = IsLogEntryAccepted( info, _columnFilters );
			}

			return isAccepted;
		}


		/// <summary>
		///     Determines whether the log entry is accepted.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="columnFilters">The column filters.</param>
		/// <returns>
		///     <c>true</c> if the log entry is accepted; otherwise, <c>false</c>.
		/// </returns>
		private bool IsLogEntryAccepted( EventLogEntryInfo info, IEnumerable<ColumnFilter> columnFilters )
		{
			bool isAccepted = true;

			foreach ( ColumnFilter columnFilter in columnFilters )
			{
				if ( !columnFilter.IsEnabled )
				{
					continue;
				}

				object value = null;

				switch ( columnFilter.ColumnName )
				{
					case "Date":
						value = info.Date;
						break;
					case "Message":
						value = info.Message;
						break;
					case "Process":
						value = info.Process;
						break;
					case "Source":
						value = info.Source;
						break;
					case "ThreadId":
						value = info.ThreadId;
						break;
					case "Machine":
						value = info.Machine;
						break;
					case "TenantName":
						value = info.TenantName;
						break;
					case "UserName":
						value = info.UserName;
						break;
				}

				switch ( columnFilter.Operator )
				{
					case ComparisonOperator.Between:
						if ( value is int )
						{
							var valueAsInt = ( int ) value;
							int compValueMin;
							int compValueMax;

							if ( !int.TryParse( ( string ) columnFilter.MinValue, out compValueMin ) )
							{
								compValueMin = int.MinValue;
							}
							if ( !int.TryParse( ( string ) columnFilter.MinValue, out compValueMax ) )
							{
								compValueMax = int.MaxValue;
							}

							isAccepted = ( valueAsInt >= compValueMin ) && ( valueAsInt <= compValueMax );
						}
						else if ( value is DateTime )
						{
							var valueAsDateTime = ( DateTime ) value;
							var compValueMin = ( DateTime ) columnFilter.MinValue;
							var compValueMax = ( DateTime ) columnFilter.MaxValue;

							isAccepted = ( valueAsDateTime >= compValueMin ) && ( valueAsDateTime <= compValueMax );
						}
						break;
					case ComparisonOperator.BeginsWith:
						if ( value != null )
						{
							isAccepted = value.ToString( ).StartsWith( columnFilter.MinValue.ToString( ) );
						}
						break;
					case ComparisonOperator.Contains:
						if ( value != null )
						{
							isAccepted = value.ToString( ).Contains( columnFilter.MinValue.ToString( ) );
						}
						break;
					case ComparisonOperator.EndsWith:
						if ( value != null )
						{
							isAccepted = value.ToString( ).EndsWith( columnFilter.MinValue.ToString( ) );
						}
						break;
					case ComparisonOperator.Equals:
						if ( value != null )
						{
							isAccepted = value.ToString( ).Equals( columnFilter.MinValue );
						}
						break;
					case ComparisonOperator.GreaterThan:
					case ComparisonOperator.GreaterThanOrEqualTo:
						if ( value is int )
						{
							var valueAsInt = ( int ) value;
							int compValue;
							if ( int.TryParse( ( string ) columnFilter.MinValue, out compValue ) )
							{
								if ( columnFilter.Operator == ComparisonOperator.GreaterThan )
								{
									isAccepted = valueAsInt > compValue;
								}
								else
								{
									isAccepted = valueAsInt >= compValue;
								}
							}
						}
						else if ( value is DateTime )
						{
							var valueAsDateTime = ( DateTime ) value;
							var compValue = ( DateTime ) columnFilter.MinValue;

							if ( columnFilter.Operator == ComparisonOperator.GreaterThan )
							{
								isAccepted = valueAsDateTime > compValue;
							}
							else
							{
								isAccepted = valueAsDateTime >= compValue;
							}
						}
						break;
					case ComparisonOperator.LessThan:
					case ComparisonOperator.LessThanForEqualTo:
						if ( value is int )
						{
							var valueAsInt = ( int ) value;
							int compValue;
							if ( int.TryParse( ( string ) columnFilter.MinValue, out compValue ) )
							{
								if ( columnFilter.Operator == ComparisonOperator.LessThan )
								{
									isAccepted = valueAsInt < compValue;
								}
								else
								{
									isAccepted = valueAsInt <= compValue;
								}
							}
						}
						else if ( value is DateTime )
						{
							var valueAsDateTime = ( DateTime ) value;
							var compValue = ( DateTime ) columnFilter.MinValue;

							if ( columnFilter.Operator == ComparisonOperator.LessThan )
							{
								isAccepted = valueAsDateTime < compValue;
							}
							else
							{
								isAccepted = valueAsDateTime <= compValue;
							}
						}
						break;
					case ComparisonOperator.NotEquals:
						if ( value != null )
						{
							isAccepted = !value.ToString( ).Equals( columnFilter.MinValue );
						}
						break;
				}

				if ( columnFilter.Action == FilterAction.Exclude )
				{
					isAccepted = !isAccepted;
					break;
				}
				if ( columnFilter.Action == FilterAction.Include &&
				     isAccepted )
				{
					break;
				}
			}

			return isAccepted;
		}


		/// <summary>
		///     Loads the log entries from collection.
		/// </summary>
		/// <param name="fileLogEntries">The file log entries.</param>
		private void LoadLogEntriesFromCollection( IEnumerable<EventLogEntry> fileLogEntries )
		{
			foreach ( EventLogEntry logEntry in fileLogEntries )
			{
				if ( logEntry.Date < _minLogEntryDateTime )
				{
					continue;
				}

				var entryInfo = new EventLogEntryInfo( this )
				{
					Date = logEntry.Date.ToLocalTime(),
					Id = logEntry.Id,
					Level = logEntry.Level,
					Machine = logEntry.Machine,
					Message = logEntry.Message,
					Process = logEntry.Process,
					Source = logEntry.Source,
					ThreadId = logEntry.ThreadId,
					Timestamp = logEntry.Timestamp,
					TenantId = logEntry.TenantId,
					TenantName = logEntry.TenantName,
					UserName = logEntry.UserName,
					LogFilePath = logEntry.LogFilePath
				};

				lock ( _syncRoot )
				{
					if ( !_logEntriesDictionary.ContainsKey( entryInfo ) )
					{
						_logEntriesDictionary[ entryInfo ] = entryInfo;

						if ( IsLogEntryAccepted( entryInfo ) )
						{
							entryInfo.IsAccepted = IsLogEntryAccepted( entryInfo );

							if ( !_logEntriesSortedListFiltered.ContainsKey( entryInfo ) )
							{
								_logEntriesSortedListFiltered[ entryInfo ] = entryInfo;
							}
						}
					}
				}
			}
		}


		/// <summary>
		///     Loads the settings from configuration.
		/// </summary>
		private void LoadSettingsFromConfiguration( )
		{
			IsHighlightTextRegex = Settings.Default.IsHighlightTextRegex;
			HighlightText = Settings.Default.HighlightText;
			IsFilterTextInverse = Settings.Default.IsFilterTextInverse;
			IsFilterTextRegex = Settings.Default.IsFilterTextRegex;
			FilterText = Settings.Default.FilterText;
			IsSearchTextRegex = Settings.Default.IsSearchTextRegex;
			SearchText = Settings.Default.SearchText;
		}

		/// <summary>
		///     Called when the filter text changed timer event is raised.
		/// </summary>
		/// <param name="state">The state.</param>
		private void OnFilterTextChangedTimerCallback( object state )
		{
			lock ( _syncRoot )
			{
				foreach ( EventLogEntryInfo info in _logEntriesDictionary.Values )
				{
					info.IsAccepted = IsLogEntryAccepted( info );
				}
			}

			Application.Current.Dispatcher.BeginInvoke( new Action( ( ) =>
			{
				lock ( _syncRoot )
				{
					_logEntriesSortedListFiltered.Clear( );

					foreach ( EventLogEntryInfo info in _logEntriesDictionary.Values )
					{
						if ( info.IsAccepted )
						{
							_logEntriesSortedListFiltered[ info ] = info;
						}
					}
				}

				_logEntries.View.Refresh( );
				ScrollGridToEnd( );
			} ), null);
		}

		/// <summary>
		///     Called when the highlight text changed timer event is raised.
		/// </summary>
		/// <param name="state">The state.</param>
		private void OnHighlightTextChangedTimerCallback( object state )
		{
			Application.Current.Dispatcher.BeginInvoke( new Action( ( ) => _logEntries.View.Refresh( ) ), null );
		}

		/// <summary>
		///     Refresh thread worker method.
		/// </summary>
		private void OnRefreshThread( )
		{
			// These variables are used so that a quick initial load of 1000 records is done.
			// After that all the records are loaded.
			bool initialLoad = true;
			int maxEntriesToLoad = 1000;
			bool loadAll = false;

			while ( true )
			{
				try
				{
					if ( !loadAll )
					{
						_refreshEvent.WaitOne( 5000 );
					}

					var logEntriesList = new List<EventLogEntryCollection>( );

					bool loadExistingFilesLocal;

					lock ( _syncRoot )
					{
						loadExistingFilesLocal = _loadExistingFiles;
						_loadExistingFiles = false;
					}

					if ( loadExistingFilesLocal || loadAll )
					{
						logEntriesList = GetLogEntriesFromFolder( _logPath, maxEntriesToLoad );
					}

					HashSet<string> changedFilesLocal;

					lock ( _syncRoot )
					{
						changedFilesLocal = _changedFiles;
						_changedFiles = new HashSet<string>( );
					}

					if ( changedFilesLocal.Count > 0 )
					{
						logEntriesList.AddRange( changedFilesLocal.Select( EventLogHelper.GetEntries ) );
					}

					if ( logEntriesList.Count > 0 )
					{
						MonitorStatusText = "Refreshing";

						Application.Current.Dispatcher.BeginInvoke( new Action( ( ) =>
						{
							foreach ( EventLogEntryCollection fileLogEntries in logEntriesList )
							{
								LoadLogEntriesFromCollection( fileLogEntries );
							}

							object currentItem = _logEntries.View.CurrentItem;
							_logEntries.View.Refresh( );
							if ( currentItem != null )
							{
								_logEntries.View.MoveCurrentTo( currentItem );
							}
							ScrollGridToEnd( );
						} ), null );

						MonitorStatusText = "Monitoring";
					}

					lock ( _syncRoot )
					{
						_totalLogEntries = _logEntriesDictionary.Count;
						_displayedLogEntries = _logEntriesSortedListFiltered.Count;
					}

					UpdateCountLogEntriesText( );
				}
				catch ( Exception ex )
				{
					Trace.TraceError( "MainWindowViewModel.OnRefreshThread error. Error {0}.", ex.ToString( ) );
				}
				finally
				{
					if ( initialLoad )
					{
						maxEntriesToLoad = -1;
						initialLoad = false;
						// After the quick initial load all the entries
						loadAll = true;
					}
					else
					{
						loadAll = false;
					}
				}
			}
		}

		/// <summary>
		///     Parses the message guid's.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		private string ParseGuids( string message )
		{
			try
			{
				if (!ResolveGuids || string.IsNullOrEmpty( message ) )
				{
					return message;
				}

				const string guidRegex = @"\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b";

				var matches = Regex.Matches( message, guidRegex, RegexOptions.Multiline | RegexOptions.IgnoreCase );

				if ( matches.Count == 0 )
				{
					return message;
				}

				var unmatched = new HashSet<Guid>( );

				foreach ( Match match in matches )
				{
					Guid guid;

					if ( ! Guid.TryParse( match.Value, out guid ) )
					{
						continue;
					}

					if ( ! GuidMap.ContainsKey( guid ) )
					{
						unmatched.Add( guid );
					}
				}

				if ( unmatched.Count > 0 )
				{
					FetchGuids( unmatched );
				}

				using ( DatabaseContext.GetContext( ) )
				{
					message = Regex.Replace( message, guidRegex, ReplaceGuid, RegexOptions.Multiline | RegexOptions.IgnoreCase );
				}
			}
			catch ( Exception exc )
			{
				Trace.TraceError( "MainWindowViewModel.ParseGuids error. Error {0}.", exc.ToString( ) );
			}

			return message;
		}

		/// <summary>
		///     Replaces the unique identifier.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		private string ReplaceGuid( Match match )
		{
			if ( match == null )
			{
				return string.Empty;
			}

			Guid guid;

			if ( ! Guid.TryParse( match.Value, out guid ) )
			{
				return string.Empty;
			}

			string substitution;

			if ( ! GuidMap.TryGetValue( guid, out substitution ) )
			{
				substitution = match.Value;
			}

			return substitution;
		}

		/// <summary>
		///     Scrolls the grid to end.
		/// </summary>
		private void ScrollGridToEnd( )
		{
			if ( Autoscroll &&
			     _logEntriesGrid != null &&
			     _logEntriesGrid.Items.Count > 0 )
			{
				_logEntriesGrid.UpdateLayout( );
				_logEntriesGrid.ScrollIntoView( _logEntriesSortedListFiltered.Values.Last( ) );
			}
		}

		/// <summary>
		///     Updates the count log entries text.
		/// </summary>
		private void UpdateCountLogEntriesText( )
		{
			CountLogEntriesText = string.Format( "Showing {0} of {1} entries", _displayedLogEntries, _totalLogEntries );
		}

		/// <summary>
		///     Updates the quick search filter hint text.
		/// </summary>
		private void UpdateQuickSearchFilterHintText( )
		{
			var text = new StringBuilder( );

			text.Append( "[" );
			text.Append( "Level = " );
			bool addComma = false;
			if ( ShowErrors )
			{
				text.Append( "Errors" );
				addComma = true;
			}
			if ( ShowWarnings )
			{
				if ( addComma )
				{
					text.Append( ", " );
				}
				text.Append( "Warnings" );
				addComma = true;
			}
			if ( ShowInformation )
			{
				if ( addComma )
				{
					text.Append( ", " );
				}
				text.Append( "Info" );
				addComma = true;
			}
			if ( ShowTrace )
			{
				if ( addComma )
				{
					text.Append( ", " );
				}
				text.Append( "Trace" );
			}
			text.Append( "]" );

			if ( !string.IsNullOrEmpty( FilterText ) )
			{
				text.AppendFormat( " [Filter = '{0}'", FilterText );
				if ( IsFilterTextRegex )
				{
					text.Append( ", IsRegex" );
				}
				if ( IsFilterTextInverse )
				{
					text.Append( ", IsInverse" );
				}
				text.Append( "]" );
			}

			if ( !string.IsNullOrEmpty( HighlightText ) )
			{
				text.AppendFormat( " [Highlight = '{0}'", HighlightText );
				if ( IsHighlightTextRegex )
				{
					text.Append( ", IsRegex" );
				}
				text.Append( "]" );
			}

			QuickSearchFilterHintText = text.ToString( );
		}

		/// <summary>
		///     Updates the selected event log entry text.
		/// </summary>
		/// <param name="logEntry">The log entry.</param>
		private void UpdateSelectedEventLogEntryText( EventLogEntryInfo logEntry )
		{
			var builder = new StringBuilder( );
			if ( logEntry != null )
			{
				builder.Append( string.Format( "Date: {0}\n", logEntry.Date.ToString( "o" ) ) );
				builder.Append( string.Format( "Timestamp: {0}\n", logEntry.Timestamp ) );
				builder.Append( string.Format( "Level: {0}\n", logEntry.Level ) );
				builder.Append( string.Format( "Computer: {0}\n", logEntry.Machine ) );
				builder.Append( string.Format( "Process: {0}\n", logEntry.Process ) );
				builder.Append( string.Format( "Thread Id: {0}\n", logEntry.ThreadId ) );
				builder.Append( string.Format( "Source: {0}\n", logEntry.Source ) );
				builder.Append( string.Format( "Message: {0}\n", ParseGuids( logEntry.Message ) ) );
				builder.Append( string.Format( "TenantId: {0}\n", logEntry.TenantId ) );
				builder.Append( string.Format( "TenantName: {0}\n", logEntry.TenantName ) );
				builder.Append( string.Format( "UserName: {0}\n", logEntry.UserName ) );
				builder.Append( string.Format( "Log File Path: {0}\n", logEntry.LogFilePath ?? string.Empty ) );

				SelectedLogEntryText = builder.ToString( );
			}
		}

		/// <summary>
		///     Called when the column filters are changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private void filtersVm_ColumnFiltersChanged( object sender, ColumnFiltersArgs e )
		{
			_columnFilters.Clear( );
			_columnFilters.AddRange( e.ColumnFilters );

			if ( _filterTextChangedTimer == null )
			{
				_filterTextChangedTimer = new Timer( OnFilterTextChangedTimerCallback, null, 100, Timeout.Infinite );
			}
			else
			{
				_filterTextChangedTimer.Change( 100, Timeout.Infinite );
			}
		}

		/// <summary>
		///     Handles the Changed event of the logFolderWatcher control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.IO.FileSystemEventArgs" /> instance containing the event data.</param>
		private void logFolderWatcher_Changed( object sender, FileSystemEventArgs e )
		{
			AddChangedFile( e.FullPath );
		}

		/// <summary>
		///     Handles the Created event of the logFolderWatcher control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.IO.FileSystemEventArgs" /> instance containing the event data.</param>
		private void logFolderWatcher_Created( object sender, FileSystemEventArgs e )
		{
			AddChangedFile( e.FullPath );
		}

		/// <summary>
		///     Handles the Renamed event of the logFolderWatcher control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.IO.RenamedEventArgs" /> instance containing the event data.</param>
		private void logFolderWatcher_Renamed( object sender, RenamedEventArgs e )
		{
			AddChangedFile( e.FullPath );
		}

		#endregion

		#region Fields   

		/// <summary>
		/// </summary>
		private readonly List<ColumnFilter> _columnFilters = new List<ColumnFilter>( );

		/// <summary>
		/// </summary>
		private readonly SortedDictionary<EventLogEntryInfo, EventLogEntryInfo> _logEntriesDictionary = new SortedDictionary<EventLogEntryInfo, EventLogEntryInfo>( new EventLogEntryInfoComparer( ) );

		/// <summary>
		/// </summary>
		private readonly SortedList<EventLogEntryInfo, EventLogEntryInfo> _logEntriesSortedListFiltered = new SortedList<EventLogEntryInfo, EventLogEntryInfo>( new EventLogEntryInfoComparer( ) );


		/// <summary>
		/// </summary>
		private readonly FileSystemWatcher _logFolderWatcher;

		/// <summary>
		/// </summary>
		private readonly string _logPath;

		/// <summary>
		/// </summary>
		private readonly AutoResetEvent _refreshEvent = new AutoResetEvent( true );


		/// <summary>
		/// </summary>
		private readonly object _syncRoot = new object( );

		/// <summary>
		/// </summary>
		private HashSet<string> _changedFiles = new HashSet<string>( );

		/// <summary>
		/// </summary>
		private long _displayedLogEntries;

		/// <summary>
		/// </summary>
		private Timer _filterTextChangedTimer;


		/// <summary>
		/// </summary>
		private Regex _filterTextRegex;

		/// <summary>
		/// </summary>
		private Timer _highlightTextChangedTimer;


		/// <summary>
		/// </summary>
		private Regex _highlightTextRegex;


		/// <summary>
		/// </summary>
		private bool _loadExistingFiles = true;

		/// <summary>
		/// </summary>
		private DataGrid _logEntriesGrid;

		/// <summary>
		/// </summary>
		private DateTime _minLogEntryDateTime = DateTime.MinValue;

		/// <summary>
		/// </summary>
		private Regex _searchTextRegex;

		/// <summary>
		/// </summary>
		private int _selectedRowIndex = -1;


		/// <summary>
		/// </summary>
		private long _totalLogEntries; 

        #endregion
    }
}