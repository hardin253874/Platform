// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using ReadiMon.AddinView;
using ReadiMon.AddinView.Configuration;

namespace ReadiMon.Plugin.Redis
{
	//[AddIn( "Profiler Plugin", Version = "1.0.0.0" )]
	/// <summary>
	///     The ProfilerPlugin class.
	/// </summary>
	/// <seealso cref="ReadiMon.AddinView.PluginBase" />
	/// <seealso cref="ReadiMon.AddinView.IPlugin" />
	public class ProfilerPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private ProfilerMonitor _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPlugin" /> class.
		/// </summary>
		public ProfilerPlugin( )
		{
			SectionName = "Profiler";
			EntryName = "ReadiNow Profiler";
			EntryOrdinal = 0;
			SectionOrdinal = 1;
			HasUserInterface = true;
			HasOptionsUserInterface = false;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new ProfilerMonitor( Settings ) );
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetToolBar( )
		{
			return null;
		}

		/// <summary>
		///     Creates the configuration section.
		/// </summary>
		/// <returns></returns>
		protected override PluginConfigurationBase CreateConfigurationSection( )
		{
			return new PluginConfiguration( );
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			if ( _userInterface != null )
			{
				var viewModel = _userInterface.DataContext as ProfilerMonitorViewModel;

				if ( viewModel != null )
				{
					viewModel.PluginSettings = Settings;
				}
			}
		}
	}
}