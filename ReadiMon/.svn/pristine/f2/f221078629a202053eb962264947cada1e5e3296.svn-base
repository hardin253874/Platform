// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;

namespace ReadiMon
{
	/// <summary>
	///     Interaction logic for ChangeLog.xaml
	/// </summary>
	public partial class ChangeLog : Window
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="ChangeLog" /> class.
		/// </summary>
		/// <param name="text">The text.</param>
		public ChangeLog( string text )
		{
			InitializeComponent( );

			var vm = new ChangeLogViewModel
			{
				Text = text
			};

			DataContext = vm;
		}
	}
}