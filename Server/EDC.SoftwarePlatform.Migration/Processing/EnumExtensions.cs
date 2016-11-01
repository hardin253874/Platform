// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.ComponentModel;
using System.Reflection;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Enumeration extensions.
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string GetDescription( this Enum value )
		{
			Type type = value.GetType( );

			string name = Enum.GetName( type, value );

			if ( name != null )
			{
				FieldInfo field = type.GetField( name );

				if ( field != null )
				{
					var attr = Attribute.GetCustomAttribute( field, typeof ( DescriptionAttribute ) ) as DescriptionAttribute;

					if ( attr != null )
					{
						return attr.Description;
					}
				}
			}

			return null;
		}
	}
}