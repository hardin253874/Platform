// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A user ID, Entity ID and Permission IDs used for caching.
    /// </summary>
    [DataContract]
    public class RuleSetEntityPermissionTuple : IEquatable<RuleSetEntityPermissionTuple>
    {
        private int _hashCode;

        /// <summary>
        /// Create a new <see cref="UserEntityPermissionTuple"/>.
        /// </summary>
        /// <param name="userRuleSet">
        /// An object that represents a set of rules.
        /// </param>
        /// <param name="entityId">
        /// The ID of the entity.
        /// </param>
        /// <param name="permissionIds">
        /// The permissions. This cannot be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="permissionIds"/> cannot be null.
        /// </exception>
        /// <seealso cref="UserRuleSet"/>
        /// <seealso cref="EntityId"/>
        /// <seealso cref="PermissionIds"/>
        public RuleSetEntityPermissionTuple(UserRuleSet userRuleSet, long entityId, IEnumerable<long> permissionIds)
        {
            if (permissionIds == null)
            {
                throw new ArgumentNullException("permissionIds");
            }

            UserRuleSet = userRuleSet;
            EntityId = entityId;
            PermissionIds = (permissionIds.OrderBy(x => x)).ToArray();
            _hashCode = GenerateHashCode(UserRuleSet, EntityId, PermissionIds);
        }

        /// <summary>
        /// Parameter-less constructor used for serialization.
        /// </summary>
        private RuleSetEntityPermissionTuple( )
        {
            // Do nothing
        }

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		[OnDeserialized]
		private void OnAfterDeserialization( )
		{
			_hashCode = GenerateHashCode( UserRuleSet, EntityId, PermissionIds );
		}

        /// <summary>
        /// The user ID.
        /// </summary>
        [DataMember(Order = 1)]
        public UserRuleSet UserRuleSet { get; private set; }

        /// <summary>
        /// The Entity ID.
        /// </summary>
        [DataMember(Order = 2)]
        public long EntityId { get; private set; }

        /// <summary>
        /// The permission ID.
        /// </summary>
        [DataMember(Order = 3)]
        public long[] PermissionIds { get; private set; }

        /// <summary>
        /// Compare two objects.
        /// </summary>
        /// <param name="obj">
        /// The object to compare.
        /// </param>
        /// <returns>
        /// True if they are equal, false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            bool result;
            if (obj is RuleSetEntityPermissionTuple)
            {
                result = Equals((RuleSetEntityPermissionTuple) obj);
            }
            else
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Compare two <see cref="UserEntityPermissionTuple"/>s.
        /// </summary>
        /// <param name="other">
        /// The <see cref="UserEntityPermissionTuple"/> to compare.
        /// </param>
        /// <returns>
        /// True if they are equal, false otherwise.
        /// </returns>
        public bool Equals(RuleSetEntityPermissionTuple other)
        {
            if ( other == null
                || EntityId != other.EntityId
                || !EqualityComparer<UserRuleSet>.Default.Equals( UserRuleSet, other.UserRuleSet ) )
            {
                return false;
            }

            var perms = PermissionIds;
            var otherPerms = other.PermissionIds;
            int length = perms.Length;
            int otherLength = otherPerms.Length;

            if (length != otherLength)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (perms[i] != otherPerms[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Generate a hashcode.
        /// </summary>
        /// <returns>
        /// The hashcode.
        /// </returns>
        public override int GetHashCode()
        {           
            return _hashCode;
        }

        /// <summary>
        /// Return a human readable representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "UserRuleSet: {0} EntityId: {1} PermissionIds: {2}", 
                UserRuleSet, 
                EntityId, 
                PermissionIds == null ? "(null)" : String.Join(",", PermissionIds)
            );
        }

        /// <summary>
        /// Create the hash code.
        /// </summary>
        /// <returns></returns>
        internal static int GenerateHashCode(UserRuleSet userRuleSet, long entityId, IEnumerable<long> permissionIds)
        {
            if ( permissionIds == null )
                throw new ArgumentNullException( "permissionIds" );

            int hashCode;

            unchecked
            {
                hashCode = 17;
				hashCode = ( int ) ( hashCode * 92821 + EqualityComparer<UserRuleSet>.Default.GetHashCode( userRuleSet ) );
				hashCode = ( int ) ( hashCode * 92821 + entityId );
				hashCode = permissionIds.Aggregate( hashCode, ( current, permissionId ) => current * 92821 + ( int ) permissionId );
            }

            return hashCode;
        }
    }
}
