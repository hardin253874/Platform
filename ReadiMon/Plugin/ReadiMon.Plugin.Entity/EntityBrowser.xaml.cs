// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows.Controls;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for EntityBrowser.xaml
	/// </summary>
	public partial class EntityBrowser : UserControl
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityBrowser" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public EntityBrowser( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new EntityBrowserViewModel( settings );
			DataContext = viewModel;

			KeyDown += EntityBrowser_KeyDown;
		}

		/// <summary>
		/// Handles the KeyDown event of the EntityBrowser control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
		private void EntityBrowser_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
		{
			if ( ! ( SearchBox.IsFocused || SearchBox.IsSelectionBoxHighlighted ) && e.Key == System.Windows.Input.Key.Back )
			{
				var vm = DataContext as EntityBrowserViewModel;

				if ( vm != null && vm.CanNavigateBack )
				{
					vm.BackCommand.Execute( null );
				}
			}
		}
	}
}