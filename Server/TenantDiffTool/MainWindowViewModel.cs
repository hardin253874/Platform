// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using DiffMatchPatch;
using Microsoft.Win32;
using TenantDiffTool.Core;
using TenantDiffTool.SupportClasses;
using TenantDiffTool.SupportClasses.Diff;
using File = TenantDiffTool.SupportClasses.File;

namespace TenantDiffTool
{
	/// <summary>
	/// </summary>
	public class MainWindowViewModel : ViewModelBase
	{
		/// <summary>
		///     No corresponding row.
		/// </summary>
		private const string NoCorrespondingRowToolTip = "No corresponding row";

		/// <summary>
		///     Difference brush
		/// </summary>
		private static readonly SolidColorBrush DifferenceBrush = Brushes.Goldenrod;

		/// <summary>
		///     Missing brush
		/// </summary>
		private static readonly SolidColorBrush MissingBrush = Brushes.LightCoral;

		/// <summary>
		///     Salmon brush.
		/// </summary>
		private static readonly SolidColorBrush EntityDiffBrush = new SolidColorBrush( Color.FromArgb( 255, 240, 192, 192 ) );

		/// <summary>
		///     Background brush.
		/// </summary>
		private static readonly SolidColorBrush BackGroundBrush = Brushes.White;


		/// <summary>
		///     The console output file
		/// </summary>
		private readonly string _consoleOutputFile;


		/// <summary>
		///     The is console mode flag.
		/// </summary>
		private readonly bool _isConsoleMode;


		/// <summary>
		///     The parser.
		/// </summary>
		private readonly CommandLineParser _parser;

		/// <summary>
		///     Busy Message.
		/// </summary>
		private string _busyMessage;

		/// <summary>
		///     Dialog result.
		/// </summary>
		private bool? _closeWindow;

		/// <summary>
		///     Exclude relationship instances.
		/// </summary>
		private bool _excludeRelationshipInstances;

		/// <summary>
		///     The _gen difference enabled
		/// </summary>
		private bool _genDiffEnabled;

		/// <summary>
		///     Ignore xml whitespace.
		/// </summary>
		private bool _ignoreXmlWhitespace;

		/// <summary>
		///     IsBusy
		/// </summary>
		private bool _isBusy;

		private int _lastEntityDiff;

		/// <summary>
		///     Left count string.
		/// </summary>
		private string _leftCountString;

		/// <summary>
		///     Left diff member.
		/// </summary>
		private IList<DiffItem> _leftDiff;

		/// <summary>
		///     Left document.
		/// </summary>
		private FlowDocument _leftDocument;

		/// <summary>
		///     Left selected index.
		/// </summary>
		private int _leftSelectedIndex;

		/// <summary>
		///     Left source.
		/// </summary>
		private ISource _leftSource;

		/// <summary>
		///     Map Image.
		/// </summary>
		private ImageSource _mapImageLeft;

		/// <summary>
		///     Map image.
		/// </summary>
		private ImageSource _mapImageRight;

		/// <summary>
		/// </summary>
		private bool _onlyShowDifferences;

		/// <summary>
		///     Right count string.
		/// </summary>
		private string _rightCountString;

		/// <summary>
		///     Right diff member.
		/// </summary>
		private IList<DiffItem> _rightDiff;

		/// <summary>
		///     Right document
		/// </summary>
		private FlowDocument _rightDocument;

		/// <summary>
		///     Right selected index.
		/// </summary>
		private int _rightSelectedIndex;

		/// <summary>
		///     Right source.
		/// </summary>
		private ISource _rightSource;

		/// <summary>
		///     Search enabled.
		/// </summary>
		private bool _searchEnabled;


