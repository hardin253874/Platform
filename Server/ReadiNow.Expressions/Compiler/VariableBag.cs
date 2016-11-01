// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using ReadiNow.Expressions.Tree.Nodes;
using Irony.Parsing;

namespace ReadiNow.Expressions.Compiler
{
    public class VariableBag
    {
        public VariableBag Parent { get; private set; }

        readonly Dictionary<string, VariableInfo> _variables = new Dictionary<string, VariableInfo>();

        public void SetVariable(Token token, ExpressionNode expr, ChildContainer childContainer)
        {
            VariableInfo info = new VariableInfo( expr, childContainer );

            string name = token.Value.ToString();
            if (name != "_" && HasVariable(token))
                throw ParseExceptionHelper.New( $"Variable name {name} has already been set.", token.Location);

            _variables[name] = info;
        }

        public void ClearVariable(Token token)
        {
            string name = token.Value.ToString();
            _variables.Remove(name);
        }

        public VariableInfo GetVariable(Token token)
        {
            VariableInfo variable;
            string name = token.Value.ToString();
            if (_variables.TryGetValue(name, out variable ) )
                return variable;

            if (Parent != null)
                return Parent.GetVariable(token);

            return null;    
        }

        public bool HasVariable(Token token)
        {
            string name = token.Value.ToString();
            if (_variables.ContainsKey(name))
                return true;

            if (Parent != null)
                return Parent.HasVariable(token);

            return false;
        }

        public VariableBag GetChildScope()
        {
            VariableBag bag = new VariableBag();
            bag.Parent = this;
            return bag;
        }

        public ChildContainer ParentNodeExpression
        {
            get
            {
                if (_parentNodeExpression != null)
                    return _parentNodeExpression;
                if (Parent != null)
                    return Parent.ParentNodeExpression;
                return null;
            }
            set
            {
                _parentNodeExpression = value;
            }
        }
        ChildContainer _parentNodeExpression;

    }

}
