// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Performs parameter checking for IEntityAccessControlChecker.
    /// </summary>
    /// <remarks>
    /// Motivation is to avoid having to repeatedly check the contents of lists.
    /// </remarks>
    public class SafetyEntityAccessControlChecker : IEntityAccessControlChecker
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="checker">The inner checker.</param>
        public SafetyEntityAccessControlChecker( IEntityAccessControlChecker checker )
        {
            if ( checker == null )
                throw new ArgumentNullException( "checker" );

            Checker = checker;
        }

        /// <summary>
        /// The inner checker.
        /// </summary>
        public IEntityAccessControlChecker Checker { get; private set; }

        /// <summary>
        /// Check access.
        /// </summary>
        public IDictionary<long, bool> CheckAccess( IList<Model.EntityRef> entities, IList<Model.EntityRef> permissions, Model.EntityRef user )
        {
            if ( entities == null )
                throw new ArgumentNullException( nameof( entities ) );
            if ( permissions == null )
                throw new ArgumentNullException( nameof( permissions ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );
#if DEBUG
            if ( entities.Any( x => x == null ) )
            {
                throw new ArgumentException( @"Cannot contain null entries", nameof( entities ) );
            }
            if ( permissions.Any( x => x == null ) )
            {
                throw new ArgumentException( @"Cannot contain null entries", nameof( permissions ) );
            }
#endif

            return Checker.CheckAccess( entities, permissions, user );
        }

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">The <see cref="Model.EntityType"/>s to check. This cannot be null or contain null.</param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        public IDictionary<long, bool> CheckTypeAccess( IList<Model.EntityType> entityTypes, Model.EntityRef permission, Model.EntityRef user )
        {
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( entityTypes.Contains( null ) )
                throw new ArgumentNullException( nameof( entityTypes ), "Cannot contain null" );
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            return Checker.CheckTypeAccess( entityTypes, permission, user );
        }
    }
}
