// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Windows.Input;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Failure row.
	/// </summary>
	public class FailureRow : ViewModelBase
	{
		/// <summary>
		///     Whether the is row enabled
		/// </summary>
		private bool _rowEnabled;

		/// <summary>
		///     Whether the is row selected
		/// </summary>
		private bool _rowSelected;

		/// <summary>
		///     Initializes a new instance of the <see cref="FailureRow" /> class.
		/// </summary>
		/// <param name="testViewerViewModel">The test viewer view model.</param>
		/// <param name="fields">The fields.</param>
		/// <param name="allFields">All fields.</param>
		/// <param name="entityColumns">The entity columns.</param>
		/// <param name="resolution">The resolution.</param>
		/// <param name="settings">The settings.</param>
		public FailureRow( TestViewerViewModel testViewerViewModel, List<string> fields, List<string> allFields, string entityColumns, string resolution, IPluginSettings settings )
		{
			ParentViewModel = testViewerViewModel;

			Fields = fields;
			AllFields = allFields;

			if ( !string.IsNullOrEmpty( resolution ) )
			{
				RowEnabled = true;
			}

			EntityColumns = new List<int>( );

			PluginSettings = settings;

			NavigateCommand = new DelegateCommand<string>( SendMessage );

			if ( !string.IsNullOrEmpty( entityColumns ) )
			{
				var indexes = entityColumns.Split( ',' );

				foreach ( string index in indexes )
				{
					int value;

					if ( int.TryParse( index, out value ) )
					{
						EntityColumns.Add( value );
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the parent view model.
		/// </summary>
		/// <value>
		///     The parent view model.
		/// </value>
		private TestViewerViewModel ParentViewModel
		{
			get;
		}

		/// <summary>
		///     Gets or sets a value indicating whether the row is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if the row is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool RowEnabled
		{
			get
			{
				return _rowEnabled;
			}
			private set
			{
				SetProperty( ref _rowEnabled, value );
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether the row is selected.
		/// </summary>
		/// <value>
		///     <c>true</c> if the row is selected; otherwise, <c>false</c>.
		/// </value>
		public bool RowSelected
		{
			get
			{
				return _rowSelected;
			}
			set
			{
				SetProperty( ref _rowSelected, value );

				ParentViewModel.DeleteEnabled = value;
			}
		}

		/// <summary>
		///     Gets the entity columns.
		/// </summary>
		/// <value>
		///     The entity columns.
		/// </value>
		public List<int> EntityColumns
		{
			get;
		}

		/// <summary>
		///     Gets the fields.
		/// </summary>
		/// <value>
		///     The fields.
		/// </value>
		public List<string> Fields
		{
			get;
		}

		/// <summary>
		///     Gets all fields.
		/// </summary>
		/// <value>
		///     All fields.
		/// </value>
		public List<string> AllFields
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the navigate command.
		/// </summary>
		/// <value>
		///     The navigate command.
		/// </value>
		public ICommand NavigateCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		private IPluginSettings PluginSettings
		{
			get;
		}

		/// <summary>
		///     Sends the message.
		/// </summary>
		/// <param name="indexString">The index string.</param>
		private void SendMessage( string indexString )
		{
			int index;

			if ( int.TryParse( indexString, out index ) )
			{
				if ( Fields.Count >= index )
				{
					PluginSettings.Channel.SendMessage( new EntityBrowserMessage( Fields[ index ] ).ToString( ) );
				}
			}
		}
	}
}