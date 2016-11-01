// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EDC.Security;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Represents a hash of the IDs of a set of security rules.
    /// </summary>
    /// <remarks>
    /// This object is intended to be used in the cache-key of caches that can be shared across users that share the same set of security rules.
    /// This class is intended to be opaque.
    /// </remarks>
    [DataContract]
    public class UserRuleSet
    {
        /// <summary>
        /// Create a new <see cref="UserRuleSet"/>.
        /// </summary>
        /// <param name="rules">
        /// The rules. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="rules"/> cannot be null.
        /// </exception>
        public UserRuleSet( IEnumerable<long> rules )
        {
            if ( rules == null )
                throw new ArgumentNullException( "rules" );

            var orderedRuleList = rules.OrderBy( x => x ).ToArray( );
	        Key = CryptoHelper.HashValues( orderedRuleList );
        }

        /// <summary>
        /// Parameterless constructor used for serialization only.
        /// </summary>
        private UserRuleSet()
        {
            // Do nothing
        }

        public override bool Equals( object obj )
        {
            var other = obj as UserRuleSet;
            if ( other == null )
                return false;
            return Key.Equals( other.Key );
        }

        public override int GetHashCode( )
        {
            return Key.GetHashCode( );
        }

        /// <summary>
        /// The key.
        /// </summary>
        [DataMember(Order = 1)]
        public int Key { get; private set; }
    }
}
