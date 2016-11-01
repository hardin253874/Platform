// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Globalization;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     EntityRelationshipCacheKey converter class.
	/// </summary>
	public class EntityRelationshipCacheKeyConverter : TypeConverter
	{
		/// <summary>
		///     Returns whether this converter can convert an object of the given type to the type of this converter, using the
		///     specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
		/// <returns>
		///     true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
		{
			if ( sourceType == typeof ( string ) )
			{
				return true;
			}

			return base.CanConvertFrom( context, sourceType );
		}

		/// <summary>
		///     Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
		/// <returns>
		///     An <see cref="T:System.Object" /> that represents the converted value.
		/// </returns>
		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
		{
			if ( value == null )
			{
				return null;
			}

			var stringValue = value as string;

			if ( stringValue != null )
			{
				string[ ] split = stringValue.Split( '_' );

				if ( split.Length == 2 && ! string.IsNullOrEmpty( split[ 0 ] ) && ! string.IsNullOrEmpty( split[ 1 ] ) )
				{
					long id;

					if ( long.TryParse( split[ 0 ], out id ) )
					{
						Direction direction;

						if ( Enum.TryParse( split[ 1 ], out direction ) )
						{
							return new EntityRelationshipCacheKey( id, direction );
						}
					}
				}
			}

			return base.ConvertFrom( context, culture, value );
		}
	}
}