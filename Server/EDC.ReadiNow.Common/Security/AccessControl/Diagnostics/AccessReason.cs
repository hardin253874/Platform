// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Model;
using System.Collections.Generic;
using System.Diagnostics;

namespace EDC.ReadiNow.Security.AccessControl.Diagnostics
{
    public enum AccessRuleScope
    {
        /// <summary>
        /// Access is granted to all instances of the type, and derived types.
        /// </summary>
        AllInstances,

        /// <summary>
        /// Conditions may be applied to the access rule, so it may grant access to none, some, or all instances.
        /// Those conditions are not user-specific.
        /// </summary>
        SomeInstances,

        /// <summary>
        /// Conditions may be applied to the access rule, so it may grant access to none, some, or all instances.
        /// Those conditions are user-specific.
        /// </summary>
        PerUser,

        /// <summary>
        /// Access is granted via a secured relationship, and requires the relationship to be present.
        /// </summary>
        SecuredRelationship
    }

    /// <summary>
    /// Captures the reason for access to an entity.
    /// </summary>
    [DebuggerDisplay( "TypeId={TypeId}, SubjectId={SubjectId}" )]
    public class AccessReason
    {
        /// <summary>
        /// The type of instance that is granted access.
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// The subject (role or user) that is granted access.
        /// </summary>
        public long SubjectId { get; set; }

        /// <summary>
        /// The access rule that granted the access.
        /// </summary>
        public AccessRuleQuery AccessRuleQuery { get; set; }

        /// <summary>
        /// The access rule that granted the access.
        /// </summary>
        public AccessRule AccessRule { get; set; }

        /// <summary>
        /// The permissions that are granted.
        /// </summary>
        public IReadOnlyList<long> PermissionIds { get; set; }

        /// <summary>
        /// The permissions that are granted.
        /// </summary>
        public string PermissionsText { get; set; }

        /// <summary>
        /// A description of the reason.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// How broad is the access granted by this rule.
        /// </summary>
        public AccessRuleScope AccessRuleScope { get; set; }

        /// <summary>
        /// True if this is a derived type reason.
        /// </summary>
        public bool IsDerivedType { get; set; }
    }
}
