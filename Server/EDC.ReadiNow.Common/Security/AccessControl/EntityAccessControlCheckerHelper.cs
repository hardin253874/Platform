// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Helper methods for <see cref="EntityAccessControlChecker"/> when
    /// extracting metadata from <see cref="StructuredQuery"/> objects.
    /// </summary>
    public static class EntityAccessControlCheckerHelper
    {
        /// <summary>
        /// Get the relationship types referenced in the query.
        /// </summary>
        /// <param name="structuredQuery">
        /// The <see cref="StructuredQuery"/> to check. This cannot be null.
        /// </param>
        /// <returns>
        /// A list of relationship types or empty, if the query references no relationships.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="structuredQuery"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="structuredQuery"/>'s RootEntity property cannot be null.
        /// </exception>
        public static IList<EntityRef> GetReferencedRelationshipTypes(StructuredQuery structuredQuery)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }
            if (structuredQuery.RootEntity == null)
            {
                throw new ArgumentNullException("structuredQuery", "RootEntity cannot be null");
            }

            HashSet<EntityRef> result;

            result = new HashSet<EntityRef>();
            IEnumerable<ResourceDataColumn> resourceDataColumns;
            RelatedResource relatedResource;

            resourceDataColumns = structuredQuery.Conditions
                .SelectMany(cond => StructuredQueryHelper.WalkExpressions(cond.Expression))
                .Where(se => se is ResourceDataColumn)
                .Cast<ResourceDataColumn>();
            foreach (ResourceDataColumn resourceDataColumn in resourceDataColumns)
            {
                StructuredQueryHelper.VisitNodes(structuredQuery.RootEntity, (node, ancestors) =>
                {
                    if (node.NodeId == resourceDataColumn.NodeId)
                    {
                        relatedResource = node as RelatedResource;
                        if (relatedResource != null)
                        {
                            result.Add(relatedResource.RelationshipTypeId);
                            result.UnionWith(
                                ancestors.Where(n => n is RelatedResource)
                                         .Cast<RelatedResource>()
                                         .Select(rr => rr.RelationshipTypeId));
                        }
                    }
                });
            }

            return result.ToList();
        }

        /// <summary>
        /// Get the field types referenced in the query condition only. Ignore fields
        /// elsewhere.
        /// </summary>
        /// <param name="structuredQuery">
        /// The <see cref="StructuredQuery"/> to check. This cannot be null.
        /// </param>
        /// <returns>
        /// A list of field types or empty, if the query references no fields.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="structuredQuery"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="structuredQuery"/>'s Conditions property cannot be null.
        /// </exception>
        public static IList<EntityRef> GetFieldTypesReferencedByCondition(StructuredQuery structuredQuery)
        {
            if (structuredQuery == null)
            {
                throw new ArgumentNullException("structuredQuery");
            }
            if (structuredQuery.Conditions == null)
            {
                throw new ArgumentNullException("structuredQuery", "Conditions property cannot be null");
            }

            return structuredQuery.Conditions
                .SelectMany(cond => StructuredQueryHelper.WalkExpressions(cond.Expression))
                .Where(se => se is ResourceDataColumn)
                .Cast<ResourceDataColumn>()
                .Select(rdc => rdc.FieldId)
                .Distinct(EntityRefComparer.Instance)
                .ToList();
        }
        
    }
}
