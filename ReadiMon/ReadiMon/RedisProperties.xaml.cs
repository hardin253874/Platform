// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;

namespace ReadiMon
{
	/// <summary>
	///     Interaction logic for RedisProperties.xaml
	/// </summary>
	public partial class RedisProperties : Window
	{
		/// <summary>
		///     View Model.
		/// </summary>
		private readonly RedisPropertiesViewModel _viewModel;

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisProperties" /> class.
		/// </summary>
		public RedisProperties( )
		{
			InitializeComponent( );

			_viewModel = new RedisPropertiesViewModel( );

			DataContext = _viewModel;
		}
	}
}