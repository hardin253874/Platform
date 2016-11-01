// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace ReadiMon.Core
{
	/// <summary>
	///     VisualTree extension methods.
	/// </summary>
	public static class VisualTreeExtensions
	{
		/// <summary>
		///     Gets the visual ancestor.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static T GetVisualAncestor<T>( this DependencyObject d ) where T : class
		{
			DependencyObject item = VisualTreeHelper.GetParent( d );

			while ( item != null )
			{
				var itemAsT = item as T;

				if ( itemAsT != null )
				{
					return itemAsT;
				}


				item = VisualTreeHelper.GetParent( item );
			}

			return null;
		}

		/// <summary>
		///     Gets the visual ancestor.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static DependencyObject GetVisualAncestor( this DependencyObject d, Type type )
		{
			DependencyObject item = VisualTreeHelper.GetParent( d );

			while ( item != null )
			{
				if ( item.GetType( ) == type )
				{
					return item;
				}

				item = VisualTreeHelper.GetParent( item );
			}

			return null;
		}

		/// <summary>
		///     Gets the visual descendant.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static T GetVisualDescendant<T>( this DependencyObject d ) where T : DependencyObject
		{
			return d.GetVisualDescendants<T>( ).FirstOrDefault( );
		}

		/// <summary>
		///     Gets the visual descendants.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="d">The d.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetVisualDescendants<T>( this DependencyObject d ) where T : DependencyObject
		{
			int childCount = VisualTreeHelper.GetChildrenCount( d );

			for ( int n = 0; n < childCount; n++ )
			{
				DependencyObject child = VisualTreeHelper.GetChild( d, n );

				if ( child is T )
				{
					yield return ( T ) child;
				}

				foreach ( T match in GetVisualDescendants<T>( child ) )
				{
					yield return match;
				}
			}
		}
	}
}