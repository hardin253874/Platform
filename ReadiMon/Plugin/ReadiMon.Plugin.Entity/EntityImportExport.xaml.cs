// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for EntityImportExport.xaml
	/// </summary>
	public partial class EntityImportExport : UserControl
	{
		/// <summary>
		///     The view model
		/// </summary>
		private readonly EntityImportExportViewModel _viewModel;

		/// <summary>
		///     The first focus
		/// </summary>
		private bool _firstFocus = true;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityImportExport" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public EntityImportExport( IPluginSettings settings )
		{
			InitializeComponent( );

			XmlCode.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition( "XML" );

			_viewModel = new EntityImportExportViewModel( settings, OnTextChanged );
			DataContext = _viewModel;
		}

		/// <summary>
		///     Called when [text changed].
		/// </summary>
		private void OnTextChanged( )
		{
			_firstFocus = false;
		}

		/// <summary>
		///     Handles the GotFocus event of the XmlCode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private void XmlCode_GotFocus( object sender, RoutedEventArgs e )
		{
			if ( _firstFocus )
			{
				_viewModel.ImportExportText = string.Empty;
				_firstFocus = false;
			}
		}
	}
}