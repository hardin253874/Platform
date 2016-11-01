// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace EDC.ReadiNow.Expressions
{
    /// <summary>
    /// Compiles a calculation string into an expression.
    /// Performs static tasks on calculations.
    /// </summary>
    public interface IExpressionCompiler
    {
        /// <summary>
        /// Convert a string into an expression tree and perform static evaluation.
        /// </summary>
        /// <param name="script">The script to parse.</param>
        /// <param name="settings">Additional settings that control parsing. In particular, contains the type of the root context object, if any.</param>
        /// <returns>An expression tree.</returns>
        IExpression Compile( string script, BuilderSettings settings );

		/// <summary>
		/// Convert a string into an expression tree and perform static evaluation.
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <returns>
		/// An expression tree.
		/// </returns>
        CalculationDependencies GetCalculationDependencies(IExpression expression);

        /// <summary>
        /// Creates a structured query expression.
        /// </summary>
        /// <remarks>
        /// This method does not add the new expression to the target structured query, however it will add new tree nodes to the structured query as they are requred.
        /// </remarks>
        ScalarExpression CreateQueryEngineExpression( IExpression expressionToCreate, QueryBuilderSettings settings );


        /// <summary>
        /// Prewarm the expression engine.
        /// </summary>
        void Prewarm( );
    }
}
