// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;

namespace ReadiMon.Shared.AttachedProperties
{
	/// <summary>
	///     Window attached properties.
	/// </summary>
	public static class WindowAttachedProperties
	{
		/// <summary>
		/// </summary>
		public static readonly DependencyProperty CloseWindowProperty =
			DependencyProperty.RegisterAttached(
				"CloseWindow",
				typeof ( bool? ),
				typeof ( WindowAttachedProperties ),
				new PropertyMetadata( CloseWindowChanged ) );

		/// <summary>
		///     Closes the window changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
		private static void CloseWindowChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e )
		{
			var window = d as Window;

			if ( window != null )
			{
				var val = e.NewValue as bool?;

				if ( val.HasValue && val.Value )
				{
					window.Close( );
				}
			}
		}

		/// <summary>
		///     Sets the close window.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetCloseWindow( Window target, bool? value )
		{
			target.SetValue( CloseWindowProperty, value );
		}
	}
}