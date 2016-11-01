// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;
using System.Collections.Concurrent;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Container for information returned by <see cref="IQueryRepository"/>.
    /// </summary>
    public sealed class AccessRuleQuery
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AccessRuleQuery( long accessRuleId, long reportId, long controlsAccessForTypeId, StructuredQuery query, bool ignoreOnReports )
        {
            if ( accessRuleId == 0 )
                throw new ArgumentException( "accessRuleId" );
            if ( reportId == 0 )
                throw new ArgumentException( "reportId" );
            if ( controlsAccessForTypeId == 0 )
                throw new ArgumentException( "controlsAccessForTypeId" );
            if ( query == null )
                throw new ArgumentNullException( "query" );

            AccessRuleId = accessRuleId;
            ReportId = reportId;
            ControlsAccessForTypeId = controlsAccessForTypeId;
            Query = query;
            IgnoreOnReports = ignoreOnReports;
            _grantsAccessToAllOfType = new ConcurrentDictionary<long,bool>();
        }

        /// <summary>
        /// The ID of the access rule that is using the query
        /// </summary>
        public long AccessRuleId { get; private set; }

        /// <summary>
        /// The ID of the access rule report that generated the query
        /// </summary>
        public long ReportId { get; private set; }

        /// <summary>
        /// The ID of the type being controlled by the access rule query
        /// </summary>
        public long ControlsAccessForTypeId { get; private set; }

        /// <summary>
        /// The structured query used to drive the rule.
        /// </summary>
        public StructuredQuery Query { get; private set; }

        /// <summary>
        /// Indicates that this query should not be included in reports calculations.
        /// </summary>
        public bool IgnoreOnReports { get; private set; }

        /// <summary>
        /// Caches whether the report grants access to all instances of a given type.
        /// </summary>
        private ConcurrentDictionary<long, bool> _grantsAccessToAllOfType;

        /// <summary>
        /// Caches whether the report refers to the current user (and would therefore give different results for different users).
        /// </summary>
        private bool? _queryRefersToCurrentUser = null;


        /// <summary>
        /// Determines, and caches, whether the access rule report grants access to all instances of a specified type.
        /// </summary>
        public bool DoesQueryGrantAllOfTypes( long typeId )
        {
            bool result = _grantsAccessToAllOfType.GetOrAdd( typeId, typeId1 =>
                {
                    bool result1 = QueryInspector.DoesAccessRuleQueryGrantAllOfTypes( Query, new [ ] { typeId1 } );
                    return result1;
                });

            return result;
        }


        /// <summary>
        /// Determines, and caches, whether the access rule report refers to the current user (and would therefore give different results for different users).
        /// </summary>
        public bool DoesQueryReferToCurrentUser( )
        {
            if ( !_queryRefersToCurrentUser.HasValue )
            {
                _queryRefersToCurrentUser = QueryInspector.DoesQueryReferToCurrentUser( Query );
            }
            return _queryRefersToCurrentUser.Value;
        }
        
    }    
}
