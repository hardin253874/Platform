// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Utc;
using System.Linq;
using System.Text;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Core;

namespace EDC.SoftwarePlatform.Activities
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Given a text expression and runtimeArgs, evaluate the expression. 
        /// Expressions must be compatible with DataColumn.Expression. the result is cast to T
        /// </summary>
        public static object EvaluateExpression(this WfExpression expression, IRunState run)
        {
            
            // Evaluate
            try
            {
                var knownEntities = new Dictionary<string, Resource>();
                using (new SecurityBypassContext())
                {
                    // we need to fetch the known entities in a bypass so that the expression 
                    // evaluation will trigger a security error rather than a missing entity.
                    foreach (var ke in expression.WfExpressionKnownEntities)
                    {
                        if (ke.ReferencedEntity != null && !string.IsNullOrEmpty(ke.Name))
                        {
                            if (knownEntities.ContainsKey(ke.Name))
                            {
                                knownEntities[ke.Name] = ke.ReferencedEntity;
                            }
                            else
                            {
                                knownEntities.Add(ke.Name, ke.ReferencedEntity);
                            }
                        }
                    }                    
                }

                var result = EvaluateExpressionImpl(expression, run, knownEntities);

                return result;
            }
            catch (ParseException ex)
            {
                throw new WorkflowExpressionEvaluationException(ex.ShortMessage, ex.ToString(), ex);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();

                // Format runtimeArgs
                sb.AppendFormat("Error: {0}\n", ex.Message);
                sb.AppendFormat("Expression: ** {0} **\n", expression.ExpressionString ?? "null");
                sb.AppendFormat("Exception: {0}\n", ex.GetType().Name);
                sb.AppendFormat("Expression ID: {0}\n", expression.Id);
                sb.AppendFormat("isTemplate: {0}\n", expression.IsTemplateString == null ? "null" : expression.IsTemplateString.ToString());
                sb.AppendFormat("target: {0}\n", expression.ArgumentToPopulate != null ? expression.ArgumentToPopulate.Id.ToString() : "null");
                sb.AppendFormat("parameters: \n");
                run.PrettyPrint(sb);

                var activity = expression.ExpressionInActivity;
                var workflowRunId = activity != null && activity.ContainingWorkflow != null ? activity.ContainingWorkflow.Id : 0;

                var message = ex is PlatformSecurityException ? $"Expression failed with a security violation" : $"Expression failed during evaluation";

                throw new WorkflowExpressionEvaluationException($"{message}: '{expression.ExpressionString ?? "Null"}'", sb.ToString(), ex);
            }
        }

        enum ParseState
        {
            PlainText,
            Script,
            OpenBrace,
            CloseBrace
        }

        /// <summary>
        /// Take an expression defined with '{{varname}}' delimiters and turn it into an expressions string; 
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        internal static string ConvertTemplateToExpressionString(string template)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("'");

            ParseState state = ParseState.PlainText;

            // turn   "{{script}}" into "' + join(script) + '" 

            foreach (char ch in template ?? "")
            {
                if (state == ParseState.CloseBrace)
                {
                    if (ch == '}')
                    {
                        state = ParseState.PlainText;
                        sb.Append(") + '");                            
                        continue;
                    }
                    else
                    {
                        sb.Append("}");
                        state = ParseState.Script;
                    }
                }
                if (state == ParseState.OpenBrace)
                {
                    if (ch == '{')
                    {
                        state = ParseState.Script;
                        sb.Append("' + join(");                            
                        continue;
                    }
                    else
                    {
                        sb.Append("{");
                        state = ParseState.PlainText;
                    }
                }
                if (state == ParseState.Script)
                {
                    if (ch == '}')
                    {
                        state = ParseState.CloseBrace;
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                if (state == ParseState.PlainText)
                {
                    if (ch == '{')
                    {
                        state = ParseState.OpenBrace;
                    }
                    else if (ch == '\r')
                    {
                    }
                    else if (ch == '\'')
                    {
                        sb.Append(@"''");
                    }
                    else if (ch == '\n')
                    {
                        sb.Append(@"\n");
                    }
                    else if (ch == '\\')
                    {
                        sb.Append(@"\\");
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }
            // Finish off
            switch (state)
            {
                case ParseState.OpenBrace:
                    sb.Append("{'");
                    break;
                case ParseState.PlainText:
                    sb.Append("'");
                    break;
            }

            string result = sb.ToString();
            if (result.StartsWith("'' + "))
                result = result.Substring("'' + ".Length);
            if (result.EndsWith(" + ''"))
                result = result.Substring(0, result.Length - " + ''".Length);

            return result;
        }


        /// <summary>
        /// Given a text expression and runtimeArgs, evaluate the expression. 
        /// Expressions must be compatible with DataColumn.Expression. the result is cast to T
        /// </summary>

        internal static object EvaluateExpressionImpl(WfExpression wfExpression, IRunState context, IDictionary<string, Resource> knownEntities)
        {
            var expression = context.Metadata.CompiledExpressionCache[wfExpression.Id];

            if (expression == null)
            {
                return GetKnownEntities(wfExpression, knownEntities);
            }
            else
            {
                // Evaluate the expression
                var eSettings = new EvaluationSettings();
                eSettings.TimeZoneName = TimeZoneHelper.SydneyTimeZoneName;     // TODO: Workflow engine to provide a timezone to run in. Required for date-time functions.

                eSettings.ParameterResolver = paramName => context.ResolveParameterValue(paramName, knownEntities);           // used for runtime parameters

                object result = Factory.ExpressionRunner.Run(expression, eSettings).Value;
                return CoerceToListIfNeeded(wfExpression, result);
            }
        }

        static object GetKnownEntities(WfExpression wfExpression, IDictionary<string, Resource> knownEntities)
        {
            // Could be a naked references to entities
            var knownEntitiesCount = knownEntities.Count();

            //NOTE: We may want to cache this
            if (wfExpression.ArgumentToPopulate.Is<ResourceArgument>())
            {
                return knownEntities.Any() ? knownEntities.First().Value : null;
            }
            else if (wfExpression.ArgumentToPopulate.Is<ResourceListArgument>())
            {
                return knownEntities.Values;
            }
            else
                return null;    // it's not been populated by anything
        }

        static object CoerceToListIfNeeded(WfExpression wfExpression, object result)
        {
            if (result is IEnumerable<IEntity>)
            {
                var list = (IEnumerable<IEntity>) result;

                if (list.Count() == 1 && list.First() == null) // fix for a bug where an expression returning an empty list returns a list with a null entry
                    return Enumerable.Empty<IEntity>(); 
                else
                    return result;      // It's already ok
            }

            if (!wfExpression.ArgumentToPopulate.Is<ResourceListArgument>())
                return result;      // Nothing to do

            if (result == null)
                return Enumerable.Empty<IEntity>();
            else
                return ((IEntity) result).ToEnumerable<IEntity>();

        }

        /// <summary>
        /// Get a string representation of the expression resource that is suitable for display.
        /// </summary>
        /// <param name="exp">The expression.</param>
        /// <returns>A string</returns>
        public static string GetExpressionDisplayString(this WfExpression exp)
        {
            return exp.ExpressionString ?? "<null>";
        }

    }

    /// <summary>
    /// Thrown when an expression fails to evaluate
    /// </summary>
    public class WorkflowExpressionEvaluationException: WorkflowRunException
    {
        string _fullMessage;

        public WorkflowExpressionEvaluationException(string shortMessage, string additionalDetails, Exception innerException): base(shortMessage, innerException)
        {
            _fullMessage = string.Format("{0}\n{1}", shortMessage, additionalDetails);
        }

        public override string ToString()
        {
            return _fullMessage;
        }
    }  
}
