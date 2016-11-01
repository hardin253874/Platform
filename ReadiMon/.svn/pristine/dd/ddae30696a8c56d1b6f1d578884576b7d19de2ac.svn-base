// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for AddRelationshipDialog.xaml
	/// </summary>
	public partial class AddRelationshipDialog : Window
	{
		private readonly AddRelationshipDialogViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="AddRelationshipDialog" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="selectedEntityId">The selected entity identifier.</param>
		/// <param name="forward">if set to <c>true</c> [forward].</param>
		public AddRelationshipDialog( IPluginSettings settings, long selectedEntityId, bool forward )
		{
			InitializeComponent( );

			_viewModel = new AddRelationshipDialogViewModel( settings, selectedEntityId, forward );

			DataContext = _viewModel;
		}
	}
}