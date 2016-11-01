// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReadiMon.Core
{
	/// <summary>
	///     Type helper.
	/// </summary>
	public static class TypeHelper
	{
		/// <summary>
		///     Gets the types that implement.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IEnumerable<Type> GetTypesThatImplement<T>( )
		{
			Type sectionType = typeof ( T );

			return Assembly.GetEntryAssembly( ).GetTypes( ).Where( sectionType.IsAssignableFrom ).Where( type => !type.IsAbstract );
		}
	}
}