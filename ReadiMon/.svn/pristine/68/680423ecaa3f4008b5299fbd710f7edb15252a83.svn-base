// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Globalization;
using System.Windows.Data;

namespace ReadiMon.Shared.Converters
{
	/// <summary>
	///     Inverse boolean converter.
	/// </summary>
	[ValueConversion( typeof ( bool ), typeof ( bool ) )]
	public class InverseBooleanConverter : IValueConverter
	{
		#region IValueConverter Members

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
		/// <exception cref="System.InvalidOperationException">The target must be a boolean</exception>
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( targetType != typeof ( bool ) )
			{
				throw new InvalidOperationException( "The target must be a boolean" );
			}

			return !( bool ) value;
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
		/// <exception cref="System.NotSupportedException"></exception>
		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotSupportedException( );
		}

		#endregion
	}
}