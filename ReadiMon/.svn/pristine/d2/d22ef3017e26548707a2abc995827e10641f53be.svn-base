// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     Test Editor View Model.
	/// </summary>
	public class TestEditorViewModel : ViewModelBase
	{
		/// <summary>
		///     The query
		/// </summary>
		private string _query;

		/// <summary>
		///     Initializes a new instance of the <see cref="TestEditorViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="query">The query.</param>
		public TestEditorViewModel( IPluginSettings settings, string query )
		{
			PluginSettings = settings;
			Query = query;

			OkCommand = new DelegateCommand<Window>( OnOkClick );
		}

		/// <summary>
		///     Gets or sets the OK command.
		/// </summary>
		/// <value>
		///     The OK command.
		/// </value>
		public ICommand OkCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		private IPluginSettings PluginSettings
		{
			[UsedImplicitly]
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the query.
		/// </summary>
		/// <value>
		///     The query.
		/// </value>
		public string Query
		{
			get
			{
				return _query;
			}
			set
			{
				SetProperty( ref _query, value );
			}
		}

		/// <summary>
		///     Called when OK is clicked.
		/// </summary>
		private void OnOkClick( Window window )
		{
			window.Close( );
		}
	}
}