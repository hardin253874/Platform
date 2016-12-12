// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// 
    /// </summary>
    static class FanoutHelper
    {
        /// <summary>
        /// The maximum number of related entities that can be loaded before we get red in the logs.
        /// </summary>
        public static int MaxRelatedEntities = EntityWebApiSettings.Current.MaxRelatedLimit;

        /// <summary>
        /// The maximum number of related entities that can be loaded before we get warnings in the logs.
        /// </summary>
        private static readonly int MaxRelatedEntitiesWarning = EntityWebApiSettings.Current.MaxRelatedWarning;

        /// <summary>
        /// Verify that a relationship is under the necessary fanout limit.
        /// </summary>
        /// <param name="relReq"></param>
        /// <param name="entityId"></param>
        /// <param name="relCount"></param>
        internal static void CheckFanoutLimit( RelationshipRequest relReq, long entityId, int relCount )
        {
            string prefix = null;
            bool isError = false;
            int threshold = MaxRelatedEntitiesWarning;

            // Warning or error?
            if ( relCount > MaxRelatedEntitiesWarning && !EntityDataBuilder<IEntity>.BypassMaxRelatedEntities( relReq, entityId ) )
            {
                prefix = "Large number";
                if ( relCount > MaxRelatedEntities )
                {
                    isError = true;
                    prefix = "Exceeded maximum number";
                    threshold = MaxRelatedEntities;
                }
            }

            // All OK
            if ( prefix == null )
                return;

            // Format message
            string message = string.Format(
                    "{0} ({1} > {2}) of related entities for EntityInfoService. Relationship: {3} {4}. {5} entity: {6} {7}\n{8}",
                    prefix,
                    relCount,
                    threshold,
                    relReq.RelationshipTypeId,
                    Entity.GetNameForLogEntry( relReq.RelationshipTypeId.Id ),
                    relReq.IsReverse ? "To" : "From",
                    entityId,
                    Entity.GetNameForLogEntry( entityId ),
                    Environment.StackTrace
                    );

            if ( isError )
                EventLog.Application.WriteError( message );
            else
                EventLog.Application.WriteWarning( message );
        }

    }
}
