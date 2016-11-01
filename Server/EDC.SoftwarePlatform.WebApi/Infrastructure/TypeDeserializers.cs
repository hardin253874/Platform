// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Jil;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Jil Type Deserializers
	/// </summary>
	public static class TypedDeserializers
	{
		/// <summary>
		///     The method used to construct generic lambdas
		/// </summary>
		private static readonly MethodInfo DeserializeMethod = typeof ( JSON ).GetMethod( "Deserialize", new[ ]
		{
			typeof ( TextReader ),
			typeof ( Options )
		} );

		/// <summary>
		///     Cache of type methods
		/// </summary>
		private static readonly ConcurrentDictionary<Type, Func<TextReader, Options, object>> Methods = new ConcurrentDictionary<Type, Func<TextReader, Options, object>>( );

		/// <summary>
		///     Creates the delegate.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		private static Func<TextReader, Options, object> CreateDelegate( Type type )
		{
			return ( Func<TextReader, Options, object> ) DeserializeMethod.MakeGenericMethod( type ).CreateDelegate( typeof ( Func<TextReader, Options, object> ) );
		}

		/// <summary>
		///     Gets the typed.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static Func<TextReader, Options, object> GetTyped( Type type )
		{
			return Methods.GetOrAdd( type, CreateDelegate );
		}
	}
}