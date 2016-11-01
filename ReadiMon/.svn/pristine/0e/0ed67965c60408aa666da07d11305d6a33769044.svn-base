// Copyright 2011-2016 Global Software Innovation Pty Ltd

using ReadiMon.Shared;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     Interaction logic for LibraryApplications.xaml
	/// </summary>
	public partial class LibraryApplications
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="LibraryApplications" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public LibraryApplications( IPluginSettings settings )
		{
			InitializeComponent( );

			var viewModel = new LibraryApplicationsViewModel( settings );
			DataContext = viewModel;
		}
	}
}