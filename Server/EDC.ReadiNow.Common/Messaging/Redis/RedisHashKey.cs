// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using StackExchange.Redis;

namespace EDC.ReadiNow.Messaging.Redis
{
	/// <summary>
	///     Redis Hash Key.
	/// </summary>
	[DebuggerStepThrough]
	public class RedisHashKey
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisHashKey" /> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="hashField">The hash field.</param>
		/// <exception cref="System.ArgumentNullException">
		///     key
		///     or
		///     hashField
		/// </exception>
		public RedisHashKey( RedisKey key, RedisValue hashField )
		{
			if ( string.IsNullOrEmpty( key ) )
			{
				throw new ArgumentNullException( "key" );
			}

			if ( string.IsNullOrEmpty( hashField ) )
			{
				throw new ArgumentNullException( "hashField" );
			}

			Key = key;
			HashField = hashField;
		}

		/// <summary>
		///     Gets the hash field.
		/// </summary>
		/// <value>
		///     The hash field.
		/// </value>
		public RedisValue HashField
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		public RedisKey Key
		{
			get;
			private set;
		}

		/// <summary>
		///     Creates the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="hashField">The hash field.</param>
		/// <returns></returns>
		public static RedisHashKey Create( RedisKey key, RedisValue hashField )
		{
			return new RedisHashKey( key, hashField );
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var key = obj as RedisHashKey;

			if ( key == null )
			{
				return false;
			}

			return string.Equals( Key, key.Key ) && Equals( HashField, key.HashField );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + Key.GetHashCode( );

				hash = hash * 92821 + HashField.GetHashCode( );

				return hash;
			}
		}
	}
}