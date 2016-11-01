// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	///     Redis PubSub Per-Tenant Cache message.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	[DataContract]
	public class RedisPubSubPerTenantCacheMessage<TKey> : IEquatable<RedisPubSubPerTenantCacheMessage<TKey>>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPubSubPerTenantCacheMessage{TKey}" /> class.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <exception cref="System.ArgumentNullException">keys</exception>
		public RedisPubSubPerTenantCacheMessage( params PerTenantCacheMessage<RedisPubSubCacheMessage<TKey>>[ ] keys )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			Keys = keys.ToList( );
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RedisPubSubPerTenantCacheMessage{TKey}" /> class.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <exception cref="System.ArgumentNullException">keys</exception>
		public RedisPubSubPerTenantCacheMessage( IEnumerable<PerTenantCacheMessage<RedisPubSubCacheMessage<TKey>>> keys )
		{
			if ( keys == null )
			{
				throw new ArgumentNullException( "keys" );
			}

			Keys = keys.ToList( );
		}

		/// <summary>
		///     Prevents a default instance of the <see cref="RedisPubSubPerTenantCacheMessage{TKey}" /> class from being created.
		/// </summary>
		private RedisPubSubPerTenantCacheMessage( )
		{
		}

		/// <summary>
		///     Gets or sets the keys.
		/// </summary>
		/// <value>
		///     The keys.
		/// </value>
		[DataMember( Order = 1 )]
		public List<PerTenantCacheMessage<RedisPubSubCacheMessage<TKey>>> Keys
		{
			get;
			set;
		}

		/// <summary>
		///     Tests equality.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool Equals( RedisPubSubPerTenantCacheMessage<TKey> other )
		{
			if ( ReferenceEquals( null, other ) )
			{
				return false;
			}

			if ( ReferenceEquals( this, other ) )
			{
				return true;
			}

			return Keys.SequenceEqual( other.Keys );
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
			if ( ReferenceEquals( null, obj ) )
			{
				return false;
			}

			if ( ReferenceEquals( this, obj ) )
			{
				return true;
			}

			if ( obj.GetType( ) != GetType( ) )
			{
				return false;
			}

			return Equals( ( RedisPubSubPerTenantCacheMessage<TKey> ) obj );
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

				if ( Keys != null )
				{
					hash = hash * 92821 + Keys.GetHashCode( );
				}

				return hash;
			}
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return string.Join( ", ", Keys );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( RedisPubSubPerTenantCacheMessage<TKey> left, RedisPubSubPerTenantCacheMessage<TKey> right )
		{
			return Equals( left, right );
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( RedisPubSubPerTenantCacheMessage<TKey> left, RedisPubSubPerTenantCacheMessage<TKey> right )
		{
			return !Equals( left, right );
		}
	}
}