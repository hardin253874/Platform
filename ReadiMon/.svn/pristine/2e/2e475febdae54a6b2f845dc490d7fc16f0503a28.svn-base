// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Data;
using ReadiMon.Core;

namespace ReadiMon.Converters
{
	/// <summary>
	///     Plugin Options Section Selected Converter.
	/// </summary>
	public class PluginOptionsSectionSelectedConverter : IValueConverter
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
			if ( value == null )
			{
				return null;
			}

			var section = value as Section;

			if ( section != null )
			{
				section.EntriesView.View.MoveCurrentToFirst( );
				return section.EntriesView.View.CurrentItem;
			}

			return value;
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
		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null )
			{
				return null;
			}

			var section = value as Section;

			if ( section != null )
			{
				section.EntriesView.View.MoveCurrentToFirst( );
				return section.EntriesView.View.CurrentItem;
			}

			return value;
		}
	}
}