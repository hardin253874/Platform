// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Sets the text to selected upon focus.
	/// </summary>
	public class SelectTextOnFocus : DependencyObject
	{
		/// <summary>
		///     The active property
		/// </summary>
		public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached(
			"Active",
			typeof( bool ),
			typeof( SelectTextOnFocus ),
			new PropertyMetadata( false, ActivePropertyChanged ) );

		/// <summary>
		///     Gets the active.
		/// </summary>
		/// <param name="object">The object.</param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForChildren( IncludeDescendants = false )]
		[AttachedPropertyBrowsableForType( typeof( TextBox ) )]
		public static bool GetActive( DependencyObject @object )
		{
			return ( bool ) @object.GetValue( ActiveProperty );
		}

		/// <summary>
		///     Sets the active.
		/// </summary>
		/// <param name="object">The object.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetActive( DependencyObject @object, bool value )
		{
			@object.SetValue( ActiveProperty, value );
		}

		/// <summary>
		///     Actives the property changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
		private static void ActivePropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			if ( d is TextBox )
			{
				var textBox = d as TextBox;

				if ( ( e.NewValue as bool? ).GetValueOrDefault( false ) )
				{
					textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
					textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
				}
				else
				{
					textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
					textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
				}
			}
		}

		/// <summary>
		///     Gets the parent from visual tree.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		private static DependencyObject GetParentFromVisualTree( object source )
		{
			DependencyObject parent = source as UIElement;

			while ( parent != null && !( parent is TextBox ) )
			{
				parent = VisualTreeHelper.GetParent( parent );
			}

			return parent;
		}

		/// <summary>
		///     Called when [keyboard focus select text].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
		private static void OnKeyboardFocusSelectText( object sender, KeyboardFocusChangedEventArgs e )
		{
			var textBox = e.OriginalSource as TextBox;

			if ( textBox != null )
			{
				textBox.SelectAll( );
			}
		}

		/// <summary>
		///     Called when [mouse left button down].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
		private static void OnMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			DependencyObject dependencyObject = GetParentFromVisualTree( e.OriginalSource );

			if ( dependencyObject == null )
			{
				return;
			}

			var textBox = ( TextBox ) dependencyObject;

			if ( !textBox.IsKeyboardFocusWithin )
			{
				textBox.Focus( );
				e.Handled = true;
			}
		}
	}
}