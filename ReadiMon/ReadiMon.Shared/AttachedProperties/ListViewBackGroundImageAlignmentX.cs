// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Media;

namespace ReadiMon.Shared.AttachedProperties
{
	/// <summary>
	///     ListView BackGround Image attached properties.
	/// </summary>
	public class ListViewBackGroundImage : DependencyObject
	{
		/// <summary>
		///     The alignment x property
		/// </summary>
		public static readonly DependencyProperty AlignmentXProperty = DependencyProperty.RegisterAttached( "AlignmentX", typeof ( AlignmentX ), typeof ( ListViewBackGroundImage ), new PropertyMetadata( AlignmentX.Center ) );

		/// <summary>
		///     The alignment y property
		/// </summary>
		public static readonly DependencyProperty AlignmentYProperty = DependencyProperty.RegisterAttached( "AlignmentY", typeof ( AlignmentY ), typeof ( ListViewBackGroundImage ), new PropertyMetadata( AlignmentY.Center ) );

		/// <summary>
		///     The image source URI property
		/// </summary>
		public static readonly DependencyProperty ImageSourceUriProperty = DependencyProperty.RegisterAttached( "ImageSourceUri", typeof ( string ), typeof ( ListViewBackGroundImage ), new PropertyMetadata( string.Empty ) );

		/// <summary>
		///     Gets the alignment x.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static AlignmentX GetAlignmentX( DependencyObject obj )
		{
			return ( AlignmentX ) obj.GetValue( AlignmentXProperty );
		}

		/// <summary>
		///     Gets the alignment y.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static AlignmentY GetAlignmentY( DependencyObject obj )
		{
			return ( AlignmentY ) obj.GetValue( AlignmentYProperty );
		}

		/// <summary>
		///     Gets the image source URI.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static string GetImageSourceUri( DependencyObject obj )
		{
			return ( string ) obj.GetValue( ImageSourceUriProperty );
		}

		/// <summary>
		///     Sets the alignment x.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="alignment">The alignment.</param>
		public static void SetAlignmentX( DependencyObject obj, AlignmentX alignment )
		{
			obj.SetValue( AlignmentXProperty, alignment );
		}

		/// <summary>
		///     Sets the alignment y.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="alignment">The alignment.</param>
		public static void SetAlignmentY( DependencyObject obj, AlignmentY alignment )
		{
			obj.SetValue( AlignmentYProperty, alignment );
		}

		/// <summary>
		///     Sets the image source URI.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="source">The source.</param>
		public static void SetImageSourceUri( DependencyObject obj, string source )
		{
			obj.SetValue( ImageSourceUriProperty, source );
		}
	}
}