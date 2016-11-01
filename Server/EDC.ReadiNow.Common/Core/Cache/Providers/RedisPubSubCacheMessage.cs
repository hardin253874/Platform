// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Core.Cache.Providers
{
	/// <summary>
	/// Redis PubSub cache message.
	/// </summary>
	/// <typeparam name="TKey">The cache key.</typeparam>
    [DataContract]
    public class RedisPubSubCacheMessage<TKey> : IEquatable<RedisPubSubCacheMessage<TKey>>
    {
        /// <summary>
        /// Create a new <see cref="RedisPubSubCacheMessage{TKey}"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="RedisPubSubCacheMessageAction"/>.
        /// </param>
        /// <param name="keys">
        /// The keys of the cache entries affected, if any. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keys"/> cannot be null.
        /// </exception>
        public RedisPubSubCacheMessage(RedisPubSubCacheMessageAction action, params TKey[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            Action = action;
            Keys = keys;
        }

        /// <summary>
        /// Create a new <see cref="RedisPubSubCacheMessage{TKey}"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="RedisPubSubCacheMessageAction"/>.
        /// </param>
        /// <param name="keys">
        /// The keys of the cache entries affected, if any. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keys"/> cannot be null.
        /// </exception>
        public RedisPubSubCacheMessage(RedisPubSubCacheMessageAction action, IEnumerable<TKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            Action = action;
            Keys = keys.ToArray();
        }

        /// <summary>
        /// Parameterless constructor used for serialization.
        /// </summary>
        protected RedisPubSubCacheMessage()
        {
            Keys = new TKey[0];
        }

        /// <summary>
        /// The message action.
        /// </summary>
        [DataMember(Order = 1)]
        public RedisPubSubCacheMessageAction Action { get; set; }

        /// <summary>
        /// The cache keys involved in the message.
        /// </summary>
        [DataMember(Order = 2)]
        public TKey[] Keys { get; set; }

        /// <summary>
        /// Return a human readable representation.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Action, string.Join(", ", Keys));
        }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
        public bool Equals(RedisPubSubCacheMessage<TKey> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Action == other.Action && Keys.SequenceEqual(other.Keys);
        }

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RedisPubSubCacheMessage<TKey>)obj);
        }

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
        public override int GetHashCode()
        {
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + Action.GetHashCode( );

				if ( Keys != null )
				{
					hash = hash * 92821 + Keys.GetHashCode( );
				}

				return hash;
			}
        }

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
        public static bool operator ==(RedisPubSubCacheMessage<TKey> left, RedisPubSubCacheMessage<TKey> right)
        {
            return Equals(left, right);
        }

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
        public static bool operator !=(RedisPubSubCacheMessage<TKey> left, RedisPubSubCacheMessage<TKey> right)
        {
            return !Equals(left, right);
        }
    }
}
