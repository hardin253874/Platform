// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;

namespace TenantDiffTool.Core
{
	public class PasswordHelper
	{
		/// <summary>
		///     Password property.
		/// </summary>
		public static readonly DependencyProperty PasswordProperty =
			DependencyProperty.RegisterAttached( "Password",
				typeof( string ), typeof( PasswordHelper ),
				new FrameworkPropertyMetadata( string.Empty, OnPasswordPropertyChanged ) );

		/// <summary>
		///     Attach method.
		/// </summary>
		public static readonly DependencyProperty AttachProperty =
			DependencyProperty.RegisterAttached( "Attach",
				typeof( bool ), typeof( PasswordHelper ), new PropertyMetadata( false, Attach ) );

		/// <summary>
		///     IsUpdating property
		/// </summary>
		private static readonly DependencyProperty IsUpdatingProperty =
			DependencyProperty.RegisterAttached( "IsUpdating", typeof( bool ),
				typeof( PasswordHelper ) );


		/// <summary>
		///     Gets the attach.
		/// </summary>
		/// <param name="dp">The dependency property.</param>
		/// <returns></returns>
		public static bool GetAttach( DependencyObject dp )
		{
			return ( bool ) dp.GetValue( AttachProperty );
		}

		/// <summary>
		///     Gets the password.
		/// </summary>
		/// <param name="dp">The dependency property.</param>
		/// <returns></returns>
		public static string GetPassword( DependencyObject dp )
		{
			return ( string ) dp.GetValue( PasswordProperty );
		}

		/// <summary>
		///     Sets the attach.
		/// </summary>
		/// <param name="dp">The dependency property.</param>
		/// <param name="value">
		///     if set to <c>true</c> [value].
		/// </param>
		public static void SetAttach( DependencyObject dp, bool value )
		{
			dp.SetValue( AttachProperty, value );
		}

		/// <summary>
		///     Sets the password.
		/// </summary>
		/// <param name="dp">The dependency property.</param>
		/// <param name="value">The value.</param>
		public static void SetPassword( DependencyObject dp, string value )
		{
			dp.SetValue( PasswordProperty, value );
		}

		/// <summary>
		///     Attaches the specified sender.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
		private static void Attach( DependencyObject sender, DependencyPropertyChangedEventArgs e )
		{
			var passwordBox = sender as PasswordBox;

			if ( passwordBox == null )
			{
				return;
			}

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
		///     Gets the is updating.
		/// </summary>
		/// <param name="dp">The dependency property.</param>
		/// <returns></returns>
		private static bool GetIsUpdating( DependencyObject dp )
		{
			return ( bool ) dp.GetValue( IsUpdatingProperty );
		}

		/// <summary>
		///     Called when [password property changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
		/// </param>
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
		/// <param name="e">
		///     The <see cref="RoutedEventArgs" /> instance containing the event data.
		/// </param>
		private static void PasswordChanged( object sender, RoutedEventArgs e )
		{
			var passwordBox = sender as PasswordBox;

			SetIsUpdating( passwordBox, true );

			if ( passwordBox != null )
			{
				SetPassword( passwordBox, passwordBox.Password );
				SetIsUpdating( passwordBox, false );
			}
		}

		/// <summary>
		///     Sets the is updating.
		/// </summary>
		/// <param name="dp">The dependency property.</param>
		/// <param name="value">
		///     if set to <c>true</c> [value].
		/// </param>
		private static void SetIsUpdating( DependencyObject dp, bool value )
		{
			dp.SetValue( IsUpdatingProperty, value );
		}
	}
}