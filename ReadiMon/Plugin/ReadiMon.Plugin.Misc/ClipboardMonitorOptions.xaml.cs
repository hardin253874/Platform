// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows.Controls;

namespace ReadiMon.Plugin.Misc
{
	/// <summary>
	///     Interaction logic for ClipboardMonitorOptions.xaml
	/// </summary>
	public partial class ClipboardMonitorOptions : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ClipboardMonitorOptions" /> class.
		/// </summary>
		public ClipboardMonitorOptions( ClipboardMonitorOptionsViewModel viewModel )
		{
			InitializeComponent( );

			viewModel.OnLoad( );

			DataContext = viewModel;
		}
	}
}