// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// A tuple combining a subject, permission and entity types.
    /// </summary>
    internal class SubjectPermissionTypesTuple : IEquatable<SubjectPermissionTypesTuple>
    {
        /// <summary>
        /// The cached hash code.
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// Create a new <see cref="SubjectPermissionTypesTuple"/>.
        /// </summary>
        /// <param name="subjectId">
        /// The subject ID.
        /// </param>
        /// <param name="permissionId">
        /// The permission ID.
        /// </param>
        /// <param name="entityTypes">
        /// The entity types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entityTypes"/> cannot be null.
        /// </exception>
        public SubjectPermissionTypesTuple(long subjectId, EntityRef permissionId, IEnumerable<long> entityTypes)
        {
            List<long> entityTypesList;

            SubjectId = subjectId;
            PermissionId = permissionId == null ? -1 : permissionId.Id;

            if ( entityTypes != null )
            {
                entityTypesList = new List<long>( entityTypes );
                entityTypesList.Sort( );
                EntityTypes = entityTypesList;
            }

            _hashCode = GenerateHashCode(subjectId, PermissionId, EntityTypes);
        }

        /// <summary>
        /// The subject ID.
        /// </summary>
        public long SubjectId { get; }

        /// <summary>
        /// The permission ID.
        /// </summary>
        public long PermissionId { get; }

        /// <summary>
        /// The types.
        /// </summary>
        public IEnumerable<long> EntityTypes { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(SubjectPermissionTypesTuple other)
        {
            return Equals((object) other);
        }

        /// <summary>
        /// Are two objects equal?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            SubjectPermissionTypesTuple other;
            bool result;

            result = false;
            other = obj as SubjectPermissionTypesTuple;
            if (other != null)
            {
                result = SubjectId == other.SubjectId
                         && PermissionId == other.PermissionId
                         && SequenceEqual( EntityTypes, other.EntityTypes );;
            }

            return result;
        }

        /// <summary>
        /// Are two sequences equal, or both null.
        /// </summary>
        /// <typeparam name="T">Type of sequence.</typeparam>
        /// <param name="first">First sequence.</param>
        /// <param name="second">Second sequence.</param>
        /// <returns></returns>
        private static bool SequenceEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if ( first == null )
                return second == null;

            if ( second == null )
                return false;

            return first.SequenceEqual( second );
        }

        /// <summary>
        /// Generate a hash code.
        /// </summary>
        /// <returns>
        /// The hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Create the hash code.
        /// </summary>
        /// <returns></returns>
        internal static int GenerateHashCode(long subjectId, long permissionId, IEnumerable<long> entityTypes)
        {
            int result;

            unchecked
            {
                result = 17;
				result = result * 92821 + subjectId.GetHashCode( );
				result = result * 92821 + permissionId.GetHashCode( );
                if ( entityTypes != null )
                {
                    result = entityTypes.Aggregate( result, ( current, entityType ) => current * 92821 + entityType.GetHashCode( ) );
                }				
            }

            return result;
        }

        /// <summary>
        /// Return a human readable representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Subject '{0}' Permission '{1}' Types: '{2}'", 
                SubjectId, PermissionId, string.Join(",", EntityTypes));
        }
    }
}
