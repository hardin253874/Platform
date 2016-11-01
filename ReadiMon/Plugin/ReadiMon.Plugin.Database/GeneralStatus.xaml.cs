// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Interaction logic for GeneralStatus.xaml
	/// </summary>
	public partial class GeneralStatus : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="GeneralStatus" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public GeneralStatus( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new GeneralSettingsViewModel( settings );
			DataContext = viewModel;
		}
	}
}