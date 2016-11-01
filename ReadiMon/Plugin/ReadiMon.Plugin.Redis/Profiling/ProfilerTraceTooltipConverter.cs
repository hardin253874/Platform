using System;
using System.Globalization;
using System.Windows.Data;

namespace ReadiMon.Plugin.Redis.Profiling
{
	class ProfilerTraceTooltipConverter : IValueConverter
	{
		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			var trace = value as ProfilerTrace;

			if ( trace == null )
			{
				return string.Empty;
			}

			return trace.Tooltip;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException( );
		}
	}
}
