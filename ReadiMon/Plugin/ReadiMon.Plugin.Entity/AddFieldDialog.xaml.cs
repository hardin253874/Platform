// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for AddFieldDialog.xaml
	/// </summary>
	public partial class AddFieldDialog : Window
	{
		private readonly AddFieldDialogViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="AddFieldDialog" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="selectedEntityId">The selected entity identifier.</param>
		public AddFieldDialog( IPluginSettings settings, long selectedEntityId )
		{
			InitializeComponent( );

			_viewModel = new AddFieldDialogViewModel( settings, selectedEntityId );

			DataContext = _viewModel;
		}
	}
}