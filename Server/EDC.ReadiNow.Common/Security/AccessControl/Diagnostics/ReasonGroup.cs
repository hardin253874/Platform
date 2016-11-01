// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Security.AccessControl.Diagnostics
{
    /// <summary>
    /// A group of reasons why access control was granted.
    /// </summary>
    class AccessReasonGrouping : Tuple<long, string, AccessRuleScope>
    {
        public AccessReasonGrouping(AccessReason reason) 
            : base( reason.SubjectId, reason.PermissionsText, reason.AccessRuleScope)
        {
        }
        public AccessReasonGrouping( AccessReason reason, AccessRuleScope scope )
            : base( reason.SubjectId, reason.PermissionsText, scope )
        {
        }

        public long SubjectId => Item1;

        public string PermissionText => Item2;

        public AccessRuleScope AccessRuleScope => Item3;
    }
}
