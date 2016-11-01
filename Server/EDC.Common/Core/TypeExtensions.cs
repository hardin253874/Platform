// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.Core
{
	public static class TypeExtensions
	{
		/// <summary>
		///     Determines whether [is subclass of generic] [the specified type].
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="genericType">Type of the generic.</param>
		/// <returns>
		///     <c>true</c> if [is subclass of generic] [the specified type]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsSubclassOfGeneric( this Type type, Type genericType )
		{
			while ( type != typeof ( object ) )
			{
				if ( type != null && type.IsGenericType )
				{
					if ( type.GetGenericTypeDefinition( ) == genericType )
					{
						return true;
					}
				}

				if ( type != null )
				{
					type = type.BaseType;
				}
			}
			return false;
		}
	}
}