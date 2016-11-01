// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using ReadiNow.Expressions.Parser;
using EDC.ReadiNow.Metadata.Query.Structured;
using Irony.Parsing;
using EDC.ReadiNow.Diagnostics;
using ReadiNow.Expressions.Tree;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions.Evaluation
{
    /// <summary>
    /// Main API for interracting with the expression engine.
    /// </summary>
    public class ExpressionRunner : IExpressionRunner
    {
        /// <summary>   
        /// Evaluates an expression tree.
        /// </summary>
        /// <param name="expression">The expression tree.</param>
        /// <param name="settings">Additional settings to be used in evaluation, such as the root context object, timezone info, etc.</param>
        /// <returns>The result of the evaluation.</returns>
        public ExpressionRunResult Run(IExpression expression, EvaluationSettings settings)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if ( settings == null )
                throw new ArgumentNullException( "settings" );

            Expression expressionToRun = expression as Expression;
            if ( expressionToRun == null )
                throw new ArgumentException( "Expected instance of type Expression.", "expression" );
            if ( expressionToRun.Root == null )
                throw new ArgumentException("expression.Root");

            using ( EntryPointContext.AppendEntryPoint( "ExprRun" ) )
            using ( Profiler.Measure( "ExpressionEngine.Run" ) )
            {
                var evalContext = new EvaluationContext
                {
                    Settings = settings,
                };

                object result = expressionToRun.Evaluate( evalContext );

                var resultList = result as IEnumerable<IEntity>;
                if ( resultList != null )
                {
                    result = resultList.ToList( );
                }

                ExpressionRunResult runResult = new ExpressionRunResult( result );
                return runResult;
            }
        }



    }

}
