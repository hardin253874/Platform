// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata.Query.Structured;
using SQ = EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Helper methods for seeking to process queries for security purposes.
    /// </summary>
    public static class QueryInspector
    {
		/// <summary>
		/// Detect if an access rule query simply acts to allow access to all instances of a type.
		/// </summary>
		/// <param name="query">The access rule query.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <returns>
		/// True if it allows access to all instances of its root type, and its derived types. Otherwise false.
		/// </returns>
        public static bool DoesAccessRuleQueryGrantAllOfType( StructuredQuery query, long typeId )
        {
            return DoesAccessRuleQueryGrantAllOfTypeImpl( query, ( reportType ) =>
            {
                if ( reportType == typeId )
                    return true;
                return PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf( typeId ).Contains( reportType );
            });
        }

        /// <summary>
        /// Detect if an access rule query simply acts to allow access to all instances of a type.
        /// </summary>
        /// <param name="query">The access rule query.</param>
        /// <param name="typeIds">The type of resource, for which we want to know if the query grants everything.</param>
        /// <returns>True if it allows access to all instances of its root type, and its derived types. Otherwise false.</returns>
        public static bool DoesAccessRuleQueryGrantAllOfTypes( StructuredQuery query, IList<long> typeIds )
        {
            if (typeIds == null)
                throw new ArgumentNullException("typeIds");

            return DoesAccessRuleQueryGrantAllOfTypeImpl( query, ( reportType ) =>
                {
                    if ( typeIds.Contains( reportType ) )
                        return true;

                    foreach (var typeId in typeIds)
                    {
                        var curSet = PerTenantEntityTypeCache.Instance.GetAncestorsAndSelf( typeId );
                        if ( curSet.Contains( reportType ) )
                            return true;
                    }
                    return false;
                });
        }

		/// <summary>
		/// Detect if an access rule query simply acts to allow access to all instances of a type.
		/// Broken into private with callback so we don't duplicate logic, but don't pay the cost of calculating inheritance too early.
		/// </summary>
		/// <param name="query">The access rule query.</param>
		/// <param name="appliesToType">Type of the applies to.</param>
		/// <returns>
		/// True if it allows access to all instances of its root type, and its derived types. Otherwise false.
		/// </returns>
        private static bool DoesAccessRuleQueryGrantAllOfTypeImpl( StructuredQuery query, Func<long, bool> appliesToType )
        {
            if ( query.Conditions.Any(cond => cond.Operator != ConditionType.Unspecified) )
                return false;

            ResourceEntity root = query.RootEntity as ResourceEntity;
            if ( root == null )
                return false;   // this probably isn't a legitimate security rule

            if ( root.ExactType )
                return false;   // one day we should optimise this scenario case too

            if ( root.RelatedEntities.Any( NodeMightCauseSomeRowsToNotAppear ) )
                return false;

            long reportTypeId = root.EntityTypeId.Id;
            bool result = appliesToType( reportTypeId  );
            return result;
        }

        /// <summary>
        /// Return true if we cannot 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool NodeMightCauseSomeRowsToNotAppear( SQ.Entity node )
        {           
            // Check flags on the node
            RelatedResource relNode = node as RelatedResource;
            if (relNode != null)
            {
                if ( relNode.ResourceMustExist || relNode.CheckExistenceOnly || relNode.ConstrainParent )
                    return true;                
            }
            else
            {
                DownCastResource derivedNode = node as DownCastResource;
                if ( derivedNode != null )
                {
                    if ( derivedNode.MustExist )
                        return true;
                }
                else
                {
                    if ( node is AggregateEntity )
                        return false;   // aggregates will always give us something
                    else
                        return true;
                }
            }

            // Check children
            if (node.RelatedEntities.Any(NodeMightCauseSomeRowsToNotAppear))
                return true;

            return false;
        }

        /// <summary>
        /// Check if a query is undamaged. If schema elements have been deleted, then a query is no longer safe to use for security grants.
        /// </summary>
        /// <param name="query">The query to inspect.</param>
        /// <returns>True if the query is undamaged (e.g. does not refer to missing schema data). Otherwise false.</returns>
        public static bool IsQueryUndamaged( StructuredQuery query )
        {
            if ( query == null )
                throw new ArgumentNullException("query");

            if ( query.InvalidReportInformation == null )
                return true;

            foreach ( var pair in query.InvalidReportInformation )
            {
				if ( pair.Value.Count > 0 )
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the query refers to the current user (and would therefore give different results for different users).
        /// </summary>
        /// <param name="query">The query to inspect.</param>
        /// <returns>True if the query refers to the current user in any way. Otherwise false.</returns>
        public static bool DoesQueryReferToCurrentUser( StructuredQuery query )
        {
            if ( query == null )
                throw new ArgumentNullException( "query" );
            if ( query.Conditions == null )
                throw new ArgumentException( "query.Conditions" );

            bool result = query.Conditions.Any( condition => condition.Operator == ConditionType.CurrentUser );
            return result;
        }
    }
}
