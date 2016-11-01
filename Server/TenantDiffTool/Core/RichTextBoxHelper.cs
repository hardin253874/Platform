// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TenantDiffTool.Core
{
	/// <summary>
	///     Rich text box helper.
	/// </summary>
	public class RichTextBoxHelper : DependencyObject
	{
		/// <summary>
		///     Document property.
		/// </summary>
		public static readonly DependencyProperty DocumentXamlProperty =
			DependencyProperty.RegisterAttached(
				"DocumentXaml",
				typeof( FlowDocument ),
				typeof( RichTextBoxHelper ),
				new FrameworkPropertyMetadata
				{
					BindsTwoWayByDefault = true,
					PropertyChangedCallback = ( obj, e ) =>
					{
						var richTextBox = ( RichTextBox ) obj;

						// Parse the XAML to a document (or use XamlReader.Parse())
						FlowDocument doc = GetDocumentXaml( richTextBox );

						// Set the document
						richTextBox.Document = doc;
					}
				} );

		/// <summary>
		///     Gets the document.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public static FlowDocument GetDocumentXaml( DependencyObject obj )
		{
			return ( FlowDocument ) obj.GetValue( DocumentXamlProperty );
		}

		/// <summary>
		///     Sets the document.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="value">The value.</param>
		public static void SetDocumentXaml( DependencyObject obj, FlowDocument value )
		{
			obj.SetValue( DocumentXamlProperty, value );
		}
	}
}