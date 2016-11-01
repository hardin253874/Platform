// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ReadiMon.AddinView;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Graphs.TenantsTrend
{
	/// <summary>
	///     TenantsTrendPlugin class.
	/// </summary>
	/// <seealso cref="PluginBase" />
	/// <seealso cref="IPlugin" />
	[AddIn( "Tenants Trend Plugin", Version = "1.0.0.0" )]
	public class TenantsTrendPlugin : PluginBase, IPlugin
	{
		private readonly object _syncRoot = new object( );
		private TenantsTrend _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="TenantsTrendPlugin" /> class.
		/// </summary>
		public TenantsTrendPlugin( )
		{
			SectionOrdinal = 7;
			SectionName = "Graphs";
			EntryName = "Tenants";
			EntryOrdinal = 3;
			HasOptionsUserInterface = false;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new TenantsTrend( Settings ) );
		}

		/// <summary>
		///     Receives the message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public override bool OnMessageReceived( string message )
		{
			if ( GetUserInterface( ) != null )
			{
				var deserializeObject = Serializer.DeserializeObject<object>( message );

				var metricsUpdateMessage = deserializeObject as MetricsUpdateMessage;
				if ( metricsUpdateMessage != null )
				{
					Reload( metricsUpdateMessage );
				}
			}

			return false;
		}

		private void Reload( MetricsUpdateMessage metricsUpdateMessage )
		{
			lock ( _syncRoot )
			{
				var viewModel = _userInterface.DataContext as TenantsTrendViewModel;
				if ( viewModel != null )
				{
					viewModel.ReloadCommand.Execute( metricsUpdateMessage );
				}
			}
		}
	}
}