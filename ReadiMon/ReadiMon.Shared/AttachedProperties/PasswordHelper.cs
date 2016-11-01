// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;

namespace ReadiMon.Shared.AttachedProperties
{
	/// <summary>
	///     Password helper.
	/// </summary>
	public static class PasswordHelper
	{
		/// <summary>
		///     The attach property
		/// </summary>
		public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached( "Attach", typeof ( bool ), typeof ( PasswordHelper ), new PropertyMetadata( false, Attach ) );

		/// <summary>
		///     The is updating property
		/// </summary>
		private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached( "IsUpdating", typeof ( bool ), typeof ( PasswordHelper ) );

		/// <summary>
		///     The password property
		/// </summary>
		public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached( "Password", typeof ( string ), typeof ( PasswordHelper ), new FrameworkPropertyMetadata( string.Empty, OnPasswordPropertyChanged ) );

		/// <summary>
		///     Attaches the specified sender.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void Attach( DependencyObject sender, DependencyPropertyChangedEventArgs e )
		{
			var passwordBox = sender as PasswordBox;

			if ( passwordBox == null )
				return;

			if ( ( bool ) e.OldValue )
			{
				passwordBox.PasswordChanged -= PasswordChanged;
			}

			if ( ( bool ) e.NewValue )
			{
				passwordBox.PasswordChanged += PasswordChanged;
			}
		}


		/// <summary>
		///     Gets the attach.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		/// <returns></returns>
		public static bool GetAttach( DependencyObject dependencyProperty )
		{
			return ( bool ) dependencyProperty.GetValue( AttachProperty );
		}

		/// <summary>
		///     Gets the is updating.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		/// <returns></returns>
		private static bool GetIsUpdating( DependencyObject dependencyProperty )
		{
			return ( bool ) dependencyProperty.GetValue( IsUpdatingProperty );
		}

		/// <summary>
		///     Gets the password.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		/// <returns></returns>
		public static string GetPassword( DependencyObject dependencyProperty )
		{
			return ( string ) dependencyProperty.GetValue( PasswordProperty );
		}

		/// <summary>
		///     Called when the password property changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void OnPasswordPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
		{
			var passwordBox = sender as PasswordBox;

			if ( passwordBox != null )
			{
				passwordBox.PasswordChanged -= PasswordChanged;

				if ( !GetIsUpdating( passwordBox ) )
				{
					passwordBox.Password = ( string ) e.NewValue;
				}

				passwordBox.PasswordChanged += PasswordChanged;
			}
		}

		/// <summary>
		///     Passwords the changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
		private static void PasswordChanged( object sender, RoutedEventArgs e )
		{
			var passwordBox = sender as PasswordBox;

			if ( passwordBox != null )
			{
				SetIsUpdating( passwordBox, true );
				SetPassword( passwordBox, passwordBox.Password );
				SetIsUpdating( passwordBox, false );
			}
		}

		/// <summary>
		///     Sets the attach.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetAttach( DependencyObject dependencyProperty, bool value )
		{
			dependencyProperty.SetValue( AttachProperty, value );
		}

		/// <summary>
		///     Sets the is updating.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		private static void SetIsUpdating( DependencyObject dependencyProperty, bool value )
		{
			dependencyProperty.SetValue( IsUpdatingProperty, value );
		}

		/// <summary>
		///     Sets the password.
		/// </summary>
		/// <param name="dependencyProperty">The dependency property.</param>
		/// <param name="value">The value.</param>
		public static void SetPassword( DependencyObject dependencyProperty, string value )
		{
			dependencyProperty.SetValue( PasswordProperty, value );
		}
	}
}