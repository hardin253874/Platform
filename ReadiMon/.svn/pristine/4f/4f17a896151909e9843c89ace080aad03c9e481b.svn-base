using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	/// Orphan Detection plugin
	/// </summary>
	/// <seealso cref="ReadiMon.AddinView.PluginBase" />
	/// <seealso cref="ReadiMon.AddinView.IPlugin" />
	[AddIn( "Orphan Detection Plugin", Version = "1.0.0.0" )]
	public class OphanDetectionPlugin : PluginBase, IPlugin
	{
		/// <summary>
		/// The user interface
		/// </summary>
		private OrphanDetection _userInterface;

		/// <summary>
		/// Initializes a new instance of the <see cref="OphanDetectionPlugin"/> class.
		/// </summary>
		public OphanDetectionPlugin( )
		{
			SectionOrdinal = 1;
			SectionName = "Entity";
			EntryName = "Orphan Detection";
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
			return _userInterface ?? ( _userInterface = new OrphanDetection( Settings ) );
		}

		/// <summary>
		///     Called when updating the settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			var viewModel = _userInterface?.DataContext as OrphanDetectionViewModel;

			if ( viewModel != null )
			{
				viewModel.PluginSettings = Settings;
			}
		}
	}
}
