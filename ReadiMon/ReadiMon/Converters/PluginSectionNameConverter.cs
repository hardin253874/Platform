// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ReadiMon.Core;

namespace ReadiMon.Converters
{
	/// <summary>
	///     Section name converter.
	/// </summary>
	public class PluginSectionNameConverter : IValueConverter
	{
		/// <summary>
		///     Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		///     A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			var sections = value as ObservableCollection<Section>;

			if ( sections == null )
			{
				return value;
			}

			var sectionsWithOrdinal = sections.Where( s => !string.IsNullOrEmpty( s.Name ) && s.Ordinal >= 0 );
			var sectionsWithoutOrdinal = sections.Where( s => !string.IsNullOrEmpty( s.Name ) && s.Ordinal < 0 );

			return sectionsWithOrdinal.OrderBy( s => s.Ordinal ).Union( sectionsWithoutOrdinal.OrderBy( s => s.Name ) );
		}

		/// <summary>
		///     Converts a value.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		///     A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException( );
		}
	}
}