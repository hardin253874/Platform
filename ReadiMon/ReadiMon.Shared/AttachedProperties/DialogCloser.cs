// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;

namespace ReadiMon.Shared.AttachedProperties
{
	/// <summary>
	///     The DialogCloser class.
	/// </summary>
	public static class DialogCloser
	{
		/// <summary>
		///     The dialog result property
		/// </summary>
		public static readonly DependencyProperty DialogResultProperty =
			DependencyProperty.RegisterAttached(
				"DialogResult",
				typeof ( bool? ),
				typeof ( DialogCloser ),
				new PropertyMetadata( DialogResultChanged ) );

		/// <summary>
		///     Dialogs the result changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void DialogResultChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e )
		{
			var window = d as Window;
			if ( window != null )
				window.DialogResult = e.NewValue as bool?;
		}

		/// <summary>
		///     Sets the dialog result.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="value">The value.</param>
		public static void SetDialogResult( Window target, bool? value )
		{
			target.SetValue( DialogResultProperty, value );
		}
	}
}