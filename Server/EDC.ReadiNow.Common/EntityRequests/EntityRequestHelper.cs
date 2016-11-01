// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.Common;
using EDC.ReadiNow.Model;
using ReadiNow.EntityGraph;
using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// Static helper methods for working with EntityRequest.
    /// </summary>
    public static class EntityRequestHelper
    {
        /// <summary>
        /// Accept a CSV list of paths, and return a corresponding entity request.
        /// </summary>
        /// <example>
        /// Pass in:
        /// "name, description, worksFor.name, manager.department.name"
        /// Namespaces can also be passed, but use the full namespace name
        /// "core:name, core:description, shared:worksFor.name"
        /// </example>
        /// <remarks>
        /// Relationships are automatically combined together. That is, "worksFor.name" and "worksFor.description"
        /// will collapse into a single relationship with two fields.
        /// Note that this only works if the aliases are string-identical. (i.e. it won't work if one uses a namespace and the other doesn't.
        /// </remarks>
        /// <param name="requestText">A CSV list of aliases of relationships and fields to request.</param>
		/// <param name="settings">The settings.</param>
        /// <returns>A member-request object.</returns>
        [Obsolete("Use Factory.RequestParser.ParseRequestQuery")]
        public static EntityMemberRequest BuildRequest(string requestText, RequestParserSettings settings = null)
        {
            var requestObject = Factory.RequestParser.ParseRequestQuery(requestText, settings);
            return requestObject;
        }

        /// <summary>
        /// Finds the relationship request by type name.
        /// </summary>
        public static RelationshipRequest FindRel(this EntityMemberRequest request, EntityRef findRelationshipType)
        {
            if (findRelationshipType == null)
                return null;
            if (request == null)
                return null;
            if (request.Relationships == null)
                return null;

            string sFind = findRelationshipType.ToString();
            return request.Relationships.FirstOrDefault(r => r.RelationshipTypeId.ToString() == sFind);
        }

        /// <summary>
        /// Finds the members requested for the named relationship.
        /// </summary>
        public static EntityMemberRequest FindRelMembers(this EntityMemberRequest request, EntityRef findRelationshipType)
        {
            var rel = FindRel(request, findRelationshipType);
            if (rel == null)
                return null;
            return rel.RequestedMembers;
        }

        /// <summary>
        /// Finds the named relationships.
        /// </summary>
        public static IEnumerable<RelationshipRequest> FindRelationshipRequests(this EntityMemberRequest request, EntityRef findRelationshipType)
        {
            var relRequests = Delegates.WalkGraph(request.Relationships, rel => rel.RequestedMembers.Relationships);

            string sFind = findRelationshipType.ToString();
            return relRequests.Where(r => r.RelationshipTypeId.ToString() == sFind);
        }

        /// <summary>
        /// Finds all nodes in the request.
        /// </summary>
        public static IEnumerable<EntityMemberRequest> WalkNodes(this EntityMemberRequest request)
        {
            var nodes = Delegates.WalkGraph(request, rq => rq.Relationships.Select(rel => rel.RequestedMembers));
            return nodes;
        }

        /// <summary>
        /// Gets a cache key for the request.
        /// </summary>
        public static string GetCacheKey(this EntityMemberRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.CacheKey == null)
            {
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    request.CacheKey = request.RequestString;
                }
                else
                {
                    // This will at least allow caching if object reference matches
                    request.CacheKey = Guid.NewGuid().ToString();
                }
            }
            return request.CacheKey;
        }

        /// <summary>
        /// Produce a debug string for the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Diagnostic text.</returns>
        public static string Debug(this EntityMemberRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            try
            {
                var sb = new StringBuilder();
                var visited = new HashSet<EntityMemberRequest>();
                GetDebug(request, sb, "", visited);
                string result = sb.ToString();
                if (result.Length > 10000)
                    result = result.Substring(0, 100000) + " ...truncated at 10k";
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static void GetDebug(EntityMemberRequest request, StringBuilder sb, string indent, HashSet<EntityMemberRequest> visited)
        {
            if (visited.Contains(request))
            {
                sb.AppendFormat("{0}Cycle detected", indent);
                return;
            }
            visited.Add(request);

            if (request.Fields != null)
            {
                foreach (var field in request.Fields)
                {
                    if (field == null)
                        sb.AppendFormat("{0}Field: null\n", indent);
                    else
                        sb.AppendFormat("{0}Field: {1}\n", indent, field);
                }
            }
            if (request.Relationships != null)
            {
                foreach (var rel in request.Relationships)
                {
                    if (rel == null)
                    {
                        sb.AppendFormat("{0}Rel: null\n", indent);
                    }
                    else
                    {
                        sb.AppendFormat("{0}Rel: {1}{2}{3}\n", indent, rel.RelationshipTypeId, rel.IsReverse ? " -rev" : "", rel.IsRecursive ? " *rec" : "");
                        if (rel.RequestedMembers != null)
                            EntityRequestHelper.GetDebug(rel.RequestedMembers, sb, indent + "  ", visited);
                    }
                }
            }
        }
    }


}
