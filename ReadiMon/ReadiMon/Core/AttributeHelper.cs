// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReadiMon.Core
{
	/// <summary>
	///     Attribute helper class
	/// </summary>
	public static class AttributeHelper
	{
		/// <summary>
		///     Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T>( this Type type )
		{
			return type.GetCustomAttributes( typeof ( T ), true ).Cast<T>( ).FirstOrDefault<T>( );
		}

		/// <summary>
		///     Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T>( )
		{
			return Assembly.GetEntryAssembly( ).GetTypes( ).SelectMany( type => type.GetCustomAttributes( typeof ( T ), true ).Cast<T>( ) );
		}

		/// <summary>
		///     Gets the types with attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<Type> GetTypesWithAttribute<T>( )
		{
			return Assembly.GetEntryAssembly( ).GetTypes( ).Where( type => type.GetCustomAttributes( typeof ( T ), true ).Length > 0 );
		}
	}
}