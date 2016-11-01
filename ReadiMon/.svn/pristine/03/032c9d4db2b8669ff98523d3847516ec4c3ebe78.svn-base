// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn;
using System.Windows;
using ReadiMon.AddinView;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Entity plugin
	/// </summary>
	[AddIn( "Entity Import/Export Plugin", Version = "1.0.0.0" )]
	public class EntityImportExportPlugin : PluginBase, IPlugin
	{
		/// <summary>
		///     The options view model
		/// </summary>
		private EntityImportExportOptionsViewModel _optionsViewModel;

		/// <summary>
		///     The user interface
		/// </summary>
		private EntityImportExport _userInterface;

		/// <summary>
		///     Initializes a new instance of the <see cref="EntityImportExportPlugin" /> class.
		/// </summary>
		public EntityImportExportPlugin( )
		{
			SectionOrdinal = 1;
			SectionName = "Entity";
			EntryName = "Entity Import/Export";
			EntryOrdinal = 1;
			HasOptionsUserInterface = true;
			HasUserInterface = true;
		}

		/// <summary>
		///     Gets the options view model.
		/// </summary>
		/// <value>
		///     The options view model.
		/// </value>
		private EntityImportExportOptionsViewModel OptionsViewModel
		{
			get
			{
				return _optionsViewModel ?? ( _optionsViewModel = new EntityImportExportOptionsViewModel( ) );
			}
		}

		/// <summary>
		///     Gets the user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetUserInterface( )
		{
			return _userInterface ?? ( _userInterface = new EntityImportExport( Settings ) );
		}

		/// <summary>
		///     Gets the options user interface.
		/// </summary>
		/// <returns></returns>
		public override FrameworkElement GetOptionsUserInterface( )
		{
			return new EntityImportExportOptions( OptionsViewModel );
		}

		/// <summary>
		///     Saves the options.
		/// </summary>
		public override void SaveOptions( )
		{
			if ( _optionsViewModel != null )
			{
				_optionsViewModel.OnSave( );
			}
		}

		/// <summary>
		///     Called when updating settings.
		/// </summary>
		protected override void OnUpdateSettings( )
		{
			base.OnUpdateSettings( );

			if ( _userInterface != null )
			{
				var viewModel = _userInterface.DataContext as EntityImportExportViewModel;

				if ( viewModel != null )
				{
					viewModel.PluginSettings = Settings;
				}
			}
		}
	}
}