// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions
{
    /// <summary>
    /// Represents a single match from the relationship resolver.
    /// </summary>
    public class MemberInfo
    {
        /// <summary>
        /// The member that was matched (a field or relationship)
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        /// The type of member that was matched (a field or a relationship)
        /// </summary>
        public MemberType MemberType { get; set; }

        /// <summary>
        /// If the member was a relationship, then the direction of the relationship relative to the instance.
        /// </summary>
        public Direction Direction { get; set; }
    }

    [Flags]
    public enum MemberType
    {
        /// <summary>
        /// The match was a field.
        /// </summary>
        Field = 1,

        /// <summary>
        /// The match was a relationship.
        /// </summary>
        Relationship = 2,

        /// <summary>
        /// A field or relationship.
        /// </summary>
        Any = 3,            // Field+Relationship

        /// <summary>
        /// A type.
        /// </summary>
        Type = 4
    }
}
