// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Evaluates an expression.
    /// </summary>
    public interface IExpressionRunner
    {
        /// <summary>   
        /// Evaluates an expression tree.
        /// </summary>
        /// <param name="expression">The expression tree.</param>
        /// <param name="settings">Additional settings to be used in evaluation, such as the root context object, timezone info, etc.</param>
        /// <returns>The result of the evaluation.</returns>
        ExpressionRunResult Run( IExpression expression, EvaluationSettings settings );
    }
}
