// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public abstract class UnaryOperatorNode : ExpressionNode
    {
        /// <summary>
        /// The single argument
        /// </summary>
        public ExpressionNode Argument { get; set; }

        /// <summary>
        /// Getter for arguments
        /// </summary>
        public override List<ExpressionNode> Arguments
        {
            get
            {
                return new List<ExpressionNode> { Argument };
            }
        }

        /// <summary>
        /// Setter for arguments.
        /// </summary>
        /// <param name="argNumber">0 for left, 1 for right.</param>
        /// <param name="argument">The expression</param>
        public override void SetArgument(int argNumber, ExpressionNode argument)
        {
            if (argNumber == 0)
                Argument = argument;
            else
                throw new ArgumentOutOfRangeException("argNumber");
        }


    }

}
