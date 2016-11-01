// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	///     Interaction logic for PerfGraph.xaml
	/// </summary>
	public partial class PerfGraph : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PerfGraph" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public PerfGraph( IPluginSettings settings )
		{
			var viewModel = new PerfGraphViewModel( settings );
			DataContext = viewModel;

			InitializeComponent( );
		}
	}
}