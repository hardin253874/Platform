// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Data;

namespace ReadiMon.Shared.Converters
{
	/// <summary>
	///     Date Time to String converter.
	/// </summary>
	public class DateTimeToStringConverter : IValueConverter
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
			if ( value is DateTime )
			{
				var dt = ( DateTime ) value;

				if ( dt == DateTime.MinValue )
				{
					return string.Empty;
				}

				if ( dt.Kind == DateTimeKind.Utc )
				{
					return dt.ToLocalTime( ).ToString( CultureInfo.CurrentCulture );
				}

				return dt.ToLocalTime( ).ToString( CultureInfo.CurrentCulture );
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
		/// <exception cref="System.NotImplementedException"></exception>
		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException( );
		}
	}
}