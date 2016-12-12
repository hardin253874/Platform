// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.QueryEngine.Builder.SqlObjects;

namespace ReadiNow.QueryEngine.Builder
{
    /// <summary>
    /// An access rule. It applies to a type (and implicitly all derived types).
    /// It may or may not explicitly allow access to all instances.
    /// </summary>
    class AccessRuleCte
    {
        /// <summary>
        /// The name of the CTE that contains the rule.
        /// </summary>
        public string CteName { get; set; }

        /// <summary>
        /// The actual Sql CTE.
        /// </summary>
        public SqlCte SqlCte { get; set; }

        /// <summary>
        /// If true, then the CTE was generated with the root type suppressed.
        /// </summary>
        public bool SqlCteSuppressedRootType { get; set; }

        /// <summary>
        /// If true, then allow access to all instances of this type, and its derived.
        /// Allows other security rules to be short cut.
        /// </summary>
        public bool AllowAll { get; set; }

        /// <summary>
        /// The type ID that this rule applies to.
        /// </summary>
        public long RuleForTypeId { get; set; }

        /// <summary>
        /// Callback that actually builds the Sql CTE.
        /// Bool argument: true if the rule will be applied to all instances. I.e. the type of the rule is a superset of the 
        /// set of entities that it will be applied to.
        /// </summary>
        public Action<bool> PrepareCte { get; set; }
    }


    /// <summary>
    /// The manner in which an access rule relates to a particular type.
    /// </summary>
    class AccessRuleCteTypeRegistration
    {
        /// <summary>
        /// The access rule.
        /// </summary>
        public AccessRuleCte AccessRuleCte { get; set; }

        /// <summary>
        /// Some type, to which the rule relates.
        /// (It may be the same, an ancestor, or a descendent)
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// Rule is applicable to all instances of type.
        /// (That is to say, the rule applies to the type directly, or to an ancestor of the type. But not to a derived type).
        /// </summary>
        public bool RuleIsApplicableToAllInstances { get; set; }

        /// <summary>
        /// Access rule cte type.
        /// </summary>
        public AccessRuleCteTypeEnum AccessRuleType { get; set; }
    }
}
