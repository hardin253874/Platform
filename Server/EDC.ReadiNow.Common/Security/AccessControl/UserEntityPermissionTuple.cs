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
    public class UserEntityPermissionTuple : IEquatable<UserEntityPermissionTuple>
    {
        private int _hashCode;

        /// <summary>
        /// Create a new <see cref="UserEntityPermissionTuple"/>.
        /// </summary>
        /// <param name="userId">
        /// The ID of the user.
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
        /// <seealso cref="UserId"/>
        /// <seealso cref="EntityId"/>
        /// <seealso cref="PermissionIds"/>
        public UserEntityPermissionTuple(long userId, long entityId, IEnumerable<long> permissionIds)
        {
            if (permissionIds == null)
            {
                throw new ArgumentNullException("permissionIds");
            }

            UserId = userId;
            EntityId = entityId;
            PermissionIds = (permissionIds.OrderBy(x => x)).ToArray();
            _hashCode = GenerateHashCode(UserId, EntityId, PermissionIds);
        }

        /// <summary>
        /// Parameter-less constructor used for serialization.
        /// </summary>
        private UserEntityPermissionTuple()
        {
            // Do nothing
        }

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		[OnDeserialized]
		private void OnAfterDeserialization( )
		{
			_hashCode = GenerateHashCode( UserId, EntityId, PermissionIds );
		}

        /// <summary>
        /// The user ID.
        /// </summary>
        [DataMember(Order = 1)]
        public long UserId { get; private set; }

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
            if (obj is UserEntityPermissionTuple)
            {
                result = Equals((UserEntityPermissionTuple) obj);
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
        public bool Equals(UserEntityPermissionTuple other)
        {
            if ( other == null
                || EntityId != other.EntityId
                || UserId != other.UserId)
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
                "UserId: {0} EntityId: {1} PermissionIds: {2}", 
                UserId, 
                EntityId, 
                PermissionIds == null ? "(null)" : String.Join(",", PermissionIds)
            );
        }

        /// <summary>
        /// Create the hash code.
        /// </summary>
        /// <returns></returns>
        internal static int GenerateHashCode(long userId, long entityId, IEnumerable<long> permissionIds)
        {
            int hashCode;

            unchecked
            {
                hashCode = 17;
				hashCode = ( hashCode * 92821 + userId.GetHashCode( ) );
				hashCode = ( hashCode * 92821 + entityId.GetHashCode( ) );
				hashCode = permissionIds.Aggregate( hashCode, ( current, permissionId ) => current * 92821 + permissionId.GetHashCode( ) );
            }

            return hashCode;
        }
    }
}
