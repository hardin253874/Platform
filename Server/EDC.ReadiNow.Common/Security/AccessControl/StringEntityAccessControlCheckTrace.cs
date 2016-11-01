// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Diagnostics.Tracing;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Log out details as human readable as possible.
    /// </summary>
    public class StringEntityAccessControlCheckTrace: IEntityAccessControlCheckTrace
    {
        /// <summary>
        /// Log the access control check.
        /// </summary>
        /// <param name="results">
        /// The result of the check, mapping each entity ID to whether access is allowed (true) or not (false). This cannot be null.
        /// </param>
        /// <param name="permissions">
        /// The permissions or operations checked. This cannot be null or contain null.
        /// </param>
        /// <param name="user">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <param name="cacheResult">
        /// If the entity is present, the check was answered by cache and the value is access was granted (true) or not (false). This may be null.
        /// </param>
        /// <param name="duration">
        /// The total time taken for the check in milliseconds.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> can contain null.
        /// </exception>
        public void TraceCheckAccess(IDictionary<long, bool> results, IList<EntityRef> permissions, EntityRef user, IDictionary<long, bool> cacheResult, long duration)
        {
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            if (permissions.Any(x => x == null))
            {
                throw new ArgumentException("Cannot contain null entries", "permissions");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string cacheResultText;

            if (cacheResult != null)
            {
                cacheResultText = String.Join(",", cacheResult.Select(x => +x.Key + ":" + x.Value));
            }
            else
            {
                cacheResultText = string.Empty;
            }

            PlatformTrace.Instance.TraceSecurityCheck(
                String.Join(",", results.Select(x => + x.Key + ":" + x.Value)),
                String.Join(",", permissions.Select(x => x.ToString())),
                user.ToString(), 
                cacheResultText, duration);
        }

        /// <summary>
        /// Log the "can create" check.
        /// </summary>
        /// <param name="results">
        /// The result of the check, mapping each entity type ID to whether creation is allowed (true) or not (false). This cannot be null.
        /// </param>
        /// <param name="permission">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <param name="user">
        /// The user requesting access. This cannot be null.
        /// </param>
        /// <param name="entityTypes">
        /// The entity types being checked. This cannot be null.
        /// </param>
        /// <param name="duration">
        /// The total time taken for the check in milliseconds.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        public void TraceCheckTypeAccess(IDictionary<long, bool> results, EntityRef permission, EntityRef user, IList<EntityRef> entityTypes, long duration)
        {
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }
            if (entityTypes == null)
            {
                throw new ArgumentNullException("entityTypes");
            }
            if (entityTypes.Contains(null))
            {
                throw new ArgumentNullException("entityTypes", "Cannot contain null");
            }
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            PlatformTrace.Instance.TraceSecurityCheckType(
                string.Join(",", results.Select(x => +x.Key + ":" + x.Value)),
                string.Join(",", entityTypes.Select(x => x.ToString())),
                permission.ToString(),
                user.ToString(), 
                duration);            
        }
    }
}
