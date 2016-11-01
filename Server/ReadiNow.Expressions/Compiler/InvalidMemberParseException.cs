// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// An internal override of ParseException that is thrown if we fail to look up a field/relationship on a type.
    /// </summary>
    /// <remarks>
    /// Contains requested information, so we can more intelligently invalidate the parse exception if it gets cached.
    /// </remarks>
    class InvalidMemberParseException : ParseException
    {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="message">The message.</param>
        public InvalidMemberParseException(long typeId, string memberName, string message) : base(message)
        {
            TypeId = typeId;
            MemberName = memberName;
        }

        /// <summary>
        /// The type on which we tried to find the member.
        /// </summary>
        public long TypeId { get; private set; }

        /// <summary>
        /// The member name that we could not find.
        /// </summary>
        public string MemberName { get; private set; }
    }
}
