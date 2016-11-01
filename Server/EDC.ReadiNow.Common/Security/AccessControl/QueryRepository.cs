// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using ReadiNow.Annotations;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Get the queries for a given user and permission or operation.
    /// </summary>
    public class QueryRepository : IQueryRepository
    {
        /// <summary>
        /// Create a new <see cref="QueryRepository"/>.
        /// </summary>
        public QueryRepository()
            : this(Factory.ReportToQueryConverter, Factory.RuleRepository, Factory.GraphEntityRepository , a => true)
        {
            // Do nothing
        }

		/// <summary>
		/// Create a new <see cref="QueryRepository" />.
		/// </summary>
		/// <param name="reportToQueryConverter">A <see cref="IReportToQueryConverter" />. This cannot be null.</param>
		/// <param name="ruleRepository">The rule repository.</param>
		/// <param name="entityRepository">The entity repository.</param>
		/// <param name="accessRuleFilter">A filter applied to <see cref="AccessRule" />. This cannot be null.</param>
		/// <exception cref="System.ArgumentNullException">
		/// reportToQueryConverter
		/// or
		/// ruleRepository
		/// or
		/// entityRepository
		/// or
		/// accessRuleFilter
		/// </exception>
		/// <exception cref="ArgumentNullException">No argument can be null.</exception>
        internal QueryRepository(IReportToQueryConverter reportToQueryConverter, IRuleRepository ruleRepository, IEntityRepository entityRepository, Predicate<AccessRule> accessRuleFilter)
        {
            if (reportToQueryConverter == null)
            {
                throw new ArgumentNullException("reportToQueryConverter");
            }
            if (ruleRepository == null)
            {
                throw new ArgumentNullException( "ruleRepository" );
            }
            if (entityRepository == null)
            {
                throw new ArgumentNullException( "entityRepository" );
            }
            if (accessRuleFilter == null)
            {
                throw new ArgumentNullException("accessRuleFilter");
            }

            Converter = reportToQueryConverter;
            RuleRepository = ruleRepository;
            ReportEntityRepository = entityRepository;
            Filter = accessRuleFilter;
        }

        /// <summary>
        /// The <see cref="IReportToQueryConverter"/> used.
        /// </summary>
        internal IReportToQueryConverter Converter { get; }

        /// <summary>
        /// The <see cref="IRuleRepository"/> used.
        /// </summary>
        internal IRuleRepository RuleRepository { get; }

        /// <summary>
        /// The <see cref="IEntityRepository"/> used.
        /// </summary>
        internal IEntityRepository ReportEntityRepository { get; }

        /// <summary>
        /// The filter applied to <see cref="AccessRule"/>s.
        /// </summary>
        internal Predicate<AccessRule> Filter { get; }

        /// <summary>
        /// /// Get the queries for a given user and permission or operation.
        /// </summary>
        /// <param name="subjectId">
        /// The ID of the <see cref="Subject"/>, that is a <see cref="UserAccount"/> or <see cref="Role"/> instance.
        /// This cannot be negative.
        /// </param>
        /// <param name="permission">
        /// The permission to get the query for. This should be one of <see cref="Permissions.Read"/>,
        /// <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>. Or null to match all permissions.
        /// </param>
        /// <param name="securableEntityTypes">
        /// The IDs of <see cref="SecurableEntity"/> types being accessed. Or null to match all entity types.
        /// </param>
        /// <returns>
        /// The queries to run.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="subjectId"/> does not exist. Also, <paramref name="permission"/> should
        /// be one of <see cref="Permissions.Read"/>, <see cref="Permissions.Modify"/> or <see cref="Permissions.Delete"/>
        /// </exception>
        public IEnumerable<AccessRuleQuery> GetQueries(long subjectId, [CanBeNull] EntityRef permission, [CanBeNull] IList<long> securableEntityTypes)
        {
            // Get all applicable access rules for this subject/permission/types
            ICollection<AccessRule> accessRules = RuleRepository.GetAccessRules( subjectId, permission, securableEntityTypes );

            IList<AccessRuleQuery> result = new List<AccessRuleQuery>( );

            // Store the enties that, when changed, should invalidate this cache entry.
            using (CacheContext cacheContext = CacheContext.GetContext())
            using (new SecurityBypassContext())
            {
                foreach (AccessRule allowAccess in accessRules)
                {
                    Report accessRuleReport = allowAccess.AccessRuleReport;
                    if ( accessRuleReport == null )
                        continue;

                    // Load the report query graph
                    Report accessRuleReportGraph = ReportEntityRepository.Get<Report>( allowAccess.AccessRuleReport.Id, ReportHelpers.QueryPreloaderQuery );

                    // Convert the report to a structured query
                    StructuredQuery accessRuleReportQuery = Converter.Convert( accessRuleReportGraph, ConverterSettings );
                    StructuredQueryHelper.OptimiseAuthorisationQuery(accessRuleReportQuery);

                    // Add cache invalidations
                    // Consider using .. StructuredQueryHelper.IdentifyStructureCacheDependencies
                    // See also: cacheContext in EntityAccessControlChecker.CheckAccessControlByQuery
                    cacheContext.Entities.Add( accessRuleReport.Id ); 

                    // Should this rule be considered for reports?
                    bool ignoreForReports = allowAccess.AccessRuleIgnoreForReports == true;

                    // Create container object
                    AccessRuleQuery accessRuleQuery = new AccessRuleQuery(allowAccess.Id, accessRuleReport.Id, allowAccess.ControlAccess.Id, accessRuleReportQuery, ignoreForReports);

                    result.Add( accessRuleQuery );
                }
            }                      

            return result;
        }

        /// <summary>
        /// Settings for converting reports to queries.
        /// </summary>
        private static ReportToQueryConverterSettings ConverterSettings = new ReportToQueryConverterSettings { ConditionsOnly = true, SuppressPreload = true };
    }
}
