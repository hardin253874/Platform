// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using ReadiNow.Expressions.Tree;
using ReadiNow.Expressions.Tree.Nodes;
using Irony.Parsing;
using EDC.ReadiNow.Expressions;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Context object that maintains state during the compilation process.
    /// </summary>
    public class CompileContext
    {
        /// <summary>
        /// Compilation settings.
        /// </summary>
        public BuilderSettings Settings { get; set; }

        /// <summary>
        /// Variables that have been discovered.
        /// </summary>
        public VariableBag Variables { get; set; }

        /// <summary>
        /// Parameters that were passed in.
        /// </summary>
        public ParameterBag Parameters { get; set; }

        /// <summary>
        /// This is the expression that any identifier applies to if there is no '.' to the left.
        /// </summary>
        public EntityNode ContextExpression
        {
            get { return (EntityNode)Variables.GetVariable(_contextToken).Expression; }
            set { Variables.SetVariable(_contextToken, value, null ); }
        }

        /// <summary>
        /// This is the parent node that child nodes will get placed into.
        /// </summary>
        public ChildContainer ParentNodeExpression
        {
            get { return Variables.ParentNodeExpression; }
            set { Variables.ParentNodeExpression = value; }
        }

        private readonly Token _contextToken = new Token(new Terminal("_"), SourceLocation.Empty, "_", "_");

        /// <summary>
        /// Enter a new variable scope.
        /// </summary>
        public void PushScope()
        {
            Variables = Variables.GetChildScope();
        }

        /// <summary>
        /// Leave a variable scope.
        /// </summary>
        public void PopScope( )
        {
            if (Variables.Parent == null)
                throw new InvalidOperationException("Stack underflow.");
            Variables = Variables.Parent;
        }

        /// <summary>
        /// Assign default decimal places.
        /// </summary>
        internal int GetDecimalPlaces(ExprType type)
        {
            if (type.DecimalPlaces != null)
                return type.DecimalPlaces.Value;

            switch (type.Type)
            {
                case DataType.Currency:
                    return Settings.DefaultCurrencyPlaces;

                case DataType.Decimal:
                default:
                    return Settings.DefaultDecimalPrecision;
            }
        }
        
    }
}
