// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ReadiMon.AddinView;
using ReadiMon.AddinView.Configuration;

namespace ReadiMon.Plugin.Redis
{
	/// <summary>
	///     Redis plugin
	/// </summary>
	[AddIn( "Redis Plugin", Version = "1.0.0.0" )]
	public class RedisPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The user interface
		/// </summary>
		private RedisPubSubMonitor _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPlugin" /> class.
		/// </summary>
		public RedisPlugin( )
		{
			SectionName = "Redis";
			EntryName = "Redis Message Monitor";
			EntryOrdinal = 0;
			SectionOrdinal = 3;
			HasUserInterface = true;
			HasOptionsUserInterface = false;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new RedisPubSubMonitor( Settings ) );
		}

		/// <summary>
		///     Gets the tool bar.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetToolBar( )
		{
			//var tb = new ToolBar( );

			//tb.Items.Add( new ToggleButton
			//{
			//	Content = "Redis"
			//} );

			//return tb;
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
				var viewModel = _userInterface.DataContext as RedisPubSubMonitorViewModel;

				if ( viewModel != null )
				{
					viewModel.PluginSettings = Settings;
				}
			}
		}
	}
}