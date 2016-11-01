// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReadiMon.Shared.Controls.TreeListView
{
	/// <summary>
	///     Convert Level to left margin
	/// </summary>
	public class LevelToIndentConverter : IValueConverter
	{
		/// <summary>
		///     The indent size
		/// </summary>
		private const double IndentSize = 19.0;

		/// <summary>
		///     Converts the specified o.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="type">The type.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		public object Convert( object o, Type type, object parameter, CultureInfo culture )
		{
			return new Thickness( ( int ) o * IndentSize, 0, 0, 0 );
		}

		/// <summary>
		///     Converts the back.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="type">The type.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException"></exception>
		public object ConvertBack( object o, Type type, object parameter, CultureInfo culture )
		{
			throw new NotSupportedException( );
		}
	}

	/// <summary>
	///     Can Expand converter
	/// </summary>
	public class CanExpandConverter : IValueConverter
	{
		/// <summary>
		///     Converts the specified o.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="type">The type.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		public object Convert( object o, Type type, object parameter, CultureInfo culture )
		{
			if ( ( bool ) o )
			{
				return Visibility.Visible;
			}

			return Visibility.Hidden;
		}

		/// <summary>
		///     Converts the back.
		/// </summary>
		/// <param name="o">The o.</param>
		/// <param name="type">The type.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException"></exception>
		public object ConvertBack( object o, Type type, object parameter, CultureInfo culture )
		{
			throw new NotSupportedException( );
		}
	}
}