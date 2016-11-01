// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ApplicationManager.Controls
{
	/// <summary>
	///     Search mode.
	/// </summary>
	public enum SearchMode
	{
		/// <summary>
		///     Timer delayed.
		/// </summary>
		Instant,

		/// <summary>
		///     Manual search.
		/// </summary>
		Delayed,
	}

	/// <summary>
	///     Search text box.
	/// </summary>
	public class SearchTextBox : TextBox
	{
		/// <summary>
		///     SearchCommandProperty.
		/// </summary>
		public static readonly DependencyProperty SearchCommandProperty =
			DependencyProperty.Register(
				"SearchCommand",
				typeof ( ICommand ),
				typeof ( SearchTextBox ),
				new PropertyMetadata( null ) );

		/// <summary>
		///     HasTextPropertyKey.
		/// </summary>
		public static DependencyPropertyKey HasTextPropertyKey =
			DependencyProperty.RegisterReadOnly(
				"HasText",
				typeof ( bool ),
				typeof ( SearchTextBox ),
				new PropertyMetadata( ) );

		/// <summary>
		///     HasTextPropertyKeyProperty.
		/// </summary>
		public static DependencyProperty HasTextPropertyKeyProperty = HasTextPropertyKey.DependencyProperty;

		/// <summary>
		///     IsMouseLeftButtonDownPropertyKey.
		/// </summary>
		public static DependencyPropertyKey IsMouseLeftButtonDownPropertyKey =
			DependencyProperty.RegisterReadOnly(
				"IsMouseLeftButtonDown",
				typeof ( bool ),
				typeof ( SearchTextBox ),
				new PropertyMetadata( ) );

		/// <summary>
		///     IsMouseLeftButtonDownPropertyKeyProperty.
		/// </summary>
		public static DependencyProperty IsMouseLeftButtonDownPropertyKeyProperty = IsMouseLeftButtonDownPropertyKey.DependencyProperty;

		/// <summary>
		///     LabelTextColorProperty.
		/// </summary>
		public static DependencyProperty LabelTextColorProperty =
			DependencyProperty.Register(
				"LabelTextColor",
				typeof ( Brush ),
				typeof ( SearchTextBox ) );

		/// <summary>
		///     LabelTextProperty.
		/// </summary>
		public static DependencyProperty LabelTextProperty =
			DependencyProperty.Register(
				"LabelText",
				typeof ( string ),
				typeof ( SearchTextBox ) );

		/// <summary>
		///     SearchEventTimeDelayProperty.
		/// </summary>
		public static DependencyProperty SearchEventTimeDelayProperty =
			DependencyProperty.Register(
				"SearchEventTimeDelay",
				typeof ( Duration ),
				typeof ( SearchTextBox ),
				new FrameworkPropertyMetadata(
					new Duration( new TimeSpan( 0, 0, 0, 0, 500 ) ),
					OnSearchEventTimeDelayChanged ) );

		/// <summary>
		///     SearchModeProperty.
		/// </summary>
		public static DependencyProperty SearchModeProperty =
			DependencyProperty.Register(
				"SearchMode",
				typeof ( SearchMode ),
				typeof ( SearchTextBox ),
				new PropertyMetadata( SearchMode.Instant ) );

		/// <summary>
		///     searchEventDelayTimer.
		/// </summary>
		private readonly DispatcherTimer _searchEventDelayTimer;

		/// <summary>
		///     Initializes the <see cref="SearchTextBox" /> class.
		/// </summary>
		static SearchTextBox( )
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof ( SearchTextBox ),
				new FrameworkPropertyMetadata( typeof ( SearchTextBox ) ) );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SearchTextBox" /> class.
		/// </summary>
		public SearchTextBox( )
		{
			_searchEventDelayTimer = new DispatcherTimer
				{
					Interval = SearchEventTimeDelay.TimeSpan
				};

			_searchEventDelayTimer.Tick += OnSearchEventDelayTimerTick;
		}

		/// <summary>
		///     Gets a value indicating whether this instance has text.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has text; otherwise, <c>false</c>.
		/// </value>
		public bool HasText
		{
			get
			{
				return ( bool ) GetValue( HasTextPropertyKeyProperty );
			}
			private set
			{
				SetValue( HasTextPropertyKey, value );
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is mouse left button down.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is mouse left button down; otherwise, <c>false</c>.
		/// </value>
		public bool IsMouseLeftButtonDown
		{
			get
			{
				return ( bool ) GetValue( IsMouseLeftButtonDownPropertyKeyProperty );
			}
			private set
			{
				SetValue( IsMouseLeftButtonDownPropertyKey, value );
			}
		}

		/// <summary>
		///     Gets or sets the label text.
		/// </summary>
		/// <value>
		///     The label text.
		/// </value>
		public string LabelText
		{
			get
			{
				return ( string ) GetValue( LabelTextProperty );
			}
			set
			{
				SetValue( LabelTextProperty, value );
			}
		}

		/// <summary>
		///     Gets or sets the colour of the label text.
		/// </summary>
		/// <value>
		///     The colour of the label text.
		/// </value>
		public Brush LabelTextColor
		{
			get
			{
				return ( Brush ) GetValue( LabelTextColorProperty );
			}
			set
			{
				SetValue( LabelTextColorProperty, value );
			}
		}

		/// <summary>
		///     Gets or sets the search command.
		/// </summary>
		/// <value>
		///     The search command.
		/// </value>
		public ICommand SearchCommand
		{
			get
			{
				return ( ICommand ) GetValue( SearchCommandProperty );
			}
			set
			{
				SetValue( SearchCommandProperty, value );
			}
		}

		/// <summary>
		///     Gets or sets the search event time delay.
		/// </summary>
		/// <value>
		///     The search event time delay.
		/// </value>
		public Duration SearchEventTimeDelay
		{
			get
			{
				return ( Duration ) GetValue( SearchEventTimeDelayProperty );
			}
			set
			{
				SetValue( SearchEventTimeDelayProperty, value );
			}
		}

		/// <summary>
		///     Gets or sets the search mode.
		/// </summary>
		/// <value>
		///     The search mode.
		/// </value>
		public SearchMode SearchMode
		{
			get
			{
				return ( SearchMode ) GetValue( SearchModeProperty );
			}
			set
			{
				SetValue( SearchModeProperty, value );
			}
		}

		/// <summary>
		///     Is called when a control template is applied.
		/// </summary>
		public override void OnApplyTemplate( )
		{
			base.OnApplyTemplate( );

			var iconBorder = GetTemplateChild( "PART_SearchIconBorder" ) as Border;
			if ( iconBorder != null )
			{
				iconBorder.MouseLeftButtonDown += IconBorder_MouseLeftButtonDown;
				iconBorder.MouseLeftButtonUp += IconBorder_MouseLeftButtonUp;
				iconBorder.MouseLeave += IconBorder_MouseLeave;
			}
		}

		/// <summary>
		///     Invoked whenever an unhandled <see cref="System.Windows.Input.Keyboard" /> attached routed event reaches an element derived from this class in its route. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">Provides data about the event.</param>
		protected override void OnKeyDown( KeyEventArgs e )
		{
			if ( e.Key == Key.Escape && SearchMode == SearchMode.Instant )
			{
				Text = "";
			}
			else if ( e.Key == Key.Return || e.Key == Key.Enter )
			{
				if ( SearchMode == SearchMode.Instant )
				{
					_searchEventDelayTimer.Stop( );
				}

				RaiseSearchEvent( );
			}
			else
			{
				base.OnKeyDown( e );
			}
		}

		/// <summary>
		///     Is called when content in this editing control changes.
		/// </summary>
		/// <param name="e">
		///     The arguments that are associated with the <see cref="E:System.Windows.Controls.Primitives.TextBoxBase.TextChanged" /> event.
		/// </param>
		protected override void OnTextChanged( TextChangedEventArgs e )
		{
			base.OnTextChanged( e );

			HasText = Text.Length != 0;

			if ( SearchMode == SearchMode.Instant )
			{
				_searchEventDelayTimer.Stop( );
				_searchEventDelayTimer.Start( );
			}
		}

		/// <summary>
		///     Handles the MouseLeave event of the IconBorder control.
		/// </summary>
		/// <param name="obj">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="MouseEventArgs" /> instance containing the event data.
		/// </param>
		private void IconBorder_MouseLeave( object obj, MouseEventArgs e )
		{
			IsMouseLeftButtonDown = false;
		}

		/// <summary>
		///     Handles the MouseLeftButtonDown event of the IconBorder control.
		/// </summary>
		/// <param name="obj">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
		/// </param>
		private void IconBorder_MouseLeftButtonDown( object obj, MouseButtonEventArgs e )
		{
			IsMouseLeftButtonDown = true;
		}

		/// <summary>
		///     Handles the MouseLeftButtonUp event of the IconBorder control.
		/// </summary>
		/// <param name="obj">The source of the event.</param>
		/// <param name="e">
		///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
		/// </param>
		private void IconBorder_MouseLeftButtonUp( object obj, MouseButtonEventArgs e )
		{
			if ( !IsMouseLeftButtonDown )
			{
				return;
			}

			if ( HasText && SearchMode == SearchMode.Instant )
			{
				Text = "";
			}

			if ( HasText && SearchMode == SearchMode.Delayed )
			{
				RaiseSearchEvent( );
			}

			IsMouseLeftButtonDown = false;
		}

		/// <summary>
		///     Called when the search event delay timer fires.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="e">
		///     The <see cref="EventArgs" /> instance containing the event data.
		/// </param>
		private void OnSearchEventDelayTimerTick( object o, EventArgs e )
		{
			_searchEventDelayTimer.Stop( );
			RaiseSearchEvent( );
		}

		/// <summary>
		///     Called when [search event time delay changed].
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="e">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
		private static void OnSearchEventTimeDelayChanged(
			DependencyObject o, DependencyPropertyChangedEventArgs e )
		{
			var stb = o as SearchTextBox;
			if ( stb != null )
			{
				stb._searchEventDelayTimer.Interval = ( ( Duration ) e.NewValue ).TimeSpan;
				stb._searchEventDelayTimer.Stop( );
			}
		}

		/// <summary>
		///     Raises the search event.
		/// </summary>
		private void RaiseSearchEvent( )
		{
			SearchCommand.Execute( Text );
		}
	}
}