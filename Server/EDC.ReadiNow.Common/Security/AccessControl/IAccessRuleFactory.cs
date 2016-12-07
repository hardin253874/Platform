// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Net;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Helper methods for adding access rules (also known as security queries).
    /// </summary>
    public interface IAccessRuleFactory
    {
        /// <summary>
        /// Given the <paramref name="subject"/> create access to <paramref name="securableEntity"/>.
        /// </summary>
        /// <param name="subject">
        /// The subject (user or role). This cannot be null.
        /// </param>
        /// <param name="securableEntity">
        /// The secured entity (type). This cannot be null.
        /// </param>
        /// <returns>
        /// The <see cref="AccessRule"/> object representing the new query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        AccessRule AddAllowCreate(Subject subject, SecurableEntity securableEntity);

        /// <summary>
        /// Given the <paramref name="subject"/> the specified access to <paramref name="securableEntity"/>.
        /// </summary>
        /// <param name="subject">
        /// The subject (user or role). This cannot be null.
        /// </param>
        /// <param name="permissions">
        /// The permission(s) to add. This cannot be null or contain null.
        /// </param>
        /// <param name="securableEntity">
        /// The secured entity (type). This cannot be null.
        /// </param>
        /// <returns>
        /// The <see cref="AccessRule"/> object representing the new query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="permissions"/> cannot contain null.
        /// </exception>
        AccessRule AddAllow(Subject subject, IEnumerable<EntityRef> permissions, SecurableEntity securableEntity);

        /// <summary>
        /// Given the <paramref name="subject"/> read access to <paramref name="securableEntity"/> governed by
        /// the query <paramref name="query"/>.
        /// </summary>
        /// <param name="subject">
        /// The subject (user or role). This cannot be null.
        /// </param>
        /// <param name="securableEntity">
        /// The secured entity (type). This cannot be null.
        /// </param>
        /// <param name="query">
        /// The query (as a <see cref="Report"/>) to add. This should be a new report, not used for any security.
        /// This cannot be null.
        /// </param>
        /// <returns>
        /// The <see cref="AccessRule"/> object representing the new query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        AccessRule AddAllowReadQuery(Subject subject, SecurableEntity securableEntity, Report query);

        /// <summary>
        /// Given the <paramref name="subject"/> modify access to <paramref name="securableEntity"/> governed by
        /// the query <paramref name="query"/>.
        /// </summary>
        /// <param name="subject">
        /// The subject (user or role). This cannot be null.
        /// </param>
        /// <param name="securableEntity">
        /// The secured entity (type). This cannot be null.
        /// </param>
        /// <param name="query">
        /// The query (as a <see cref="Report"/>) to add. This should be a new report, not used for any security.
        /// This cannot be null.
        /// </param>
        /// <returns>
        /// The <see cref="AccessRule"/> object representing the new query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        AccessRule AddAllowModifyQuery(Subject subject, SecurableEntity securableEntity, Report query);

        /// <summary>
        /// Given the <paramref name="subject"/> delete access to <paramref name="securableEntity"/> governed by
        /// the query <paramref name="query"/>.
        /// </summary>
        /// <param name="subject">
        /// The subject (user or role). This cannot be null.
        /// </param>
        /// <param name="securableEntity">
        /// The secured entity (type). This cannot be null.
        /// </param>
        /// <param name="query">
        /// The query (as a <see cref="Report"/>) to add. This should be a new report, not used for any security.
        /// This cannot be null.
        /// </param>
        /// <returns>
        /// The <see cref="AccessRule"/> object representing the new query.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null.
        /// </exception>
        AccessRule AddAllowDeleteQuery(Subject subject, SecurableEntity securableEntity, Report query);

		/// <summary>
		/// Given the <paramref name="subject"/> the specified access to <paramref name="securableEntity"/> governed by
		/// the query <paramref name="report"/>.
		/// </summary>
		/// <param name="subject">
		/// The subject (user or role). This cannot be null.
		/// </param>
		/// <param name="securableEntity">
		/// The secured entity (type). This cannot be null.
		/// </param>
		/// <param name="permissions">
		/// The permission(s) to add. This cannot be null or contain null.
		/// </param>
		/// <param name="report">
		/// The query (as a <see cref="Report"/>) to add. This should be a new report, not used for any security.
		/// This cannot be null.
		/// </param>
		/// <param name="enabled">
		/// True if the access rule should be enabled on creation, false if disabled.
		/// </param>
		/// <param name="solution">
		/// The solution.
		/// </param>
		/// <returns>
		/// The <see cref="AccessRule"/> object representing the new query.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// No argument can be null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="permissions"/> cannot contain null.
		/// </exception>
		AccessRule AddAllowByQuery(Subject subject, SecurableEntity securableEntity, IEnumerable<EntityRef> permissions, Report report, bool enabled = true, Solution solution = null);
    }
}