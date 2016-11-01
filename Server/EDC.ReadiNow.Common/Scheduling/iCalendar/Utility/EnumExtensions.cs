// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EDC.ReadiNow.Scheduling.iCalendar.Utility
{
	/// <summary>
	///     Enumeration extensions.
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		///     Gets the description associated with an enumeration value.
		/// </summary>
		/// <param name="enumeration">The enumeration.</param>
		/// <returns></returns>
		public static string ToDescription( this Enum enumeration )
		{
			Type type = enumeration.GetType( );

			MemberInfo[] memInfo = type.GetMember( enumeration.ToString( ) );

			if ( memInfo.Length > 0 )
			{
				object[] attrs = memInfo[ 0 ].GetCustomAttributes( typeof ( EnumDescriptionAttribute ), false );

				if ( attrs.Length > 0 )
				{
					return ( ( EnumDescriptionAttribute ) attrs[ 0 ] ).Description;
				}
			}

			return enumeration.ToString( );
		}

		/// <summary>
		/// Converts a description to the enumeration value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="description">The description.</param>
		/// <returns></returns>
		public static T FromDescription<T>( this string description )
		{
			Type type = typeof( T );

			IEnumerable<MemberInfo> memberInfos = type.GetMembers( ).Where( mi => mi.DeclaringType == type );

			foreach ( MemberInfo memInfo in memberInfos )
			{
				object [ ] attrs = memInfo.GetCustomAttributes( typeof( EnumDescriptionAttribute ), false );

				if ( attrs.Length > 0)
				{
					if ( ( ( EnumDescriptionAttribute ) attrs[ 0 ] ).Description == description )
					{
						return ( T ) Enum.Parse( type, memInfo.Name );
					}
				}
			}

			return default( T );
		}
	}
}