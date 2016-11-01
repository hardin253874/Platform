// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	///     Per-Tenant Cache message.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	[DataContract]
	public class PerTenantCacheMessage<TKey> : IEquatable<PerTenantCacheMessage<TKey>>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="PerTenantCacheMessage{TKey}" /> class.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="key">The key.</param>
		/// <exception cref="System.ArgumentNullException">key</exception>
		public PerTenantCacheMessage( long tenantId, TKey key )
		{
			if ( Equals( key, default( TKey ) ) )
			{
				throw new ArgumentNullException( "key" );
			}

			TenantId = tenantId;
			Key = key;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PerTenantCacheMessage{TKey}" /> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <exception cref="System.ArgumentNullException">keys</exception>
		public PerTenantCacheMessage( TKey key )
			: this( RequestContext.IsSet ? RequestContext.TenantId : 0, key )
		{
		}

		/// <summary>
		///     Prevents a default instance of the <see cref="PerTenantCacheMessage{TKey}" /> class from being created.
		/// </summary>
		private PerTenantCacheMessage( )
		{
		}

		/// <summary>
		///     Gets or sets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		[DataMember( Order = 2 )]
		public TKey Key
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		[DataMember( Order = 1 )]
		public long TenantId
		{
			get;
			set;
		}

		/// <summary>
		///     Tests equality.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool Equals( PerTenantCacheMessage<TKey> other )
		{
			if ( ReferenceEquals( null, other ) )
			{
				return false;
			}

			if ( ReferenceEquals( this, other ) )
			{
				return true;
			}

			return TenantId == other.TenantId && Key.Equals( other.Key );
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

			return Equals( ( PerTenantCacheMessage<TKey> ) obj );
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

				hash = hash * 92821 + TenantId.GetHashCode( );
				hash = hash * 92821 + Key.GetHashCode( );

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
			return string.Format( "{0}: {1}", TenantId, Key );
		}

		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( PerTenantCacheMessage<TKey> left, PerTenantCacheMessage<TKey> right )
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
		public static bool operator !=( PerTenantCacheMessage<TKey> left, PerTenantCacheMessage<TKey> right )
		{
			return !Equals( left, right );
		}
	}
}