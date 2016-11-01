// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;

namespace ReadiNow.Expressions
{
    /// <summary>
    /// Resolves names in scripts.
    /// </summary>
    public interface IScriptNameResolver
    {
		/// <summary>
		/// Given the script name of a type or object, returns its ID.
		/// </summary>
		/// <param name="typeScriptName">Name of the type script.</param>
		/// <returns>
		/// The ID of the type, or zero if there are zero or duplicate matches.
		/// </returns>
        long GetTypeByName(string typeScriptName);

		/// <summary>
		/// Given a type (e.g. Person), resolve a name that could be a field name, or a relationship name.
		/// </summary>
		/// <param name="memberScriptName">Name of the member script.</param>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="memberTypeFilter">Types of members to find.</param>
		/// <returns>
		/// Either a field definition, or relationship definition, or any. Or null there are zero or duplicate matches.
		/// </returns>
        MemberInfo GetMemberOfType(string memberScriptName, long typeId, MemberType memberTypeFilter);

        /// <summary>
        /// Given a type and a name, return any instances of that type that have that name.
        /// </summary>
        /// <remarks>
        /// The name must return a unique exact match. If there are duplicates, then none are returned.
        /// </remarks>
        /// <param name="instanceName">The name (not script name) of the instance.</param>
        /// <param name="typeId">The type of instances to search (including derived types).</param>
        /// <returns>The instance. Or null there are zero or duplicate matches.</returns>
        IEntity GetInstance(string instanceName, long typeId);
    }


}
