// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Collections.ObjectModel;
using ReadiMon.Core;

namespace ReadiMon
{
	/// <summary>
	///     Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="OptionsWindow" /> class.
		/// </summary>
		/// <param name="sections">The sections.</param>
		public OptionsWindow( ObservableCollection<Section> sections )
		{
			InitializeComponent( );

			var viewModel = new OptionsWindowViewModel( this )
			{
				Sections = sections
			};

			DataContext = viewModel;
		}
	}
}