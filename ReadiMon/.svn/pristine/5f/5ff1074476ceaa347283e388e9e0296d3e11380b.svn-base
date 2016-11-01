// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	///     Copied from Fancy Balloon.
	/// </summary>
	public partial class PerfGraphPopup : UserControl
	{
		private bool _closing;

		/// <summary>
		///     Initializes a new instance of the <see cref="PerfGraphPopup" /> class.
		/// </summary>
		public PerfGraphPopup( )
		{
			InitializeComponent( );

			TaskbarIcon.AddBalloonClosingHandler( this, OnBalloonClosing );
		}

		private void grid_MouseEnter( object sender, MouseEventArgs e )
		{
			if ( _closing )
				return;

			var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );
			taskbarIcon.ResetBalloonCloseTimer( );
		}

		private void grid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );
			taskbarIcon.CloseBalloon( );
			e.Handled = true;
		}

		private void imgClose_MouseDown( object sender, MouseButtonEventArgs e )
		{
			var taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );
			taskbarIcon.CloseBalloon( );
			e.Handled = true;
		}

		private void OnBalloonClosing( object sender, RoutedEventArgs e )
		{
			e.Handled = true;
			_closing = true;
		}

		private void OnFadeOutCompleted( object sender, EventArgs e )
		{
			var pp = ( Popup ) Parent;
			pp.IsOpen = false;
			if ( Closed != null )
			{
				Closed( this, null );
			}
		}

		/// <summary>
		///     Occurs when the pop-up is closed.
		/// </summary>
		public event EventHandler Closed;
	}
}