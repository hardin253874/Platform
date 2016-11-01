// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     SerializationUtil class.
	/// </summary>
	public class SerializationUtil
	{
		/// <summary>
		///     Gets the uninitialized object.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static object GetUninitializedObject( Type type )
		{
			return FormatterServices.GetUninitializedObject( type );
		}

		/// <summary>
		///     Called when [deserialized].
		/// </summary>
		/// <param name="obj">The obj.</param>
		public static void OnDeserialized( object obj )
		{
			var ctx = new StreamingContext( StreamingContextStates.All );
			foreach ( MethodInfo mi in GetDeserializedMethods( obj.GetType( ) ) )
			{
				mi.Invoke( obj, new object[]
					{
						ctx
					} );
			}
		}

		/// <summary>
		///     Called when [deserializing].
		/// </summary>
		/// <param name="obj">The obj.</param>
		public static void OnDeserializing( object obj )
		{
			var ctx = new StreamingContext( StreamingContextStates.All );
			foreach ( MethodInfo mi in GetDeserializingMethods( obj.GetType( ) ) )
			{
				mi.Invoke( obj, new object[]
					{
						ctx
					} );
			}
		}

		/// <summary>
		///     Gets the deserialized methods.
		/// </summary>
		/// <param name="targetType">Type of the target.</param>
		/// <returns></returns>
		private static IEnumerable<MethodInfo> GetDeserializedMethods( Type targetType )
		{
			if ( targetType != null )
			{
				// FIXME: cache this
				foreach ( MethodInfo mi in targetType.GetMethods( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ) )
				{
					object[] attrs = mi.GetCustomAttributes( typeof ( OnDeserializedAttribute ), true );
					if ( attrs.Length > 0 )
					{
						yield return mi;
					}
				}
			}
		}

		/// <summary>
		///     Gets the deserializing methods.
		/// </summary>
		/// <param name="targetType">Type of the target.</param>
		/// <returns></returns>
		private static IEnumerable<MethodInfo> GetDeserializingMethods( Type targetType )
		{
			if ( targetType != null )
			{
				// FIXME: cache this
				foreach ( MethodInfo mi in targetType.GetMethods( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ) )
				{
					object[] attrs = mi.GetCustomAttributes( typeof ( OnDeserializingAttribute ), false );
					if ( attrs.Length > 0 )
					{
						yield return mi;
					}
				}
			}
		}
	}
}