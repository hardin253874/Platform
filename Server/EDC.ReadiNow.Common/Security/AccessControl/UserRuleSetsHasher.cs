// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Given a user, provide a single hash that can be used to match any two users rule sets to see if they are the same.
    /// </summary>
    public class UserRuleSetsHasher
    {
        IUserRuleSetProvider _ruleSetProvider;

        public UserRuleSetsHasher(IUserRuleSetProvider provider)
        {
            _ruleSetProvider = provider;
        }


		/// <summary>
		/// For the provided user create a hash of the rule set that apply to them
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <returns></returns>
        public int GetUserRuleSetsHash(long userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentNullException("userId");
            }

            var ruleSets = new List<UserRuleSet> {
                _ruleSetProvider.GetUserRuleSet(userId, Permissions.Create),
                _ruleSetProvider.GetUserRuleSet(userId, Permissions.Read),
                _ruleSetProvider.GetUserRuleSet(userId, Permissions.Modify),
                _ruleSetProvider.GetUserRuleSet(userId, Permissions.Delete)
            };

            var ruleSetHash = CryptoHelper.HashObjects(ruleSets);

            return ruleSetHash;
        }
             
    }
}
