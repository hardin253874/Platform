// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// </summary>
    public interface IAccessRuleQueryFactory
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        IDictionary<SubjectPermissionTuple, IList<AccessRuleQuery>> GetQueries();
    }
}