// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     Focus extension.
	/// </summary>
	public static class FocusExtension
	{
		/// <summary>
		///     The is focused property
		/// </summary>
		public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached( "IsFocused", typeof ( bool ), typeof ( FocusExtension ), new UIPropertyMetadata( false, OnIsFocusedPropertyChanged ) );

		/// <summary>
		///     Gets the is focused.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static bool GetIsFocused( DependencyObject obj )
		{
			return ( bool ) obj.GetValue( IsFocusedProperty );
		}

		/// <summary>
		///     Called when [is focused property changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void OnIsFocusedPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			var uie = ( UIElement ) d;

			if ( ( bool ) e.NewValue )
			{
				uie.Focus( );
			}
		}

		/// <summary>
		///     Sets the is focused.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetIsFocused( DependencyObject obj, bool value )
		{
			obj.SetValue( IsFocusedProperty, value );
		}
	}
}