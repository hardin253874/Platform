// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Parser
{
    /// <summary>
    /// 
    /// </summary>
    class Keywords
    {
        /// <summary>
        /// Compares two keywords for equality.
        /// </summary>
        public static bool Equals(string key1, string key2)
        {
            bool result = string.Compare(key1, key2, StringComparison.InvariantCultureIgnoreCase) == 0;
            return result;
        }

        /// <summary>
        /// Logical 'and'
        /// </summary>
        public const string And = "and";

        /// <summary>
        /// Logical 'or'
        /// </summary>
        public const string Or = "or";

        /// <summary>
        /// Logical 'not'
        /// </summary>
        public const string Not = "not";

        /// <summary>
        /// Is
        /// </summary>
        public const string Is = "is";

        /// <summary>
        /// Is
        /// </summary>
        public const string IsNot = "is not";

        /// <summary>
        /// Logical 'true' literal
        /// </summary>
        public const string True = "true";

        /// <summary>
        /// Logical 'false' literal
        /// </summary>
        public const string False = "false";

        /// <summary>
        /// Logical 'null' literal
        /// </summary>
        public const string Null = "null";

        /// <summary>
        /// Select clause
        /// </summary>
        public const string Let = "let";

        /// <summary>
        /// Select clause
        /// </summary>
        public const string Select = "select";

        /// <summary>
        /// Where clause
        /// </summary>
        public const string Where = "where";

        /// <summary>
        /// Union clause
        /// </summary>
        public const string Union = "union";

        /// <summary>
        /// Union All clause
        /// </summary>
        public const string UnionAll = "union all";

        /// <summary>
        /// Order keyword
        /// </summary>
        public const string Order = "order";

        /// <summary>
        /// By keyword
        /// </summary>
        public const string By = "by";

        /// <summary>
        /// Ascendind direction
        /// </summary>
        public const string Asc = "asc";

        /// <summary>
        /// Descending direction
        /// </summary>
        public const string Desc = "desc";

        /// <summary>
        /// Like
        /// </summary>
        public const string Like = "like";

        /// <summary>
        /// Not Like
        /// </summary>
        public const string NotLike = "not like";
    }
}
