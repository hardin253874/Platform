// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for OrphanDetection.xaml
	/// </summary>
	public partial class OrphanDetection : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="OrphanDetection" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public OrphanDetection( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new OrphanDetectionViewModel( settings );
			DataContext = viewModel;
		}
	}
}