// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     DatabaseHealth View Model.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class DatabaseHealthViewModel : ViewModelBase
	{
		/// <summary>
		///     The run test text
		/// </summary>
		private const string RunTestText = "Run Tests";

		/// <summary>
		///     The stop test text
		/// </summary>
		private const string StopTestText = "Stop";

		/// <summary>
		///     The _button text
		/// </summary>
		private string _buttonText;

		/// <summary>
		///     The _can run tests
		/// </summary>
		private bool _canRunTests;

		/// <summary>
		///     The catalog
		/// </summary>
		private string _catalog;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The run tests command.
		/// </summary>
		private ICommand _runTests;

		/// <summary>
		///     The _selected suite
		/// </summary>
		private int _selectedSuite;

		/// <summary>
		///     The server
		/// </summary>
		private string _server;

		/// <summary>
		///     The tests
		/// </summary>
		private List<DatabaseTest> _tests;

        /// <summary>
        ///     The tenants
        /// </summary>
        private List<TenantInfo> _tenants;

        /// <summary>
        ///     The tenantId to filter by
        /// </summary>
        private long _filterTenantId = -1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseHealthViewModel" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public DatabaseHealthViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			CanRunTests = true;

			RunTests = new DelegateCommand( OnRunTestsAsync );

			SelectedSuite = 0;
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
            private set
            {
                SetProperty( ref _tenants, value );
            }
        }

        /// <summary>
        ///     Gets the catalog.
        /// </summary>
        /// <value>
        ///     The catalog.
        /// </value>
        public long FilterTenantId
        {
            get
            {
                return _filterTenantId;
            }
            set
            {
                SetProperty( ref _filterTenantId, value );
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance can run tests.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can run tests; otherwise, <c>false</c>.
        /// </value>
        private bool CanRunTests
		{
			set
			{
				SetProperty( ref _canRunTests, value );

				ButtonText = value ? RunTestText : StopTestText;
			}
		}

		/// <summary>
		///     Gets the catalog.
		/// </summary>
		/// <value>
		///     The catalog.
		/// </value>
		public string Catalog
		{
			get
			{
				return _catalog;
			}
			set
			{
				SetProperty( ref _catalog, value );
			}
		}

		/// <summary>
		///     Gets or sets the selected suite.
		/// </summary>
		/// <value>
		///     The selected suite.
		/// </value>
		public int SelectedSuite
		{
			get
			{
				return _selectedSuite;
			}
			set
			{
				SetProperty( ref _selectedSuite, value );
			}
		}

		/// <summary>
		///     Gets or sets the button text.
		/// </summary>
		/// <value>
		///     The button text.
		/// </value>
		public string ButtonText
		{
			get
			{
				return _buttonText;
			}
			private set
			{
				SetProperty( ref _buttonText, value );
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
			private get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				LoadTests( );

                var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );
                var tenants = TenantInfo.LoadTenants( databaseManager );
                tenants.Insert( 0, new TenantInfo( -1, "All Tenants", "All Tenants" ) );
                Tenants = tenants;

				Server = PluginSettings.DatabaseSettings.ServerName;
				Catalog = PluginSettings.DatabaseSettings.CatalogName;
			}
		}

		/// <summary>
		///     Gets or sets the run tests.
		/// </summary>
		/// <value>
		///     The run tests.
		/// </value>
		public ICommand RunTests
		{
			get
			{
				return _runTests;
			}
			set
			{
				SetProperty( ref _runTests, value );
			}
		}

		/// <summary>
		///     Gets the server.
		/// </summary>
		/// <value>
		///     The server.
		/// </value>
		public string Server
		{
			get
			{
				return _server;
			}
			set
			{
				SetProperty( ref _server, value );
			}
		}

		/// <summary>
		///     Gets or sets the tests.
		/// </summary>
		/// <value>
		///     The tests.
		/// </value>
		public List<DatabaseTest> Tests
		{
			get
			{
				return _tests;
			}
			set
			{
				SetProperty( ref _tests, value );
			}
		}

		/// <summary>
		///     Gets or sets the cancellation token source.
		/// </summary>
		/// <value>
		///     The cancellation token source.
		/// </value>
		private CancellationTokenSource CancellationTokenSource
		{
			get;
			set;
		}

		/// <summary>
		///     Generates the j unit report.
		/// </summary>
		/// <returns></returns>
		public string GenerateJUnitReport( DateTime startTime, DateTime endTime )
		{
			StringBuilder sb = new StringBuilder( );

			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true
			};

			using ( XmlWriter writer = XmlWriter.Create( sb, settings ) )
			{
				writer.WriteStartElement( "testsuites" );
				writer.WriteStartElement( "testsuite" );
				writer.WriteAttributeString( "name", "databaseTests" );
				writer.WriteAttributeString( "errors", "0" );
				writer.WriteAttributeString( "tests", Tests.Count( t => t.TestType == "test" ).ToString( ) );
				writer.WriteAttributeString( "failures", Tests.Count( t => t.State == TestState.Failed ).ToString( ) );
				writer.WriteAttributeString( "time", ( endTime - startTime ).TotalSeconds.ToString( CultureInfo.InvariantCulture ) );
				writer.WriteAttributeString( "timestamp", startTime.ToString( "o" ) );

				foreach ( DatabaseTest test in Tests.Where( t => t.TestType == "test" ) )
				{
					writer.WriteStartElement( "testcase" );
					writer.WriteAttributeString( "name", test.Name );
					writer.WriteAttributeString( "time", ( test.EndTime - test.StartTime ).TotalSeconds.ToString( CultureInfo.InvariantCulture ) );

					if ( test.State == TestState.Failed )
					{
						writer.WriteStartElement( "failure" );
						writer.WriteString( PrettyPrint.Print( test.FailureDetails, test.HiddenColumns ) );
						writer.WriteEndElement( );
					}

					writer.WriteEndElement( );
				}

				writer.WriteEndElement( );
				writer.WriteEndElement( );
				writer.Flush( );
			}

			return sb.ToString( );
		}

		/// <summary>
		///     Called when tests are to be run.
		/// </summary>
		public void OnRunTests( bool synchronous = false )
		{
			try
			{
				CanRunTests = false;

				var semaphore = new SemaphoreSlim( Settings.Default.SimultaneousTests );

				var tasks = new List<Task>( );

				int timeout = Settings.Default.CommandTimeout;

				CancellationTokenSource = new CancellationTokenSource( );
				CancellationToken token = CancellationTokenSource.Token;

				foreach ( DatabaseTest test in Tests )
				{
					switch ( SelectedSuite )
					{
						case 0:
							// All
							test.Reset( );

							if ( test.SelectedToRun )
							{
								tasks.Add( test.Run( timeout, semaphore, token, FilterTenantId ) );
							}
							break;
						case 1:
							// Info Only
							if ( test.TestType == "info" )
							{
								test.Reset( );

								if ( test.SelectedToRun )
								{
									tasks.Add( test.Run( timeout, semaphore, token, FilterTenantId ) );
								}
							}
							break;
						case 2:
							// Test Only
							if ( test.TestType == "test" )
							{
								test.Reset( );

								if ( test.SelectedToRun )
								{
									tasks.Add( test.Run( timeout, semaphore, token, FilterTenantId ) );
								}
							}
							break;
						case 3:
							// Failures only
							if ( test.TestType == "test" && test.State == TestState.Failed )
							{
								test.Reset( );

								if ( test.SelectedToRun )
								{
									tasks.Add( test.Run( timeout, semaphore, token, FilterTenantId ) );
								}
							}
							break;
					}
				}

				Task.WhenAll( tasks ).ContinueWith( t =>
				{
					CanRunTests = true;
					semaphore.Dispose( );

					CancellationTokenSource.Dispose( );
					CancellationTokenSource = null;
				}, token );

				if ( synchronous )
				{
					Task.WaitAll( tasks.ToArray( ) );
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Gets the attribute value.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="fallbackValue">The fall-back value.</param>
		/// <returns></returns>
		private string GetAttributeValue( XmlNode node, string attributeName, string fallbackValue )
		{
			if ( node?.Attributes == null )
			{
				return fallbackValue;
			}

			var attribute = node.Attributes[ attributeName ];

			if ( attribute == null )
			{
				return fallbackValue;
			}

			return attribute.Value;
		}

		/// <summary>
		///     Loads the tests.
		/// </summary>
		private void LoadTests( )
		{
			var location = Assembly.GetExecutingAssembly( ).Location;

			string path = Path.GetDirectoryName( location );

			if ( path != null )
			{
				path = Path.Combine( path, "DatabaseTests.xml" );

				if ( !File.Exists( path ) )
				{
					MessageBox.Show( "No database tests defined." );
					return;
				}

				var doc = new XmlDocument( );
				doc.Load( path );

				var testNodes = doc.SelectNodes( "/tests/test" );

				var tests = new List<DatabaseTest>( );

				if ( testNodes != null )
				{
					foreach ( XmlNode testNode in testNodes )
					{
						var test = new DatabaseTest( PluginSettings, tests );

						if ( testNode.Attributes != null )
						{
							test.Name = GetAttributeValue( testNode, "name", "Unnamed" );
							test.Description = GetAttributeValue( testNode, "description", string.Empty );
							test.TestType = GetAttributeValue( testNode, "type", "info" );

							var entityColumns = testNode.Attributes[ "entityColumns" ];

							if ( entityColumns != null )
							{
								test.EntityColumns = entityColumns.Value;
							}

							var hiddenColumns = testNode.Attributes[ "hiddenColumns" ];

							if ( hiddenColumns != null )
							{
								test.HiddenColumns = hiddenColumns.Value;
							}

							var tenantColumn = testNode.Attributes [ "tenantColumn" ];

							int tenantColumnVal;
							if ( tenantColumn != null && int.TryParse( tenantColumn.Value, out tenantColumnVal ) )
							{
								test.TenantColumn = tenantColumnVal;
							}

							var queryNode = testNode.SelectSingleNode( "query" );

							if ( queryNode?.FirstChild != null )
							{
								test.Query = queryNode.FirstChild.Value;
							}

							var resolutionNode = testNode.SelectSingleNode( "resolution" );

							if ( resolutionNode?.FirstChild != null )
							{
								test.Resolution = resolutionNode.FirstChild.Value;
							}

							tests.Add( test );
						}
					}
				}

				Tests = tests.OrderBy( t => t.TestType ).ThenBy( t => t.Name ).ToList( );
			}
		}

		/// <summary>
		///     Called when [run tests asynchronous].
		/// </summary>
		private void OnRunTestsAsync( )
		{
			if ( ButtonText == RunTestText )
			{
				OnRunTests( );
			}
			else
			{
				OnStopTests( );
			}
		}

		/// <summary>
		///     Called when [stop tests].
		/// </summary>
		private void OnStopTests( )
		{
			try
			{
				CancellationTokenSource.Cancel( );

				CanRunTests = true;
			}
			catch ( Exception exc )
			{
				Trace.WriteLine( exc );
			}
		}
	}

	/// <summary>
	///     Class representing the PrettyPrint type.
	/// </summary>
	public static class PrettyPrint
	{
		/// <summary>
		///     Prints the specified input.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="hiddenColumns">The hidden columns.</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <returns></returns>
		public static string Print( string input, string hiddenColumns, char delimiter = '\t' )
		{
			try
			{
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

				input = input.Replace( "\r", "" );

				string[ ] lines = input.Split( new[ ]
				{
					'\n'
				}, StringSplitOptions.RemoveEmptyEntries );

				List<int> columnWidths = new List<int>( );

				foreach ( string line in lines )
				{
					string[ ] columns = line.Split( delimiter );

					for ( int i = 0; i < columns.Length; i++ )
					{
						if ( hiddenColumnSet.Contains( i ) )
						{
							continue;
						}

						string value = columns[ i ];

						if ( columnWidths.Count < i + 1 )
						{
							columnWidths.Add( string.IsNullOrEmpty( value ) ? 0 : value.Length );
						}
						else
						{
							if ( !string.IsNullOrEmpty( value ) && columnWidths[ i ] < value.Length )
							{
								columnWidths[ i ] = value.Length;
							}
						}
					}
				}

				StringBuilder sb = new StringBuilder( );
				sb.AppendLine( );

				int lineCount = 0;
				foreach ( string line in lines )
				{
					if ( lineCount == 1 )
					{
						foreach ( int width in columnWidths )
						{
							sb.Append( "-".PadRight( width, '-' ) ).Append( "   " );
						}

						sb.AppendLine( );
					}

					string[ ] columns = line.Split( delimiter );

					for ( int i = 0; i < columns.Length; i++ )
					{
						if ( hiddenColumnSet.Contains( i ) )
						{
							continue;
						}

						string value = columns[ i ];

						sb.Append( value.PadRight( columnWidths[ i ] + 3 ) );
					}

					sb.AppendLine( );

					lineCount++;
				}

				return sb.ToString( );
			}
			catch ( Exception exc )
			{
				Trace.WriteLine( exc.Message );
			}

			return input;
		}
	}
}