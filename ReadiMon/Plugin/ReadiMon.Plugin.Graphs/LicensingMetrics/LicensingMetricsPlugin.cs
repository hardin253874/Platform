// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ReadiMon.AddinView;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Graphs.LicensingMetrics
{
	/// <summary>
	///     LicensingMetricsPlugin class.
	/// </summary>
	/// <seealso cref="PluginBase" />
	/// <seealso cref="IPlugin" />
	[AddIn( "Licensing Metrics Plugin", Version = "1.0.0.0" )]
	public class LicensingMetricsPlugin : PluginBase, IPlugin
	{
		private LicensingMetrics _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="LicensingMetricsPlugin" /> class.
		/// </summary>
		public LicensingMetricsPlugin( )
		{
			SectionOrdinal = 7;
			SectionName = "Graphs";
			EntryName = "Licensing Metrics";
			EntryOrdinal = 1;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			if ( _userInterface == null )
			{
				_userInterface = new LicensingMetrics( Settings );
				_userInterface.MetricsUpdate += ( sender, args ) => Settings.Channel.SendMessage( new MetricsUpdateMessage( ).ToString( ) );
			}
			return _userInterface;
		}
	}
}