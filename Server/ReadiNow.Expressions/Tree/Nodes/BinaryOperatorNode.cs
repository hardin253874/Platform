// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public abstract class BinaryOperatorNode : ExpressionNode
    {
        /// <summary>
        /// Left hand argument
        /// </summary>
        public ExpressionNode Left { get; set; }


        /// <summary>
        /// Right hand argument
        /// </summary>
        public ExpressionNode Right { get; set; }


        /// <summary>
        /// Getter for arguments
        /// </summary>
        public override List<ExpressionNode> Arguments
        {
            get
            {
                return new List<ExpressionNode> { Left, Right };
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
                Left = argument;
            else if (argNumber == 1)
                Right = argument;
            else
                throw new ArgumentOutOfRangeException("argNumber");
        }
    }

}
