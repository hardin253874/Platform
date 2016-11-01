// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;

namespace ReadiMon.Plugin.Graphs
{
	/// <summary>
	///     BrowserBehavior class.
	/// </summary>
	public static class BrowserBehavior
	{
		/// <summary>
		///     The HTML property
		/// </summary>
		public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached( "Html", typeof ( string ), typeof ( BrowserBehavior ), new FrameworkPropertyMetadata( OnHtmlChanged ) );

		/// <summary>
		///     Gets the HTML.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		[AttachedPropertyBrowsableForType( typeof ( WebBrowser ) )]
		public static string GetHtml( WebBrowser d )
		{
			return ( string ) d.GetValue( HtmlProperty );
		}

		static void OnHtmlChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			var wb = d as WebBrowser;
			if ( wb != null )
			{
				var content = e.NewValue as string;
				if ( !string.IsNullOrEmpty( content ) )
				{
					wb.NavigateToString( content );
				}
			}
		}

		/// <summary>
		///     Sets the HTML.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="value">The value.</param>
		public static void SetHtml( WebBrowser d, string value )
		{
			d.SetValue( HtmlProperty, value );
		}
	}
}