// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Converters
{
	/// <summary>
	///     The filter converter class.
	/// </summary>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	public class FilterConverter : IValueConverter
	{
		/// <summary>
		///     Converts the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			List<FilterObject> list = value as List<FilterObject>;

			if ( list == null || list.Count <= 0 )
			{
				return Visibility.Collapsed;
			}

			return list.All( f => f.IsFiltered ) ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///     Converts the back.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="culture">The culture.</param>
		/// <returns></returns>
		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException( );
		}
	}
}