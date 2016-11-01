// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Common;
using System;

namespace ReadiNow.Expressions.NameResolver
{
    /// <summary>
    /// A cache key for the CachingScriptNameResolver.
    /// </summary>
    class CachingScriptNameKey : ProtectedTuple<long, string, MemberType>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeId">The type for which member info was requested. (Or 0 if the script name itself resolves a type).</param>
        /// <param name="scriptName">The script name (identifier) being resolved.</param>
        /// <param name="memberType">The type of member that is requested.</param>
        public CachingScriptNameKey(long typeId, string scriptName, MemberType memberType) :
            base(typeId, scriptName, memberType)
        {
            if (typeId < 0)
                throw new ArgumentOutOfRangeException("typeId");
            if (string.IsNullOrEmpty(scriptName))
                throw new ArgumentNullException("scriptName");
        }

        /// <summary>
        /// The type for which member info was requested. (Or 0 if the script name itself resolves a type).
        /// </summary>
        public long TypeId { get { return Item1; } }

        /// <summary>
        /// The script name (identifier) being resolved.
        /// </summary>
        public string ScriptName { get { return Item2; } }

        /// <summary>
        /// The type of member that is requested.
        /// </summary>
        public MemberType MemberType { get { return Item3; } }

    }
}