		/// <summary>
		///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
		/// </summary>
		public MainWindowViewModel( Window parent )
			: base( parent )
		{
			Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;

			_parser = new CommandLineParser( App.StartupArgs );

			if ( App.StartupArgs != null && App.StartupArgs.Length == 2 && _parser.Count == 0 )
			{
				// Keep existing behaviour
				FileInfo info = new FileInfo( App.StartupArgs[ 0 ] );

				if ( string.Equals( info.Extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
				{
					LeftSource = new XmlFile( App.StartupArgs[ 0 ], DatabaseContext.GetContext( ) );
				}
				else
				{
					LeftSource = new SqliteFile( App.StartupArgs [ 0 ], DatabaseContext.GetContext( ) );
				}

				info = new FileInfo( App.StartupArgs [ 1 ] );

				if ( string.Equals( info.Extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
				{
					RightSource = new XmlFile( App.StartupArgs[ 1 ], DatabaseContext.GetContext( ) );
				}
				else
				{
					RightSource = new SqliteFile( App.StartupArgs [ 1 ], DatabaseContext.GetContext( ) );
				}

				OnlyShowDifferences = true;

				PerformDiff( );
			}
			else if ( App.StartupArgs != null &&
			          _parser.ContainsNonEmptyArgument( "left" ) &&
			          _parser.ContainsNonEmptyArgument( "right" ) &&
			          GetArgument<bool>( "consoleMode" ) )
			{
				try
				{
					var leftSource = GetArgument<string>( "left" );
					var rightSource = GetArgument<string>( "right" );
					_consoleOutputFile = GetArgument<string>( "output" );
					_isConsoleMode = true;

					DeleteOutputFile( );

					WriteToOutputFile( $"Comparing {leftSource} and {rightSource}." );

					FileInfo info = new FileInfo( leftSource );

					if ( string.Equals( info.Extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
					{
						LeftSource = new XmlFile( leftSource, DatabaseContext.GetContext( ) );
					}
					else
					{
						LeftSource = new SqliteFile( leftSource, DatabaseContext.GetContext( ) );
					}

					info = new FileInfo( rightSource );

					if ( string.Equals( info.Extension, ".xml", StringComparison.OrdinalIgnoreCase ) )
					{
						RightSource = new XmlFile( rightSource, DatabaseContext.GetContext( ) );
					}
					else
					{
						RightSource = new SqliteFile( rightSource, DatabaseContext.GetContext( ) );
					}

					OnlyShowDifferences = true;

					Application.Current.MainWindow.Hide( );
					DiffThreadStart( );
					return;
				}
				catch ( Exception ex )
				{
					string msg = $"An unexpected error occurred, exiting. Error {ex}.";
					Trace.TraceError( msg );
					WriteToOutputFile( msg );
					Shutdown( ExitCodes.Error );
				}
			}
			else
			{
				LeftSource = new Empty( DatabaseContext.GetContext( ) );
				RightSource = new Empty( DatabaseContext.GetContext( ) );
			}

			LeftDiffOriginal = new List<DiffItem>( );
			RightDiffOriginal = new List<DiffItem>( );

			SearchCommand = new DelegateCommand<string>( Search );
			CloseCommand = new DelegateCommand( ( ) => CloseWindow = true );
			GenDiffCommand = new DelegateCommand( GenerateDiffFile );
			LeftSourceSelectCommand = new DelegateCommand( ShowLeftSourceSelector );
			RightSourceSelectCommand = new DelegateCommand( ShowRightSourceSelector );
			LeftDoubleClick = new DelegateCommand( ( ) =>
			{
				if ( _leftSelectedIndex >= 0 && LeftDiff.Count > _leftSelectedIndex )
				{
					ShowEntityViewer( LeftDiff[ _leftSelectedIndex ], LeftSource );
				}
			} );

			RightDoubleClick = new DelegateCommand( ( ) =>
			{
				if ( _rightSelectedIndex >= 0 && RightDiff.Count > _rightSelectedIndex )
				{
					ShowEntityViewer( RightDiff[ _rightSelectedIndex ], RightSource );
				}
			} );

			NextDifference = new DelegateCommand( GotoNextDifference );
			PreviousDifference = new DelegateCommand( GotoPreviousDifference );

			BusyMessage = "Processing...";

			SetRowCounts( );
		}

		/// <summary>
		///     Gets or sets a value indicating whether [gen difference enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [gen difference enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool GenDiffEnabled
		{
			get
			{
				return _genDiffEnabled;
			}
			set
			{
				_genDiffEnabled = value;
				RaisePropertyChanged( "GenDiffEnabled" );
			}
		}

		/// <summary>
		///     Gets or sets the busy message.
		/// </summary>
		/// <value>
		///     The busy message.
		/// </value>
		public string BusyMessage
		{
			get
			{
				return _busyMessage;
			}
			set
			{
				if ( _busyMessage != value )
				{
					_busyMessage = value;
					RaisePropertyChanged( "BusyMessage" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the close command.
		/// </summary>
		/// <value>
		///     The close command.
		/// </value>
		public ICommand CloseCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the gen difference command.
		/// </summary>
		/// <value>
		///     The gen difference command.
		/// </value>
		public ICommand GenDiffCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the close window.
		/// </summary>
		/// <value>
		///     The close window.
		/// </value>
		public bool? CloseWindow
		{
			get
			{
				return _closeWindow;
			}
			set
			{
				if ( _closeWindow != value )
				{
					_closeWindow = value;
					RaisePropertyChanged( "CloseWindow" );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [exclude relationship instances].
		/// </summary>
		/// <value>
		///     <c>true</c> if [exclude relationship instances]; otherwise, <c>false</c>.
		/// </value>
		public bool ExcludeRelationshipInstances
		{
			get
			{
				return _excludeRelationshipInstances;
			}
			set
			{
				if ( _excludeRelationshipInstances != value )
				{
					_excludeRelationshipInstances = value;
					RaisePropertyChanged( "ExcludeRelationshipInstances" );

					PerformDiff( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether [ignore XML whitespace].
		/// </summary>
		/// <value>
		///     <c>true</c> if [ignore XML whitespace]; otherwise, <c>false</c>.
		/// </value>
		public bool IgnoreXmlWhitespace
		{
			get
			{
				return _ignoreXmlWhitespace;
			}
			set
			{
				if ( _ignoreXmlWhitespace != value )
				{
					_ignoreXmlWhitespace = value;
					RaisePropertyChanged( "IgnoreXmlWhitespace" );

					PerformDiff( );
				}
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether this instance is busy.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is busy; otherwise, <c>false</c>.
		/// </value>
		public bool IsBusy
		{
			get
			{
				return _isBusy;
			}
			set
			{
				if ( _isBusy != value )
				{
					_isBusy = value;
					RaisePropertyChanged( "IsBusy" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the left count.
		/// </summary>
		/// <value>
		///     The left count.
		/// </value>
		public string LeftCountString
		{
			get
			{
				return _leftCountString;
			}
			set
			{
				if ( _leftCountString != value )
				{
					_leftCountString = value;
					RaisePropertyChanged( "LeftCountString" );
				}
			}
		}

		/// <summary>
		///     Gets the left source.
		/// </summary>
		/// <value>
		///     The left source.
		/// </value>
		public IList<DiffItem> LeftDiff
		{
			get
			{
				return _leftDiff;
			}
			set
			{
				if ( !Equals( _leftDiff, value ) )
				{
					_leftDiff = value;
					RaisePropertyChanged( "LeftDiff" );

					SearchEnabled = LeftDiff != null && RightDiff != null;
				}
			}
		}

		/// <summary>
		///     Gets or sets the left document.
		/// </summary>
		/// <value>
		///     The left document.
		/// </value>
		public FlowDocument LeftDocument
		{
			get
			{
				return _leftDocument;
			}
			set
			{
				if ( !Equals( _leftDocument, value ) )
				{
					_leftDocument = value;
					RaisePropertyChanged( "LeftDocument" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the left double click.
		/// </summary>
		/// <value>
		///     The left double click.
		/// </value>
		public ICommand LeftDoubleClick
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the left selected index.
		/// </summary>
		/// <value>
		///     The left selected.
		/// </value>
		public int LeftSelectedIndex
		{
			get
			{
				return _leftSelectedIndex;
			}
			set
			{
				if ( _leftSelectedIndex != value )
				{
					_leftSelectedIndex = value;
					RaisePropertyChanged( "LeftSelectedIndex" );

					RightSelectedIndex = value;

					PerformEntityDiff( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the left source.
		/// </summary>
		/// <value>
		///     The left source.
		/// </value>
		public ISource LeftSource
		{
			get
			{
				return _leftSource;
			}
			set
			{
				if ( _leftSource != value )
				{
					_leftSource = value;
					RaisePropertyChanged( "LeftSource" );
				}
			}
		}


		/// <summary>
		///     Gets or sets the left source select command.
		/// </summary>
		/// <value>
		///     The left source select command.
		/// </value>
		public ICommand LeftSourceSelectCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the map image.
		/// </summary>
		/// <value>
		///     The map image.
		/// </value>
		public ImageSource MapImageLeft
		{
			get
			{
				return _mapImageLeft;
			}
			set
			{
				if ( !Equals( _mapImageLeft, value ) )
				{
					_mapImageLeft = value;
					RaisePropertyChanged( "MapImageLeft" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the map image right.
		/// </summary>
		/// <value>
		///     The map image right.
		/// </value>
		public ImageSource MapImageRight
		{
			get
			{
				return _mapImageRight;
			}
			set
			{
				if ( !Equals( _mapImageRight, value ) )
				{
					_mapImageRight = value;
					RaisePropertyChanged( "MapImageRight" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the next difference.
		/// </summary>
		/// <value>
		///     The next difference.
		/// </value>
		public ICommand NextDifference
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [only show differences].
		/// </summary>
		/// <value>
		///     <c>true</c> if [only show differences]; otherwise, <c>false</c>.
		/// </value>
		public bool OnlyShowDifferences
		{
			get
			{
				return _onlyShowDifferences;
			}
			set
			{
				if ( _onlyShowDifferences != value )
				{
					_onlyShowDifferences = value;
					RaisePropertyChanged( "OnlyShowDifferences" );

					if ( !_isConsoleMode )
					{
						ProcessDifferencesOnly( );
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the previous difference.
		/// </summary>
		/// <value>
		///     The previous difference.
		/// </value>
		public ICommand PreviousDifference
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the right count.
		/// </summary>
		/// <value>
		///     The right count.
		/// </value>
		public string RightCountString
		{
			get
			{
				return _rightCountString;
			}
			set
			{
				if ( _rightCountString != value )
				{
					_rightCountString = value;
					RaisePropertyChanged( "RightCountString" );
				}
			}
		}

		/// <summary>
		///     Gets the right diff.
		/// </summary>
		/// <value>
		///     The right diff.
		/// </value>
		public IList<DiffItem> RightDiff
		{
			get
			{
				return _rightDiff;
			}
			set
			{
				if ( !Equals( _rightDiff, value ) )
				{
					_rightDiff = value;
					RaisePropertyChanged( "RightDiff" );

					SearchEnabled = LeftDiff != null && RightDiff != null;
				}
			}
		}

		/// <summary>
		///     Gets or sets the right document.
		/// </summary>
		/// <value>
		///     The right document.
		/// </value>
		public FlowDocument RightDocument
		{
			get
			{
				return _rightDocument;
			}
			set
			{
				if ( !Equals( _rightDocument, value ) )
				{
					_rightDocument = value;
					RaisePropertyChanged( "RightDocument" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the right double click.
		/// </summary>
		/// <value>
		///     The right double click.
		/// </value>
		public ICommand RightDoubleClick
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the right selected index.
		/// </summary>
		/// <value>
		///     The right selected.
		/// </value>
		public int RightSelectedIndex
		{
			get
			{
				return _rightSelectedIndex;
			}
			set
			{
				if ( _rightSelectedIndex != value )
				{
					_rightSelectedIndex = value;
					RaisePropertyChanged( "RightSelectedIndex" );

					LeftSelectedIndex = value;

					PerformEntityDiff( );
				}
			}
		}

		/// <summary>
		///     Gets or sets the right source.
		/// </summary>
		/// <value>
		///     The right source.
		/// </value>
		public ISource RightSource
		{
			get
			{
				return _rightSource;
			}
			set
			{
				if ( _rightSource != value )
				{
					_rightSource = value;
					RaisePropertyChanged( "RightSource" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the right source select command.
		/// </summary>
		/// <value>
		///     The right source select command.
		/// </value>
		public ICommand RightSourceSelectCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the search command.
		/// </summary>
		/// <value>
		///     The search command.
		/// </value>
		public ICommand SearchCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [search enabled].
		/// </summary>
		/// <value>
		///     <c>true</c> if [search enabled]; otherwise, <c>false</c>.
		/// </value>
		public bool SearchEnabled
		{
			get
			{
				return _searchEnabled;
			}
			set
			{
				if ( _searchEnabled != value )
				{
					_searchEnabled = value;
					RaisePropertyChanged( "SearchEnabled" );
				}
			}
		}

		/// <summary>
		///     Gets or sets the search string.
		/// </summary>
		/// <value>
		///     The search string.
		/// </value>
		public string SearchString
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the left diff original.
		/// </summary>
		/// <value>
		///     The left diff original.
		/// </value>
		private IList<DiffItem> LeftDiffOriginal
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the right diff original.
		/// </summary>
		/// <value>
		///     The right diff original.
		/// </value>
		private IList<DiffItem> RightDiffOriginal
		{
			get;
			set;
		}

		/// <summary>
		///     Formats the XML.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static String FormatXml( string xml )
		{
			using ( var mStream = new MemoryStream( ) )
			{
				using ( var writer = new XmlTextWriter( mStream, Encoding.UTF8 ) )
				{
					var document = new XmlDocument( );

					try
					{
						/////
						// Load the XmlDocument with the XML.
						/////
						document.LoadXml( xml );

						writer.Formatting = Formatting.Indented;

						/////
						// Write the XML into a formatting XmlTextWriter
						/////
						document.WriteContentTo( writer );
						writer.Flush( );
						mStream.Flush( );

						/////
						// Have to rewind the MemoryStream in order to read its contents.
						/////
						mStream.Position = 0;

						/////
						// Read MemoryStream contents into a StreamReader.
						/////
						var sReader = new StreamReader( mStream );

						/////
						// Extract the text from the StreamReader.
						/////
						return sReader.ReadToEnd( );
					}
					catch ( XmlException )
					{
					}

					return xml;
				}
			}
		}

		public double[ ] Scale( double[ ] arr, double min, double max )
		{
			double m = ( max - min ) / ( arr.Max( ) - arr.Min( ) );
			double c = min - arr.Min( ) * m;
			var newarr = new double[arr.Length];
			for ( int i = 0; i < newarr.Length; i++ )
			{
				newarr[ i ] = m * arr[ i ] + c;
			}
			return newarr;
		}

		/// <summary>
		///     Calculates the diff.
		/// </summary>
		/// <typeparam name="TLeft">The type of the left.</typeparam>
		/// <typeparam name="TRight">The type of the right.</typeparam>
		/// <typeparam name="TInner">The type of the inner.</typeparam>
		/// <param name="leftInput">The left input.</param>
		/// <param name="rightInput">The right input.</param>
		/// <param name="leftDiff">The left diff.</param>
		/// <param name="rightDiff">The right diff.</param>
		/// <param name="conditions">The conditions.</param>
		/// <param name="differences">The differences.</param>
		/// <param name="getData">The get data.</param>
		/// <param name="getType">Type of the get.</param>
		/// <param name="getTooltip">The get tooltip.</param>
		private static void CalculateDiff<TLeft, TRight, TInner>( TLeft leftInput, TRight rightInput, List<DiffItem> leftDiff, List<DiffItem> rightDiff, List<Func<TInner, TInner, int>> conditions, List<Func<TInner, TInner, bool>> differences, Func<TInner, string> getData, Func<TInner, string> getType, Func<TInner, string> getTooltip )
			where TLeft : IList<TInner>
			where TRight : IList<TInner>
			where TInner : DiffBase
		{
			int leftIndex = 0;
			int rightIndex = 0;

			while ( leftIndex < leftInput.Count )
			{
				TInner left = leftInput[ leftIndex ];

				if ( rightIndex < rightInput.Count )
				{
					TInner right = rightInput[ rightIndex ];

					bool success = true;

					foreach ( var condition in conditions )
					{
						int result = condition( left, right );

						if ( result < 0 )
						{
							var leftItem = new DiffItem
							{
								Data = getData( left ),
								Type = getType( left ),
								ToolTip = getTooltip( left ),
								Source = left,
								RowBackground = BackGroundBrush
							};

							leftDiff.Add( leftItem );

							var rightItem = new DiffItem
							{
								RowBackground = MissingBrush,
								ToolTip = NoCorrespondingRowToolTip
							};

							rightDiff.Add( rightItem );

							leftIndex++;

							success = false;

							break;
						}

						if ( result > 0 )
						{
							var leftItem = new DiffItem
							{
								RowBackground = MissingBrush,
								ToolTip = NoCorrespondingRowToolTip
							};

							leftDiff.Add( leftItem );

							var rightItem = new DiffItem
							{
								Data = getData( right ),
								Type = getType( right ),
								ToolTip = getTooltip( right ),
								Source = right,
								RowBackground = BackGroundBrush
							};

							rightDiff.Add( rightItem );

							rightIndex++;

							success = false;

							break;
						}
					}

					if ( success )
					{
						var leftItem = new DiffItem
						{
							Data = getData( left ),
							Type = getType( left ),
							ToolTip = getTooltip( left ),
							Source = left,
							RowBackground = BackGroundBrush
						};

						leftDiff.Add( leftItem );

						var rightItem = new DiffItem
						{
							Data = getData( right ),
							Type = getType( right ),
							ToolTip = getTooltip( right ),
							Source = right,
							RowBackground = BackGroundBrush
						};

						rightDiff.Add( rightItem );

						if ( differences != null && !differences.All( difference => difference( left, right ) ) )
						{
							leftItem.RowBackground = DifferenceBrush;
							rightItem.RowBackground = DifferenceBrush;
						}

						leftIndex++;
						rightIndex++;
					}
				}
				else
				{
					var leftItem = new DiffItem
					{
						Data = getData( left ),
						Type = getType( left ),
						ToolTip = getTooltip( left ),
						Source = left,
						RowBackground = BackGroundBrush
					};

					leftDiff.Add( leftItem );

					var rightItem = new DiffItem
					{
						RowBackground = MissingBrush,
						ToolTip = NoCorrespondingRowToolTip
					};

					rightDiff.Add( rightItem );

					leftIndex++;
				}
			}

			while ( rightIndex < rightInput.Count )
			{
				TInner right = rightInput[ rightIndex ];

				var rightItem = new DiffItem
				{
					Data = getData( right ),
					Type = getType( right ),
					ToolTip = getTooltip( right ),
					Source = right,
					RowBackground = BackGroundBrush
				};

				rightDiff.Add( rightItem );

				var leftItem = new DiffItem
				{
					RowBackground = MissingBrush,
					ToolTip = NoCorrespondingRowToolTip
				};

				leftDiff.Add( leftItem );

				rightIndex++;
			}
		}


		/// <summary>
		///     Currents the on dispatcher unhandled exception.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="dispatcherUnhandledExceptionEventArgs">
		///     The <see cref="DispatcherUnhandledExceptionEventArgs" /> instance
		///     containing the event data.
		/// </param>
		private void CurrentOnDispatcherUnhandledException( object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs )
		{
			if ( _isConsoleMode )
			{
				if ( dispatcherUnhandledExceptionEventArgs?.Exception != null )
				{
					string msg = $"An unexpected error occurred, exiting. Error {dispatcherUnhandledExceptionEventArgs.Exception}.";
					Trace.TraceError( msg );
					WriteToOutputFile( msg );
				}
				Shutdown( ExitCodes.Error );
			}
		}


		/// <summary>
		///     Deletes the output file.
		/// </summary>
		private void DeleteOutputFile( )
		{
			if ( string.IsNullOrWhiteSpace( _consoleOutputFile ) )
			{
				return;
			}

			if ( System.IO.File.Exists( _consoleOutputFile ) )
			{
				System.IO.File.Delete( _consoleOutputFile );
			}
		}

		/// <summary>
		///     Differences the data.
		/// </summary>
		/// <param name="leftSource">The left source.</param>
		/// <param name="rightSource">The right source.</param>
		/// <param name="leftDiff">The left diff.</param>
		/// <param name="rightDiff">The right diff.</param>
		private void DiffData( ISource leftSource, ISource rightSource, List<DiffItem> leftDiff, List<DiffItem> rightDiff )
		{
			IList<Data> leftData = leftSource.Data ?? leftSource.GetData( );
			IList<Data> rightData = rightSource.Data ?? rightSource.GetData( );

			const string tooltip = @"Entity Name: {0}
Field Name: {1}
Data: {2}
Entity Id: {3}
Field Id: {4}";

			var conditions = new List<Func<Data, Data, int>>
			{
				( a, b ) => a.EntityUpgradeId.CompareTo( b.EntityUpgradeId ),
				( a, b ) => a.FieldUpgradeId.CompareTo( b.FieldUpgradeId )
			};

			var differences = new List<Func<Data, Data, bool>>
			{
				( a, b ) =>
				{
					/////
					// Special processing for dates
					/////
					if ( a.Type == "DateTime" )
					{
						DateTime dt1;
						DateTime dt2;

						if ( DateTime.TryParse( a.Value.ToString( ), out dt1 ) && DateTime.TryParse( b.Value.ToString( ), out dt2 ) )
						{
							/////
							// Ignore seconds and milliseconds.
							/////
							return dt1.AddMilliseconds( -dt1.Millisecond ).AddSeconds( -dt1.Second ) == dt2.AddMilliseconds( -dt2.Millisecond ).AddSeconds( -dt2.Second );
							//return dt1.Date == dt2.Date;
						}

						return a.Value.Equals( b.Value );
					}

					/////
					// Special processing for decimals.
					/////
					if ( a.Type == "Decimal" )
					{
						Decimal d1;
						Decimal d2;

						if ( Decimal.TryParse( a.Value.ToString( ), out d1 ) && Decimal.TryParse( b.Value.ToString( ), out d2 ) )
						{
							return d1 == d2;
						}

						return a.Value.Equals( b.Value );
					}

					if ( a.Type == "Guid" )
					{
						return String.Compare( ( string ) a.Value, ( string ) b.Value, StringComparison.OrdinalIgnoreCase ) == 0;
					}

					/////
					// Special processing for xml.
					/////
					if ( a.Type == "Xml" && IgnoreXmlWhitespace )
					{
						string xml1 = FormatXml( ( string ) a.Value );
						string xml2 = FormatXml( ( string ) b.Value );

						return xml1.Equals( xml2 );
					}

					/////
					// Special processing for strings.
					/////
					if ( a.Type == "NVarChar" )
					{
						bool equals = Regex.Replace(a.Value.ToString( ), @"\r\n|\n\r|\n|\r", "\r\n").Equals( Regex.Replace(b.Value.ToString( ), @"\r\n|\n\r|\n|\r", "\r\n") );

						return equals;
					}

					bool result = a.Value.Equals( b.Value );

					return result;
				}
			};

			CalculateDiff( leftData, rightData, leftDiff, rightDiff, conditions, differences, a => a.Value.ToString( ), a => a.Type, a => string.Format( tooltip, a.EntityName, a.FieldName, a.Value.ToString( ).Left( 200 ), a.EntityUpgradeId.ToString( "B" ), a.FieldUpgradeId.ToString( "B" ) ) );
		}

		/// <summary>
		///     Differences the entities.
		/// </summary>
		/// <param name="leftSource">The left source.</param>
		/// <param name="rightSource">The right source.</param>
		/// <param name="leftDiff">The left diff.</param>
		/// <param name="rightDiff">The right diff.</param>
		private void DiffEntities( ISource leftSource, ISource rightSource, List<DiffItem> leftDiff, List<DiffItem> rightDiff )
		{
			IList<Entity> leftEntities = leftSource.Entities ?? leftSource.GetEntities( ExcludeRelationshipInstances );
			IList<Entity> rightEntities = rightSource.Entities ?? rightSource.GetEntities( ExcludeRelationshipInstances );

			const string tooltip = @"Entity Name: {0}
Entity Id: {1}";

			var conditions = new List<Func<Entity, Entity, int>>
			{
				( a, b ) => a.EntityUpgradeId.CompareTo( b.EntityUpgradeId )
			};

			CalculateDiff( leftEntities, rightEntities, leftDiff, rightDiff, conditions, null, a => a.Name, a => "Entity", a => string.Format( tooltip, a.Name, a.EntityUpgradeId.ToString( "B" ) ) );
		}

		/// <summary>
		///     Differences the relationships.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <param name="leftDiff">The left diff.</param>
		/// <param name="rightDiff">The right diff.</param>
		private void DiffRelationships( ISource left, ISource right, List<DiffItem> leftDiff, List<DiffItem> rightDiff )
		{
			IList<Relationship> leftRelationships = left.Relationships ?? left.GetRelationships( ExcludeRelationshipInstances );
			IList<Relationship> rightRelationships = right.Relationships ?? right.GetRelationships( ExcludeRelationshipInstances );

			const string tooltip = @"From Name: {0}
From Id: {1}
Type Name: {2}
Type Id: {3}
To Name: {4}
To Id: {5}";

			var conditions = new List<Func<Relationship, Relationship, int>>
			{
				( a, b ) => a.FromUpgradeId.CompareTo( b.FromUpgradeId ),
				( a, b ) => a.TypeUpgradeId.CompareTo( b.TypeUpgradeId ),
				( a, b ) => a.ToUpgradeId.CompareTo( b.ToUpgradeId )
			};

			CalculateDiff( leftRelationships, rightRelationships, leftDiff, rightDiff, conditions, null, a => $"{a.FromName ?? "<NoName>"} -> ({a.TypeName ?? "<NoName>"}) -> {a.ToName ?? "<NoName>"}", a => "Relationship", a => string.Format( tooltip, a.FromName, a.FromUpgradeId.ToString( "B" ), a.TypeName, a.TypeUpgradeId.ToString( "B" ), a.ToName, a.ToUpgradeId.ToString( "B" ) ) );
		}

		/// <summary>
		///     Differences the thread start.
		/// </summary>
		private void DiffThreadStart( )
		{
			var leftDiff = new List<DiffItem>( );
			var rightDiff = new List<DiffItem>( );

			DiffEntities( LeftSource, RightSource, leftDiff, rightDiff );
			DiffRelationships( LeftSource, RightSource, leftDiff, rightDiff );
			DiffData( LeftSource, RightSource, leftDiff, rightDiff );

			LeftDiffOriginal = new List<DiffItem>( );
			RightDiffOriginal = new List<DiffItem>( );

			foreach ( DiffItem i in leftDiff )
			{
				LeftDiffOriginal.Add( i );
			}

			foreach ( DiffItem i in rightDiff )
			{
				RightDiffOriginal.Add( i );
			}

			if ( OnlyShowDifferences )
			{
				ProcessDifferencesThreadStart( );
			}
			else
			{
				LeftDiff = LeftDiffOriginal;
				RightDiff = RightDiffOriginal;

				SetRowCounts( );

				GenerateMap( );
			}

			IsBusy = false;

			if ( _isConsoleMode )
			{
				int exitCode = LeftDiff.Any( ) || RightDiff.Any( ) ? ExitCodes.Different : ExitCodes.Identical;

				string msg = exitCode == ExitCodes.Identical ? "The data sources do not have differences." : "The data sources have differences.";
				Trace.TraceInformation( msg );
				WriteToOutputFile( msg );

				Shutdown( exitCode );
			}

			var leftFile = LeftSource as File;
			var rightFile = RightSource as File;

			GenDiffEnabled = leftFile != null && rightFile != null;
		}

		/// <summary>
		///     Filters this instance.
		/// </summary>
		private void Filter( )
		{
			string searchText = SearchString;

			IList<DiffItem> leftSource;
			IList<DiffItem> rightSource;

			if ( string.IsNullOrEmpty( searchText ) )
			{
				if ( OnlyShowDifferences )
				{
					leftSource = new List<DiffItem>( );
					rightSource = new List<DiffItem>( );

					for ( int i = 0; i < LeftDiffOriginal.Count; i++ )
					{
						DiffItem left = LeftDiffOriginal[ i ];
						DiffItem right = RightDiffOriginal[ i ];

						if ( Equals( left.RowBackground, MissingBrush ) || Equals( left.RowBackground, DifferenceBrush ) || Equals( right.RowBackground, MissingBrush ) || Equals( right.RowBackground, DifferenceBrush ) )
						{
							leftSource.Add( left );
							rightSource.Add( right );
						}
					}
				}
				else
				{
					leftSource = LeftDiffOriginal;
					rightSource = RightDiffOriginal;
				}
			}
			else
			{
				leftSource = new List<DiffItem>( );
				rightSource = new List<DiffItem>( );

				for ( int i = 0; i < LeftDiffOriginal.Count; i++ )
				{
					DiffItem left = LeftDiffOriginal[ i ];
					DiffItem right = RightDiffOriginal[ i ];

					if ( ( left.Type != null && left.Type.IndexOf( searchText, StringComparison.OrdinalIgnoreCase ) >= 0 ) ||
					     ( left.Data != null && left.Data.IndexOf( searchText, StringComparison.OrdinalIgnoreCase ) >= 0 ) ||
					     ( right.Type != null && right.Type.IndexOf( searchText, StringComparison.OrdinalIgnoreCase ) >= 0 ) ||
					     ( right.Data != null && right.Data.IndexOf( searchText, StringComparison.OrdinalIgnoreCase ) >= 0 ) )
					{
						if ( OnlyShowDifferences )
						{
							if ( Equals( left.RowBackground, MissingBrush ) || Equals( left.RowBackground, DifferenceBrush ) || Equals( right.RowBackground, MissingBrush ) || Equals( right.RowBackground, DifferenceBrush ) )
							{
								leftSource.Add( left );
								rightSource.Add( right );
							}
						}
						else
						{
							leftSource.Add( left );
							rightSource.Add( right );
						}
					}
				}
			}

			LeftDiff = leftSource;
			RightDiff = rightSource;

			GenerateMap( );

			SetRowCounts( );
		}

		/// <summary>
		///     Generates the difference file.
		/// </summary>
		private void GenerateDiffFile( )
		{
			if ( LeftDiffOriginal == null || RightDiffOriginal == null )
			{
				return;
			}

			SaveFileDialog dlg = new SaveFileDialog
			{
				FileName = "diff",
				DefaultExt = ".sql",
				Filter = "SQLite Server files (.sql)|*.sql"
			};

			// Show save file dialog box
			bool? result = dlg.ShowDialog( );

			if ( result == true )
			{
				StringBuilder sb = new StringBuilder( );
				sb.AppendLine( $"-- Differences between '{LeftSource.SourceString}' and '{RightSource.SourceString}'" );
				sb.AppendLine( );

				StringBuilder entityString = new StringBuilder( );
				entityString.AppendLine( );

				StringBuilder relationshipString = new StringBuilder( );
				relationshipString.AppendLine( );

				StringBuilder fieldString = new StringBuilder( );
				fieldString.AppendLine( );

				for ( int i = 0; i < RightDiffOriginal.Count; i++ )
				{
					DiffItem rightItem = RightDiffOriginal[ i ];
					DiffItem leftItem = LeftDiffOriginal[ i ];

					Data field;

					if ( Equals( leftItem.RowBackground, MissingBrush ) )
					{
						switch ( rightItem.Type )
						{
							case "Entity":
								Entity entity = rightItem.Source as Entity;

								if ( entity != null )
								{
									entityString.AppendLine( $"INSERT INTO _Entity ( Uid ) VALUES ( '{entity.EntityUpgradeId}' );" );
								}
								break;
							case "Relationship":
								Relationship rel = rightItem.Source as Relationship;

								if ( rel != null )
								{
									relationshipString.AppendLine( $"INSERT INTO _Relationship ( FromUid, TypeUid, ToUid ) VALUES ( '{rel.FromUpgradeId}', '{rel.TypeUpgradeId}', '{rel.ToUpgradeId}' );" );
								}
								break;
							case "Alias":
								field = rightItem.Source as Data;

								string[ ] value = field?.Value?.ToString( ).Split( ':' );

								if ( value?.Length == 3 )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_Alias (EntityUid, FieldUid, Data, Namespace, AliasMarkerId ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', '{value[ 1 ]}', '{value[ 0 ]}', {value[ 2 ]} );" );
								}
								break;
							case "Bit":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', {field.Value} );" );
								}
								break;
							case "DateTime":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', '{field.Value}' );" );
								}
								break;
							case "Decimal":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', {field.Value} );" );
								}
								break;
							case "Guid":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', '{field.Value}' );" );
								}
								break;
							case "Int":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', {field.Value} );" );
								}
								break;
							case "NVarChar":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', '{field.Value.ToString( ).Replace( "'", "''" )}' );" );
								}
								break;
							case "Xml":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"INSERT INTO _Data_{rightItem.Type} (EntityUid, FieldUid, Data ) VALUES ( '{field.EntityUpgradeId}', '{field.FieldUpgradeId}', '{field.Value.ToString( ).Replace( "'", "''" )}' );" );
								}
								break;
							default:
								throw new InvalidOperationException( "Unexpected data type" );
						}
					}
					else if ( Equals( rightItem.RowBackground, MissingBrush ) )
					{
						switch ( leftItem.Type )
						{
							case "Entity":
								Entity entity = leftItem.Source as Entity;

								if ( entity != null )
								{
									entityString.AppendLine( $"DELETE FROM _Entity WHERE Uid = '{entity.EntityUpgradeId}';" );
								}
								break;
							case "Relationship":
								Relationship rel = leftItem.Source as Relationship;

								if ( rel != null )
								{
									relationshipString.AppendLine( $"DELETE FROM _Relationship WHERE FromUid = '{rel.FromUpgradeId}' AND TypeUid = '{rel.TypeUpgradeId}' AND ToUid = '{rel.ToUpgradeId}';" );
								}
								break;
							case "Alias":
							case "Bit":
							case "DateTime":
							case "Decimal":
							case "Guid":
							case "Int":
							case "NVarChar":
							case "Xml":
								field = leftItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"DELETE FROM _Data_{leftItem.Type} WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							default:
								throw new InvalidOperationException( "Unexpected data type" );
						}
					}
					else if ( Equals( rightItem.RowBackground, DifferenceBrush ) )
					{
						switch ( leftItem.Type )
						{
							case "Alias":
								field = rightItem.Source as Data;

								string[ ] value = field?.Value?.ToString( ).Split( ':' );

								if ( value?.Length == 3 )
								{
									fieldString.AppendLine( $"UPDATE _Data_Alias SET Data = '{value[ 1 ]}', Namespace = '{value[ 0 ]}', AliasMarkerId = '{value[ 2 ]}' WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "Bit":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_Bit SET Data = {field.Value} WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "DateTime":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_DateTime SET Data = '{field.Value}' WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "Decimal":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_Decimal SET Data = {field.Value} WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "Guid":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_Guid SET Data = '{field.Value}' WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "Int":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_Int SET Data = {field.Value} WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "NVarChar":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_NVarChar SET Data = '{field.Value.ToString( ).Replace( "'", "''" )}' WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							case "Xml":
								field = rightItem.Source as Data;

								if ( field?.Value != null )
								{
									fieldString.AppendLine( $"UPDATE _Data_Xml SET Data = '{field.Value.ToString( ).Replace( "'", "''" )}' WHERE EntityUid = '{field.EntityUpgradeId}' AND FieldUid = '{field.FieldUpgradeId}';" );
								}
								break;
							default:
								throw new InvalidOperationException( "Unexpected data type" );
						}
					}
				}


				using ( var sw = System.IO.File.CreateText( dlg.FileName ) )
				{
					sw.Write( sb.ToString( ) );
					sw.Write( entityString.ToString( ) );
					sw.Write( relationshipString.ToString( ) );
					sw.Write( fieldString.ToString( ) );
				}
			}
		}

		/// <summary>
		///     Generates the map.
		/// </summary>
		private void GenerateMap( )
		{
			if ( LeftDiff == null || RightDiff == null )
			{
				return;
			}

			int height = LeftDiff.Count > RightDiff.Count ? LeftDiff.Count : RightDiff.Count;

			if ( height <= 0 )
			{
				return;
			}

			int bitmapHeight = height;

			if ( height > 2000 )
			{
				bitmapHeight = 2000;
			}

			var bitmapLeft = new WriteableBitmap( 1, bitmapHeight, 1, 1, PixelFormats.Bgr32, null );
			var bitmapRight = new WriteableBitmap( 1, bitmapHeight, 1, 1, PixelFormats.Bgr32, null );

			var rectLeft = new Int32Rect( 0, 0, bitmapLeft.PixelWidth, bitmapLeft.PixelHeight );
			var rectRight = new Int32Rect( 0, 0, bitmapRight.PixelWidth, bitmapLeft.PixelHeight );

			int bytesPerPixel = ( bitmapLeft.Format.BitsPerPixel + 7 ) / 8;

			int stride = bitmapLeft.PixelWidth * bytesPerPixel;

			int arraySize = stride * bitmapHeight;
			var colorArrayLeft = new byte[arraySize];
			var colorArrayRight = new byte[arraySize];

			int colorArrayOffset = 0;

			double divisor = height / ( double ) bitmapHeight;
			double current = divisor;

			SolidColorBrush leftBrush = BackGroundBrush;
			SolidColorBrush rightBrush = BackGroundBrush;

			for ( int i = 0; i < height; i++ )
			{
				if ( i > current )
				{
					SetColor( colorArrayLeft, leftBrush, colorArrayOffset );
					SetColor( colorArrayRight, rightBrush, colorArrayOffset );

					current += divisor;
					colorArrayOffset += 4;

					leftBrush = BackGroundBrush;
					rightBrush = BackGroundBrush;
				}

				if ( Equals( leftBrush, BackGroundBrush ) )
				{
					if ( LeftDiff[ i ].RowBackground != null )
					{
						leftBrush = LeftDiff[ i ].RowBackground;
					}
				}

				if ( Equals( rightBrush, BackGroundBrush ) )
				{
					if ( RightDiff[ i ].RowBackground != null )
					{
						rightBrush = RightDiff[ i ].RowBackground;
					}
				}
			}

			while ( colorArrayOffset < colorArrayLeft.Length )
			{
				SetColor( colorArrayLeft, leftBrush, colorArrayOffset );
				SetColor( colorArrayRight, rightBrush, colorArrayOffset );

				colorArrayOffset += 4;
			}

			bitmapLeft.WritePixels( rectLeft, colorArrayLeft, stride, 0 );
			bitmapRight.WritePixels( rectRight, colorArrayRight, stride, 0 );

			bitmapLeft.Freeze( );
			bitmapRight.Freeze( );

			MapImageLeft = bitmapLeft;
			MapImageRight = bitmapRight;
		}

		/// <summary>
		///     Gets the argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException"></exception>
		private T GetArgument<T>( string argumentName )
		{
			T value = default(T);

			if ( _parser.ContainsArgument( argumentName ) )
			{
				value = _parser.ValueForArgument<T>( argumentName );
			}

			return value;
		}

		/// <summary>
		///     Go to the next difference.
		/// </summary>
		private void GotoNextDifference( )
		{
			int current = LeftSelectedIndex;

			current++;

			while ( current < LeftDiff.Count )
			{
				DiffItem left = LeftDiff[ current ];
				DiffItem right = RightDiff[ current ];

				if ( Equals( left.RowBackground, MissingBrush ) || Equals( left.RowBackground, DifferenceBrush ) || Equals( right.RowBackground, MissingBrush ) || Equals( right.RowBackground, DifferenceBrush ) )
				{
					LeftSelectedIndex = current;
					break;
				}

				current++;
			}
		}

		/// <summary>
		///     Go to the previous difference.
		/// </summary>
		private void GotoPreviousDifference( )
		{
			int current = LeftSelectedIndex;

			current--;

			while ( current >= 0 )
			{
				DiffItem left = LeftDiff[ current ];
				DiffItem right = RightDiff[ current ];

				if ( Equals( left.RowBackground, MissingBrush ) || Equals( left.RowBackground, DifferenceBrush ) || Equals( right.RowBackground, MissingBrush ) || Equals( right.RowBackground, DifferenceBrush ) )
				{
					LeftSelectedIndex = current;
					break;
				}

				current--;
			}
		}

		/// <summary>
		///     Measures the string.
		/// </summary>
		/// <param name="doc">The doc.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">No glyph type face found</exception>
		private int MeasureString( FlowDocument doc )
		{
			var typeface = new Typeface( doc.FontFamily,
				doc.FontStyle,
				doc.FontWeight,
				doc.FontStretch );

			GlyphTypeface glyphTypeface;

			if ( !typeface.TryGetGlyphTypeface( out glyphTypeface ) )
			{
				throw new InvalidOperationException( "No glyph type face found" );
			}

			string docText = new TextRange( doc.ContentStart, doc.ContentEnd ).Text;

			string[ ] lines = docText.Split( '\n' );

			int max = 0;

			foreach ( string line in lines )
			{
				double size = doc.FontSize;

				var glyphIndexes = new ushort[line.Length];
				var advanceWidths = new double[line.Length];

				double totalWidth = 0;

				for ( int n = 0; n < line.Length; n++ )
				{
					ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[ line[ n ] ];
					glyphIndexes[ n ] = glyphIndex;

					double width = glyphTypeface.AdvanceWidths[ glyphIndex ] * size;
					advanceWidths[ n ] = width;

					totalWidth += width;
				}

				if ( totalWidth > max )
				{
					max = ( int ) Math.Ceiling( totalWidth ) + 25;
				}
			}

			return max;
		}

		/// <summary>
		///     Performs the diff.
		/// </summary>
		private void PerformDiff( )
		{
			if ( LeftSource == null || RightSource == null )
			{
				return;
			}

			IsBusy = true;

			var workerThread = new Thread( DiffThreadStart )
			{
				IsBackground = true
			};

			workerThread.Start( );
		}

		/// <summary>
		///     Performs the entity diff.
		/// </summary>
		private void PerformEntityDiff( )
		{
			if ( LeftSelectedIndex == _lastEntityDiff )
			{
				return;
			}

			_lastEntityDiff = LeftSelectedIndex;

			if ( LeftDiff == null || RightDiff == null || LeftSelectedIndex < 0 || RightSelectedIndex < 0 || LeftSelectedIndex >= LeftDiff.Count || RightSelectedIndex >= RightDiff.Count )
			{
				LeftDocument = new FlowDocument( );
				RightDocument = new FlowDocument( );

				return;
			}

			DiffItem leftDiffItem = LeftDiff[ LeftSelectedIndex ];
			DiffItem rightDiffItem = RightDiff[ RightSelectedIndex ];

			var dmp = new diff_match_patch( );
			List<Diff> diffs = dmp.diff_main( leftDiffItem.Data ?? string.Empty, rightDiffItem.Data ?? string.Empty );

			/////
			// Left document.
			/////
			var leftDoc = new FlowDocument
			{
				FontFamily = new FontFamily( "Segoe UI" ),
				FontSize = 10
			};

			//leftDoc.PageWidth = 65000;
			var leftParagraph = new Paragraph( );
			leftDoc.Blocks.Add( leftParagraph );

			/////
			// Right document.
			/////
			var rightDoc = new FlowDocument
			{
				FontFamily = new FontFamily( "Segoe UI" ),
				FontSize = 10
			};

			var rightParagraph = new Paragraph( );
			rightDoc.Blocks.Add( rightParagraph );

			foreach ( Diff diff in diffs )
			{
				switch ( diff.operation )
				{
					case Operation.EQUAL:
					{
						var leftSpan = new Span( );
						leftSpan.Inlines.Add( diff.text );
						leftParagraph.Inlines.Add( leftSpan );

						var rightSpan = new Span( );
						rightSpan.Inlines.Add( diff.text );
						rightParagraph.Inlines.Add( rightSpan );
					}
						break;
					case Operation.DELETE:
					{
						var leftSpan = new Span
						{
							Background = EntityDiffBrush
						};

						leftSpan.Inlines.Add( diff.text );
						leftParagraph.Inlines.Add( leftSpan );
					}
						break;
					case Operation.INSERT:
					{
						var rightSpan = new Span
						{
							Background = EntityDiffBrush
						};

						rightSpan.Inlines.Add( diff.text );
						rightParagraph.Inlines.Add( rightSpan );
					}
						break;
				}
			}

			leftDoc.PageWidth = MeasureString( leftDoc );
			rightDoc.PageWidth = MeasureString( rightDoc );

			LeftDocument = leftDoc;
			RightDocument = rightDoc;
		}

		/// <summary>
		///     Processes the differences only.
		/// </summary>
		private void ProcessDifferencesOnly( )
		{
			var thread = new Thread( ProcessDifferencesThreadStart )
			{
				IsBackground = true
			};

			thread.Start( );
		}

		/// <summary>
		///     Processes the differences thread start.
		/// </summary>
		private void ProcessDifferencesThreadStart( )
		{
			Filter( );
		}

		/// <summary>
		///     Searches the specified search text.
		/// </summary>
		/// <param name="searchText">The search text.</param>
		private void Search( string searchText )
		{
			SearchString = searchText;

			var searchThread = new Thread( SearchThreadStart )
			{
				IsBackground = true
			};

			searchThread.Start( );
		}

		/// <summary>
		///     Searches the thread start.
		/// </summary>
		private void SearchThreadStart( )
		{
			Filter( );
		}

		/// <summary>
		///     Sets the colour.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="brush">The brush.</param>
		/// <param name="offset">The offset.</param>
		private void SetColor( byte[ ] array, SolidColorBrush brush, int offset )
		{
			int length = array.Length;

			if ( offset + 4 > length )
			{
				return;
			}

			array[ offset ] = brush.Color.B;
			array[ offset + 1 ] = brush.Color.G;
			array[ offset + 2 ] = brush.Color.R;
			array[ offset + 3 ] = brush.Color.A;
		}

		/// <summary>
		///     Sets the row counts.
		/// </summary>
		private void SetRowCounts( )
		{
			LeftCountString = LeftDiff != null ? $"Total Rows: {LeftDiff.Count( d => d.ToolTip != NoCorrespondingRowToolTip )}" : "Total Rows: 0";
			RightCountString = RightDiff != null ? $"Total Rows: {RightDiff.Count( d => d.ToolTip != NoCorrespondingRowToolTip )}" : "Total Rows: 0";
		}

		/// <summary>
		///     Shows the entity viewer.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="source">The source.</param>
		private void ShowEntityViewer( DiffItem item, ISource source )
		{
			if ( item.Source == null )
			{
				return;
			}

			var viewer = new Viewer( );

			var vm = viewer.DataContext as ViewerViewModel;

			if ( vm != null )
			{
				vm.SetSource( item.Source, source );

				viewer.Owner = ParentWindow;
				viewer.ShowDialog( );
			}
		}

		/// <summary>
		///     Shows the left source selector.
		/// </summary>
		private void ShowLeftSourceSelector( )
		{
			ShowSelector( v => LeftSource = v, ( ) => LeftSource, ( ) => RightSource != null );
		}

		/// <summary>
		///     Shows the right source selector.
		/// </summary>
		private void ShowRightSourceSelector( )
		{
			ShowSelector( v => RightSource = v, ( ) => RightSource, ( ) => LeftSource != null );
		}

		/// <summary>
		///     Shows the selector.
		/// </summary>
		/// <param name="setSourceValues">The set source values.</param>
		/// <param name="getSource">The get source.</param>
		/// <param name="diffDetermination">The diff determination.</param>
		private void ShowSelector( Action<ISource> setSourceValues, Func<ISource> getSource, Func<bool> diffDetermination )
		{
			var sourceSelector = new SourceSelector( getSource( ) );

			var viewModel = sourceSelector.DataContext as SourceSelectorViewModel;

			if ( viewModel != null )
			{
				ISource existingSource = getSource( );

				if ( existingSource != null )
				{
					viewModel.SetExistingSource( existingSource );
				}

				sourceSelector.Owner = ParentWindow;
				sourceSelector.ShowDialog( );

				if ( !viewModel.OkClicked )
				{
					return;
				}

				var provider = ( ISourceProvider ) viewModel;

				ISource source = provider.GetSource( );

				setSourceValues( source );

				if ( diffDetermination( ) )
				{
					PerformDiff( );
				}
			}
		}


		/// <summary>
		///     Shuts down.
		/// </summary>
		/// <param name="exitCode">The exit code.</param>
		private void Shutdown( int exitCode )
		{
			Application.Current.Dispatcher.BeginInvoke( new Action( ( ) => Application.Current.Shutdown( exitCode ) ) );
		}


		/// <summary>
		///     Writes to output file.
		/// </summary>
		/// <param name="text">The text.</param>
		private void WriteToOutputFile( string text )
		{
			if ( string.IsNullOrWhiteSpace( _consoleOutputFile ) )
			{
				return;
			}

			string msg = $"{DateTime.Now} {text} {Environment.NewLine}";
			System.IO.File.AppendAllText( _consoleOutputFile, msg );
		}
	}
}