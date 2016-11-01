// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Entity access control checker that offers the union of two providers.
    /// </summary>
    /// <remarks>
    /// Motivation is to avoid having to repeatedly check the contents of lists.
    /// </remarks>
    public class UnionEntityAccessControlChecker : IEntityAccessControlChecker
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="firstChecker">The first checker.</param>
		/// <param name="secondChecker">The second checker.</param>
		/// <param name="typeCheckerUseFirstCheckerOnly">if set to <c>true</c> [can create use first checker only].</param>
		/// <exception cref="System.ArgumentNullException">
		/// firstChecker
		/// or
		/// secondChecker
		/// </exception>
        public UnionEntityAccessControlChecker( IEntityAccessControlChecker firstChecker, IEntityAccessControlChecker secondChecker, bool typeCheckerUseFirstCheckerOnly )
        {
            if ( firstChecker == null )
                throw new ArgumentNullException( "firstChecker" );
            if ( secondChecker == null )
                throw new ArgumentNullException( "secondChecker" );

            FirstChecker = firstChecker;
            SecondChecker = secondChecker;
            TypeCheckerUseFirstCheckerOnly = typeCheckerUseFirstCheckerOnly;
        }

        /// <summary>
        /// The inner checker.
        /// </summary>
        public IEntityAccessControlChecker FirstChecker { get; }

        /// <summary>
        /// The inner checker.
        /// </summary>
        public IEntityAccessControlChecker SecondChecker { get; }

        /// <summary>
        /// If true, then 'CanCreate' calls are only directed to the first checker.
        /// </summary>
        public bool TypeCheckerUseFirstCheckerOnly { get; }

        /// <summary>
        /// Check access.
        /// </summary>
        public IDictionary<long, bool> CheckAccess( IList<Model.EntityRef> entities, IList<Model.EntityRef> permissions, Model.EntityRef user )
        {
            if ( entities == null )
            {
                throw new ArgumentNullException( "entities" );
            }
            if ( permissions == null )
            {
                throw new ArgumentNullException( "permissions" );
            }
            if ( user == null )
            {
                throw new ArgumentNullException( "user" );
            }

            IDictionary<long, bool> result;

            // Check using first checker
            IDictionary<long, bool> firstResult = FirstChecker.CheckAccess( entities, permissions, user );

            // Determine denied entries
            List<EntityRef> outstandingEntities = null;
            bool anyGrantsInFirst = false;
            foreach ( EntityRef entity in entities )
            {
                if ( firstResult[entity.Id] )
                {
                    anyGrantsInFirst = true;
                }
                else
                {
                    if ( outstandingEntities == null )
                        outstandingEntities = new List<EntityRef>( );
                    outstandingEntities.Add( entity );
                }
            }

            if ( outstandingEntities == null)
            {
                result = firstResult;
            }
            else
            {
                // Check using second checker
                IDictionary<long, bool> secondResult = SecondChecker.CheckAccess( outstandingEntities, permissions, user );

                // If first granted nothing, then second checker gives a complete picture of the result
                if ( !anyGrantsInFirst )
                {
                    result = secondResult;
                }
                else
                {
                    // Combine results from both checkers
                    result = new Dictionary<long, bool>( entities.Count );
                    foreach ( var pair in firstResult )
                    {
                        result[pair.Key] = pair.Value || secondResult [ pair.Key ];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">The <see cref="EntityType"/>s to check. This cannot be null or contain null.</param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        public IDictionary<long, bool> CheckTypeAccess( IList<Model.EntityType> entityTypes, Model.EntityRef permission, Model.EntityRef user )
        {
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            IDictionary<long, bool> result;

            // Check using first checker
            IDictionary<long, bool> firstResult = FirstChecker.CheckTypeAccess( entityTypes, permission, user );

            if ( TypeCheckerUseFirstCheckerOnly )
            {
                return firstResult;
            }

            // Determine denied entries
            List<Model.EntityType> outstandingEntities = null;
            bool anyGrantsInFirst = false;
            foreach ( Model.EntityType entity in entityTypes )
            {
                if ( firstResult [ entity.Id ] )
                {
                    anyGrantsInFirst = true;
                }
                else
                {
                    if ( outstandingEntities == null )
                        outstandingEntities = new List<Model.EntityType>( );
                    outstandingEntities.Add( entity );
                }
            }

            if ( outstandingEntities == null )
            {
                result = firstResult;
            }
            else
            {
                // Check using second checker
                IDictionary<long, bool> secondResult = SecondChecker.CheckTypeAccess( outstandingEntities, permission, user );

                // If first granted nothing, then second checker gives a complete picture of the result
                if ( !anyGrantsInFirst )
                {
                    result = secondResult;
                }
                else
                {
                    // Combine results from both checkers
                    result = new Dictionary<long, bool>( entityTypes.Count );
                    foreach ( var pair in firstResult )
                    {
                        result [ pair.Key ] = pair.Value || secondResult [ pair.Key ];
                    }
                }
            }

            return result;
        }
    }
}
