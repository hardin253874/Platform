// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace ReadiMon.Shared.Behaviours
{
	/// <summary>
	///     Avalon Edit Behavior
	/// </summary>
	public sealed class AvalonEditBehavior : Behavior<TextEditor>
	{
		/// <summary>
		///     The AvalonText property
		/// </summary>
		public static readonly DependencyProperty AvalonTextProperty = DependencyProperty.Register( "AvalonText", typeof ( string ), typeof ( AvalonEditBehavior ), new FrameworkPropertyMetadata( default( string ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback ) );

		/// <summary>
		///     Gets or sets the AvalonText.
		/// </summary>
		/// <value>
		///     The AvalonText.
		/// </value>
		public string AvalonText
		{
			get
			{
				return ( string ) GetValue( AvalonTextProperty );
			}
			set
			{
				SetValue( AvalonTextProperty, value );
			}
		}

		/// <summary>
		///     Associates the object on text changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void AssociatedObjectOnTextChanged( object sender, EventArgs eventArgs )
		{
			var textEditor = sender as TextEditor;

			if ( textEditor != null )
			{
				if ( textEditor.Document != null )
				{
					AvalonText = textEditor.Document.Text;
				}
			}
		}

		/// <summary>
		///     Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>
		///     Override this to hook up functionality to the AssociatedObject.
		/// </remarks>
		protected override void OnAttached( )
		{
			base.OnAttached( );

			if ( AssociatedObject != null )
			{
				AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
			}
		}

		/// <summary>
		///     Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>
		///     Override this to unhook functionality from the AssociatedObject.
		/// </remarks>
		protected override void OnDetaching( )
		{
			base.OnDetaching( );

			if ( AssociatedObject != null )
			{
				AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
			}
		}

		/// <summary>
		///     Properties the changed callback.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <param name="dependencyPropertyChangedEventArgs">
		///     The <see cref="DependencyPropertyChangedEventArgs" /> instance
		///     containing the event data.
		/// </param>
		private static void PropertyChangedCallback( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs )
		{
			var behavior = dependencyObject as AvalonEditBehavior;

			if ( behavior != null && behavior.AssociatedObject != null )
			{
				var editor = behavior.AssociatedObject;

				if ( editor.Document != null )
				{
					var caretOffset = editor.CaretOffset;

					if ( dependencyPropertyChangedEventArgs.NewValue == null )
					{
						editor.Document.Text = string.Empty;
					}
					else
					{
						editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString( );
						editor.CaretOffset = caretOffset;
					}
				}
			}
		}
	}
}