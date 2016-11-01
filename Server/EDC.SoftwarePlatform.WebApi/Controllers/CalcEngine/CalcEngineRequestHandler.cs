// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Controllers.CalcEngine
{
    /// <summary>
    /// Calculation engine request handler.
    /// </summary>
    public class CalcEngineRequestHandler
    {
        /// <summary>
        ///     Compiles the expression.
        /// </summary>
        /// <param name="contextEntityTypeId">The context entity type identifier.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private IExpression CompileExpression(EntityRef contextEntityTypeId, CalcEngineExpression expression)
        {
            var builderSettings = new BuilderSettings
            {
                RootContextType = ExprTypeHelper.EntityOfType(contextEntityTypeId),
                ScriptHost = ScriptHostType.Any
            };

            return Factory.ExpressionCompiler.Compile(expression.Expression, builderSettings);
        }

        /// <summary>
        ///     Runs the expression.
        /// </summary>
        /// <param name="compiledExpression">The compile result.</param>
        /// <param name="contextEntity">The context entity.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        ///     TimeZone information must be provided in settings object, or via
        ///     RequestContext.
        /// </exception>
        private string RunExpression(IExpression compiledExpression, IEntity contextEntity)
        {
            var evaluationSettings = new EvaluationSettings
            {
                ContextEntity = contextEntity
            };

            var requestContext = RequestContext.GetContext();
            if (requestContext != null)
                evaluationSettings.TimeZoneName = requestContext.TimeZone;

            if (evaluationSettings.TimeZoneName == null)
                throw new InvalidOperationException(
                    "TimeZone information must be provided in settings object, or via RequestContext.");

            var result = Factory.ExpressionRunner.Run(compiledExpression, evaluationSettings).Value;

            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        ///     Evaluates the expressions.
        /// </summary>
        /// <param name="contextEntity">The context entity.</param>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        private Dictionary<string, EvaluateResult> EvaluateExpressionsInternal(IEntity contextEntity, IDictionary<string, CalcEngineExpression> expressions)
        {
            var results = new Dictionary<string, EvaluateResult>();

            var contextEntityTypeId = new EntityRef(contextEntity.TypeIds.FirstOrDefault());

            foreach (var item in expressions)
            {
                string key = item.Key;

                try
                {
                    CalcEngineExpression expression = item.Value;

                    IExpression compiledExpression = CompileExpression(contextEntityTypeId, expression);
                    string value = RunExpression(compiledExpression, contextEntity);

                    results[key] = new EvaluateResult
                    {
                        Value = value,
                        CompiledExpression = compiledExpression
                    };
                }
                catch (Exception ex)
                {
                    results[key] = new EvaluateResult
                    {
                        Error = ex.ToString()
                    };
                }
            }

            return results;
        }

        /// <summary>
        ///     Gets the calculate engine eval response.
        /// </summary>
        /// <param name="expressionResults">The expression results.</param>
        /// <returns></returns>
        private CalcEngineEvalResponse GetCalcEngineEvalResponse(
            IDictionary<string, EvaluateResult> expressionResults)
        {
            var response = new CalcEngineEvalResponse {Results = new Dictionary<string, CalcEngineEvalResult>()};            

            foreach (var kvp in expressionResults)
            {
                string key = kvp.Key;
                EvaluateResult result = kvp.Value;

                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    // Error result
                    var errorResult = new CalcEngineEvalResult
                    {
                        ErrorMessage = result.Error
                    };
                    response.Results[key] = errorResult;
                }
                else
                {
                    IExpression expression = result.CompiledExpression;

                    // Success Result
                    var successResult = new CalcEngineEvalResult
                    {
                        Value = result.Value,
                        ResultType = expression.ResultType.Type,
                        IsList = expression.ResultType.IsList,
                        EntityTypeId = expression.ResultType.EntityTypeId
                    };

                    response.Results[key] = successResult;
                }
            }

            return response;
        }

        /// <summary>
        /// Evaluates the expressions.
        /// </summary>
        /// <param name="contextEntity">The context entity.</param>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// ContextEntity is not specified.
        /// or
        /// Expressions are not specified.
        /// </exception>
        /// <exception cref="ArgumentException">Expressions are not specified.</exception>
        public CalcEngineEvalResponse EvaluateExpressions(IEntity contextEntity, IDictionary<string, CalcEngineExpression> expressions)
        {            
            if (contextEntity == null)
                throw new ArgumentNullException(nameof(contextEntity), @"ContextEntity is not specified.");
            
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions), @"Expressions are not specified.");

            if (expressions.Count == 0)
                throw new ArgumentException(@"Expressions are not specified.", nameof(expressions));

            var evalResults = EvaluateExpressionsInternal(contextEntity, expressions);

            return GetCalcEngineEvalResponse(evalResults);
        }

        /// <summary>
        ///     Evaluate result class
        /// </summary>
        private class EvaluateResult
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the error.
            /// </summary>
            /// <value>
            /// The error.
            /// </value>
            public string Error { get; set; }

            /// <summary>
            /// Gets or sets the compiled expression.
            /// </summary>
            /// <value>
            /// The compiled expression.
            /// </value>
            public IExpression CompiledExpression { get; set; }
        }
    }
}