// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;
using Irony.Parsing;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Interface to support parsing of Irony nodes directly.
    /// </summary>
    /// <remarks>
    /// This is for use by external libraries (i.e. doc gen) that extend the expression language
    /// and wish to compile subtrees.
    /// </remarks>
    public interface IExpressionParseTreeCompiler
    {
        /// <summary>
        /// Compile an expression from a parse tree node.
        /// </summary>
        /// <param name="parseTreeNode">An Irony parse tree node.</param>
        /// <param name="settings">Compilation settings.</param>
        /// <returns>An expression tree.</returns>
        IExpression CompileParseTreeNode(ParseTreeNode parseTreeNode, BuilderSettings settings);
    }
}
