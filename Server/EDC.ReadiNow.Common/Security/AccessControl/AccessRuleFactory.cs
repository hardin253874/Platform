// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Helper methods for creating access rules (also called security queries).
    /// </summary>
    public class AccessRuleFactory : IAccessRuleFactory
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
        public AccessRule AddAllowCreate(Subject subject, SecurableEntity securableEntity)
        {
            return AddAllow(subject, Permissions.Create.ToEnumerable(), securableEntity);
        }

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
        public AccessRule AddAllow(Subject subject, IEnumerable<EntityRef> permissions, SecurableEntity securableEntity)
        {
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }
            if (permissions == null)
            {
                throw new ArgumentNullException("permissions");
            }
            if (permissions.Contains(null))
            {
                throw new ArgumentException("Cannot contain null", "permissions");
            }
            if (securableEntity == null)
            {
                throw new ArgumentNullException("securableEntity");
            }

            AccessRule accessRule;

            accessRule = Entity.Create<AccessRule>();
            accessRule.Name = string.Format("'{0}' accessing '{1}'", subject.Name ?? string.Empty, securableEntity.Name ?? string.Empty);
            accessRule.AccessRuleEnabled = true;
            accessRule.PermissionAccess.AddRange(permissions.Select(x => x.Entity.As<Permission>()));
            accessRule.ControlAccess = securableEntity;
            accessRule.AllowAccessBy = subject;
            accessRule.Save();
            
            subject.Save();

            return accessRule;
        }

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
        public AccessRule AddAllowReadQuery(Subject subject, SecurableEntity securableEntity, Report query)
        {
            return AddAllowByQuery(subject, securableEntity, Permissions.Read.ToEnumerable(), query);
        }

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
        public AccessRule AddAllowModifyQuery(Subject subject, SecurableEntity securableEntity, Report query)
        {
            return AddAllowByQuery(subject, securableEntity, Permissions.Modify.ToEnumerable(), query);
        }

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
        public AccessRule AddAllowDeleteQuery(Subject subject, SecurableEntity securableEntity, Report query)
        {
            return AddAllowByQuery(subject, securableEntity, Permissions.Delete.ToEnumerable(), query);
        }

		/// <summary>
		/// Given the <paramref name="subject" /> the specified access to <paramref name="securableEntity" /> governed by
		/// the query <paramref name="report" />.
		/// </summary>
		/// <param name="subject">The subject (user or role). This cannot be null.</param>
		/// <param name="securableEntity">The secured entity (type). This cannot be null.</param>
		/// <param name="permissions">The permission(s) to add. This cannot be null or contain null.</param>
		/// <param name="report">The query (as a <see cref="Report" />) to add. This should be a new report, not used for any security.
		/// This cannot be null.</param>
		/// <param name="enabled">True if the access rule should be enabled on creation, false if disabled.</param>
		/// <param name="solution">The solution.</param>
		/// <returns>
		/// The <see cref="AccessRule" /> object representing the new query.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">subject
		/// or
		/// securableEntity
		/// or
		/// permissions
		/// or
		/// report</exception>
		/// <exception cref="System.ArgumentException">Cannot contain null - permissions</exception>
		/// <exception cref="ArgumentNullException">No argument can be null.</exception>
		/// <exception cref="ArgumentException">
		///   <paramref name="permissions" /> cannot contain null.</exception>
		public AccessRule AddAllowByQuery( Subject subject, SecurableEntity securableEntity, IEnumerable<EntityRef> permissions,
			Report report, bool enabled = true, Solution solution = null )
		{
			if ( subject == null )
			{
				throw new ArgumentNullException( "subject" );
			}
			if ( securableEntity == null )
			{
				throw new ArgumentNullException( "securableEntity" );
			}
			if ( permissions == null )
			{
				throw new ArgumentNullException( "permissions" );
			}
			if ( permissions.Contains( null ) )
			{
				throw new ArgumentException( "Cannot contain null", "permissions" );
			}
			if ( report == null )
			{
				throw new ArgumentNullException( "report" );
			}

			AccessRule accessRule;

			// Give a name to avoid warnings about unnamed reports
			if ( string.IsNullOrWhiteSpace( report.Name ) )
			{
				report.Name = "Security report";
			}

			accessRule = Entity.Create<AccessRule>( );
			accessRule.Name = string.Format( "'{0}' accessing '{1}'", subject.Name ?? string.Empty, securableEntity.Name ?? string.Empty );
			accessRule.AccessRuleEnabled = enabled;
			accessRule.PermissionAccess.AddRange( permissions.Select( x => x.Entity.As<Permission>( ) ) );
			accessRule.ControlAccess = securableEntity;
			accessRule.AccessRuleReport = report;
			accessRule.AllowAccessBy = subject;

			if ( solution != null )
			{
				accessRule.InSolution = solution;
			}

			accessRule.Save( );

			subject.Save( );

			return accessRule;
		}
	}
}
