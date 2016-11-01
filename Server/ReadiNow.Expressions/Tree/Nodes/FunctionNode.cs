// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public abstract class FunctionNode : ExpressionNode
    {
        /// <summary>
        /// Getter for arguments
        /// </summary>
        public override List<ExpressionNode> Arguments
        {
            get { return _arguments; }
        }
        private readonly List<ExpressionNode> _arguments = new List<ExpressionNode>();

        public ExpressionNode Argument
        {
            get { return _arguments[0]; }
            set { SetArgument(0, value); }
        }

        public ExpressionNode Left
        {
            get { return _arguments[0]; }
            set { SetArgument(0, value); }
        }

        public ExpressionNode Right
        {
            get { return _arguments[1]; }
            set { SetArgument(1, value); }
        }


        /// <summary>
        /// Setter for arguments.
        /// </summary>
        /// <param name="argNumber">0 for left, 1 for right.</param>
        /// <param name="argument">The expression</param>
        public override void SetArgument(int argNumber, ExpressionNode argument)
        {
            while (_arguments.Count < argNumber)
                _arguments.Add(null);

            if (_arguments.Count == argNumber)
                _arguments.Add(argument);
            else
                _arguments[argNumber] = argument;
        }
    }

}
