// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;
using System.Linq;

using EDC.ReadiNow.Core;

namespace EDC.ReadiNow.Expressions
{

    /// <summary>
    /// Deprecated. A class for resolving names that appear in user code. (E.g. document generation).
    /// Note: currently just implemented by relying on 'name'.
    /// </summary>
    [Obsolete("Use Factory.ScriptNameResolver")]
    public static class CodeNameResolver
    {
        /// <summary>
        /// Deprecated. Given the name of a type, returns the type.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns>A definition name.</returns>
        [Obsolete("Use Factory.ScriptNameResolver.GetTypeByName")]
        public static IEntity GetTypeByName(string typeName)
        {
            long typeId = Factory.ScriptNameResolver.GetTypeByName(typeName);

            if (typeId == 0)
                return null;

            return Entity.Get(typeId);
        }

        /// <summary>
        /// Deprecated. Given a type and a name, return any instances of that type that have that name.
        /// Note: this particular overload is more for helping with unit tests, etc.
        /// </summary>
        [Obsolete("Use Factory.ScriptNameResolver.GetInstance")]
        public static IEntity GetInstance(string instanceName, string typeName)
        {
            long typeId = Factory.ScriptNameResolver.GetTypeByName(typeName);
            if (typeId == 0)
                return null;

            return Factory.ScriptNameResolver.GetInstance(instanceName, typeId);
        }

        /// <summary>
        /// Deprecated. Given a type and a name, return any instances of that type that have that name.
        /// </summary>
        /// <param name="instanceName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete("Use Factory.ScriptNameResolver.GetInstance")]
        public static IEnumerable<IEntity> GetInstance(string instanceName, EntityType type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            IEntity result = Factory.ScriptNameResolver.GetInstance(instanceName, type.Id);

            if (result == null)
                return Enumerable.Empty<IEntity>();
            else
                return result.ToEnumerable();
        }

    }

}
