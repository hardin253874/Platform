// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.QueryEngine.Builder
{
    /// <summary>
    /// Access rule cte type.
    /// </summary>
    internal enum AccessRuleCteTypeEnum
    {
        /// <summary>
        /// Access granted via default access rules.
        /// </summary>
        Default,

        /// <summary>
        /// Access granted via system access rules.
        /// </summary>
        System,

        /// <summary>
        /// Access granted via secures flags.
        /// </summary>
        Implicit
    }
}
