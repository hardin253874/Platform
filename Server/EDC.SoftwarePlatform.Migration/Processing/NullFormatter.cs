// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Null formatter.
	/// </summary>
	public class NullFormatter : IFormatProvider, ICustomFormatter
	{
		/// <summary>
		///     Formats the specified format.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="arg">The argument.</param>
		/// <param name="provider">The provider.</param>
		/// <returns></returns>
		public string Format( string format, object arg, IFormatProvider provider )
		{
			if ( arg == null )
			{
				return "";
			}

			var formattable = arg as IFormattable;

			if ( formattable != null )
			{
				return formattable.ToString( format, provider );
			}

			return arg.ToString( );
		}

		/// <summary>
		///     Gets the format.
		/// </summary>
		/// <param name="service">The service.</param>
		/// <returns></returns>
		public object GetFormat( Type service )
		{
			if ( service == typeof ( ICustomFormatter ) )
			{
				return this;
			}

			return null;
		}
	}
}