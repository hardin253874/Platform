// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.Expressions.Tree.Nodes
{
    public abstract class ZeroArgumentNode : ExpressionNode
    {
        public override List<ExpressionNode> Arguments
        {
            get
            {
                return new List<ExpressionNode>();
            }
        }

        public override void SetArgument(int argNumber, ExpressionNode argument)
        {
            throw new ArgumentOutOfRangeException("argNumber");
        }
    }
}
