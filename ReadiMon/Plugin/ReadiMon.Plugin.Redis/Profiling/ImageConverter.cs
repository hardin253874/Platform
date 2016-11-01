// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Data;

namespace ReadiMon.Plugin.Redis.Profiling
{
	/// <summary>
	///     Image converter.
	/// </summary>
	internal class ImageConverter : IValueConverter
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
			var entry = o as ProfilerTrace;

			if ( entry == null )
			{
				return null;
			}

			return "pack://application:,,,/ReadiMon.Plugin.Redis;component/Resources/" + entry.Image;
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