// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using ReadiMon.Shared;

namespace ReadiMon.Plugin.Entity
{
	/// <summary>
	///     Interaction logic for FancyBalloon.xaml
	/// </summary>
	public partial class FancyBalloon : UserControl
	{
		/// <summary>
		///     The is closing
		/// </summary>
		private bool _isClosing;

		/// <summary>
		///     Initializes the <see cref="FancyBalloon" /> class.
		/// </summary>
		static FancyBalloon( )
		{
			CurrentEntityId = -1;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="FancyBalloon" /> class.
		/// </summary>
		public FancyBalloon( )
		{
			Active = true;
			InitializeComponent( );
			TaskbarIcon.AddBalloonClosingHandler( this, OnBalloonClosing );
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="FancyBalloon" /> is active.
		/// </summary>
		/// <value>
		///     <c>true</c> if active; otherwise, <c>false</c>.
		/// </value>
		public static bool Active
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the current entity identifier.
		/// </summary>
		/// <value>
		///     The current entity identifier.
		/// </value>
		public static long CurrentEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the logger.
		/// </summary>
		/// <value>
		///     The logger.
		/// </value>
		public IEventLog EventLog
		{
			get;
			set;
		}

		/// <summary>
		///     If the users hovers over the balloon, we don't close it.
		/// </summary>
		private void grid_MouseEnter( object sender, MouseEventArgs e )
		{
			//if we're already running the fade-out animation, do not interrupt anymore
			//(makes things too complicated for the sample)
			if ( _isClosing )
				return;

			//the tray icon assigned this attached property to simplify access
			TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );
			taskbarIcon.ResetBalloonCloseTimer( );
		}

		/// <summary>
		///     Handles the MouseLeftButtonUp event of the grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
		private void grid_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
		{
			TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );
			taskbarIcon.CloseBalloon( );
			e.Handled = true;
		}

		/// <summary>
		///     Resolves the <see cref="TaskbarIcon" /> that displayed
		///     the balloon and requests a close action.
		/// </summary>
		private void imgClose_MouseDown( object sender, MouseButtonEventArgs e )
		{
			//the tray icon assigned this attached property to simplify access
			TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon( this );

			taskbarIcon.CloseBalloon( );
			e.Handled = true;
		}

		/// <summary>
		///     By subscribing to the <see cref="TaskbarIcon.BalloonClosingEvent" />
		///     and setting the "Handled" property to true, we suppress the pop up
		///     from being closed in order to display the custom fade-out animation.
		/// </summary>
		private void OnBalloonClosing( object sender, RoutedEventArgs e )
		{
			e.Handled = true; //suppresses the pop up from being closed immediately
			_isClosing = true;
		}

		/// <summary>
		///     Closes the pop up once the fade-out animation completed.
		///     The animation was triggered in XAML through the attached
		///     BalloonClosing event.
		/// </summary>
		private void OnFadeOutCompleted( object sender, EventArgs e )
		{
			var pp = ( Popup ) Parent;
			pp.IsOpen = false;
			Active = false;
			CurrentEntityId = -1;

			var closed = Closed;

			if ( closed != null )
			{
				closed( this, null );
			}
		}

		#region BalloonText dependency property

		/// <summary>
		///     Description
		/// </summary>
		public static readonly DependencyProperty BalloonTextProperty =
			DependencyProperty.Register( "BalloonText",
				typeof ( string ),
				typeof ( FancyBalloon ),
				new FrameworkPropertyMetadata( "" ) );

		/// <summary>
		///     A property wrapper for the <see cref="BalloonTextProperty" />
		///     dependency property:<br />
		///     Description
		/// </summary>
		public string BalloonText
		{
			get
			{
				return ( string ) GetValue( BalloonTextProperty );
			}
			set
			{
				SetValue( BalloonTextProperty, value );
			}
		}

		#endregion

		/// <summary>
		///     Occurs when the balloon is closed.
		/// </summary>
		public event EventHandler Closed;
	}
}