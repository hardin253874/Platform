// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;

namespace ReadiMon.Shared.Controls.TreeListView
{
	/// <summary>
	///     RowExpander
	/// </summary>
	public class RowExpander : Control
	{
		/// <summary>
		///     Initializes the <see cref="RowExpander" /> class.
		/// </summary>
		static RowExpander( )
		{
			DefaultStyleKeyProperty.OverrideMetadata( typeof ( RowExpander ), new FrameworkPropertyMetadata( typeof ( RowExpander ) ) );
		}
	}
}