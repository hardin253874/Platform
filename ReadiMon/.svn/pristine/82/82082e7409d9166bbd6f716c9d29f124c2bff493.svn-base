// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	///     PerfGraphViewModel class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class PerfGraphViewModel : ViewModelBase
	{
		private const string CssTag = "/* PERFORMANCE_CSS */";
		private const string D3PerfTag = "/* D3_PERF_JS */";
		private const string D3Tag = "/* D3_V3_MIN_JS */";
		private const string D3TipTag = "/* D3_TIP_JS */";
		private const string LTag = "/* _LOGS_ */";
		private readonly Dispatcher _dispatcher;
		private Assembly _asm;
		private string _htmlContent = "";
		private string _htmlTemplate = "";
		private string _logs = "";

		/// <summary>
		///     Initializes a new instance of the <see cref="PerfGraphViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public PerfGraphViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			_dispatcher = Dispatcher.CurrentDispatcher;

			LoadCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Load ) );
			ResetCommand = new DelegateCommand( ( ) => _dispatcher.Invoke( Reset ) );
		}

		/// <summary>
		///     Gets or sets the content of the HTML.
		/// </summary>
		/// <value>
		///     The content of the HTML.
		/// </value>
		public string HtmlContent
		{
			get
			{
				return _htmlContent;
			}
			set
			{
				SetProperty( ref _htmlContent, value );
			}
		}

		/// <summary>
		///     Gets or sets the load command.
		/// </summary>
		/// <value>
		///     The load command.
		/// </value>
		public ICommand LoadCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the logs.
		/// </summary>
		/// <value>
		///     The logs.
		/// </value>
		public string Logs
		{
			get
			{
				return _logs;
			}
			set
			{
				SetProperty( ref _logs, value );
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
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the reset command.
		/// </summary>
		/// <value>
		///     The reset command.
		/// </value>
		public ICommand ResetCommand
		{
			get;
			set;
		}

		private string CleanUpLogs( string rawLogs )
		{
			var n1 = rawLogs.IndexOf( '{' );
			var n2 = rawLogs.LastIndexOf( '}' );

			return rawLogs.Substring( n1, ( n2 - ( n1 - 1 ) ) );
		}

		private void Load( )
		{
			_asm = Assembly.GetExecutingAssembly( );

			var content = string.Format( "<!-- // {0} // -->", DateTime.Now );

			var d3 = LoadResourceText( "d3.v3.min.js" );
			var d3Tip = LoadResourceText( "d3-tip.js" );
			var d3Perf = LoadResourceText( "d3.perf.js" );
			var css = LoadResourceText( "performance.css" );

			if ( string.IsNullOrEmpty( Logs ) )
			{
				Logs = LoadResourceText( "data.json" );
			}

			if ( d3 != string.Empty )
			{
				var sb = new StringBuilder( LoadResourceText( "performance.html" ) );

				sb.Replace( D3Tag, d3 );
				sb.Replace( D3TipTag, d3Tip );
				sb.Replace( D3PerfTag, d3Perf );
				sb.Replace( CssTag, css );

				_htmlTemplate = sb.ToString( );

				Reset( );
			}
			else
			{
				HtmlContent = content;
			}
		}

		private string LoadResourceText( string resourceName )
		{
			using ( var s = _asm.GetManifestResourceStream( "ReadiMon.Plugin.Graphs.Resources." + resourceName ) )
			{
				if ( s != null )
				{
					using ( var sr = new StreamReader( s ) )
					{
						return sr.ReadToEnd( );
					}
				}
			}

			return string.Empty;
		}

		private void Reset( )
		{
			try
			{
				var cleanedUpLogs = CleanUpLogs( Logs ).Replace( "\\", "\\\\" );

				var logs = JsonConvert.DeserializeObject( cleanedUpLogs );

				var sb = new StringBuilder( _htmlTemplate );

				sb.Replace( LTag, JsonConvert.SerializeObject( logs ).Replace( "\\", "\\\\" ) );

				HtmlContent = sb.ToString( );
			}
			catch ( Exception e )
			{
				HtmlContent = string.Format( "<!-- // {0}: {1} // -->", DateTime.Now, e.Message );
			}
		}
	}
}