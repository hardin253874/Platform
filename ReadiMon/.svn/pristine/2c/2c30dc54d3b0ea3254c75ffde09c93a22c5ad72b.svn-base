// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadiMon.Shared.Core
{
	/// <summary>
	///     VisualTreeHelper methods.
	/// </summary>
	public static class VisualTreeHelperMethods
	{
		/// <summary>
		///     Gets the scroll viewer.
		/// </summary>
		/// <param name="dependencyObject">The dependency object.</param>
		/// <returns></returns>
		public static DependencyObject GetScrollViewer( DependencyObject dependencyObject )
		{
			if ( dependencyObject is ScrollViewer )
			{
				return dependencyObject;
			}

			for ( int i = 0; i < VisualTreeHelper.GetChildrenCount( dependencyObject ); i++ )
			{
				var child = VisualTreeHelper.GetChild( dependencyObject, i );

				var result = GetScrollViewer( child );

				if ( result != null )
				{
					return result;
				}
			}

			return null;
		}
	}
}