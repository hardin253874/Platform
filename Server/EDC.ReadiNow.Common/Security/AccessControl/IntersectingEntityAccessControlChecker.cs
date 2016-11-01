// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    ///     An entity access control checker that gives access if all the configured checkers give access.
    /// </summary>
    public class IntersectingEntityAccessControlChecker : IEntityAccessControlChecker
    {
        private readonly IEnumerable<IEntityAccessControlChecker> _entityAccessControlCheckers;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityAccessControlCheckers"></param>
        public IntersectingEntityAccessControlChecker(IEnumerable<IEntityAccessControlChecker> entityAccessControlCheckers)
        {
            if (entityAccessControlCheckers == null)
            {
                throw new ArgumentNullException(nameof(entityAccessControlCheckers));
            }

            _entityAccessControlCheckers = entityAccessControlCheckers;
        }


        /// <summary>
        ///     Check whether the user has all the specified
        ///     <paramref name="permissions">access</paramref> to the specified <paramref name="entities" />.
        /// </summary>
        /// <param name="entities">
        ///     The entities to check. This cannot be null or contain null.
        /// </param>
        /// <param name="permissions">
        ///     The permissions or operations to check. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        ///     The user requesting access. This cannot be null.
        /// </param>
        /// <returns>
        ///     A mapping of each entity ID to whether the user has access (true) or not (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Neither <paramref name="entities" /> nor <paramref name="permissions" /> can contain null.
        /// </exception>
        public IDictionary<long, bool> CheckAccess(IList<EntityRef> entities, IList<EntityRef> permissions, EntityRef user)
        {
            IDictionary<long, bool> results;

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (permissions == null)
            {
                throw new ArgumentNullException(nameof(permissions));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (EntityAccessControlChecker.SkipCheck(user))
            {
                results = entities.Select(e => e.Id).ToDictionarySafe(x => x, x => true);
            }
            else if (entities.Count == 0)
            {
                results = new Dictionary<long, bool>();
            }
            else
            {
                results = new Dictionary<long, bool>();

                var checkerResultsList = _entityAccessControlCheckers.Select(checker => checker.CheckAccess(entities, permissions, user)).ToList();

                foreach (var e in entities)
                {
                    var id = e.Id;
                    results[id] = checkerResultsList.All(checkerResults => HaveAccess(checkerResults, id));
                }
            }

            return results;
        }

        /// <summary>
        /// Is there an access rule for the specified type(s) that includes the requested permission? E.g. create.
        /// </summary>
        /// <param name="entityTypes">The <see cref="EntityType"/>s to check. This cannot be null or contain null.</param>
        /// <param name="permission">The permission being sought.</param>
        /// <param name="user"> The user requesting access. This cannot be null. </param>
        /// <returns>
        ///     A mapping the entity type ID to whether the user can create instances of that
        ///     type (true) or not (false).
        /// </returns>
        public IDictionary<long, bool> CheckTypeAccess(IList<EntityType> entityTypes, EntityRef permission, EntityRef user)
        {
            if ( entityTypes == null )
                throw new ArgumentNullException( nameof( entityTypes ) );
            if ( permission == null )
                throw new ArgumentNullException( nameof( permission ) );
            if ( user == null )
                throw new ArgumentNullException( nameof( user ) );

            IDictionary<long, bool> results;

            if (EntityAccessControlChecker.SkipCheck(user))
            {
                results = entityTypes.Select(e => e.Id).ToDictionarySafe(x => x, x => true);
            }
            else if (entityTypes.Count == 0)
            {
                results = new Dictionary<long, bool>();
            }
            else
            {
                results = new Dictionary<long, bool>();

                var checkerResultsList = _entityAccessControlCheckers.Select(checker => checker.CheckTypeAccess(entityTypes, permission, user)).ToList();

                foreach (var e in entityTypes)
                {
                    var id = e.Id;
                    results[id] = checkerResultsList.All(checkerResults => HaveAccess(checkerResults, id));
                }
            }

            return results;
        }

        /// <summary>
        ///     Returns true if the specified id has access, false otherwise.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool HaveAccess(IDictionary<long, bool> results, long id)
        {
            bool result;

            if (!results.TryGetValue(id, out result))
            {
                result = false;
            }

            return result;
        }
    }
}