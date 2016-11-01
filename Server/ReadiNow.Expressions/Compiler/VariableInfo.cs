// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using ReadiNow.Annotations;
using ReadiNow.Expressions.Tree.Nodes;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Information about a variable.
    /// </summary>
    public class VariableInfo
    {
        public VariableInfo( [NotNull] ExpressionNode expression, [CanBeNull] ChildContainer childContainer )
        {
            if ( expression == null )
                throw new ArgumentNullException( nameof( expression ) );

            Expression = expression;
            ChildContainer = childContainer;
        }

        /// <summary>
        /// The expression represented by this variable.
        /// </summary>
        public ExpressionNode Expression { get; }

        /// <summary>
        /// A container that in turn contains things registered during the parsing of the variable.
        /// </summary>
        public ChildContainer ChildContainer { get; }
    }
}
