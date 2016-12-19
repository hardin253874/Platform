// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace EDC.ReadiNow.Metadata.Solutions.DataUpgrade
{
    public static class WorkflowUpgradeHelper
    {
        public static Workflow AddUpdateExpressionToArgument(this Workflow wf, WfActivity activity, string argumentName, string expressionString, bool isTemplate = false)
        {

            var destination = activity.GetInputArgument(argumentName);
            if (destination == null)
                throw new ArgumentException();
            
            return AddUpdateExpressionToArgument(wf, activity, destination, expressionString, isTemplate);
        }

        public static Workflow AddUpdateExpressionToArgument(this Workflow wf, WfActivity activity, ActivityArgument arg, string expressionString, bool isTemplate)
        {
            if (isTemplate)
            {
                expressionString = ConvertTemplateToExpressionString(expressionString);
            }

            if (expressionString == null)
            {
                throw new ArgumentNullException("expressionString");
            }

            var exp = activity.ExpressionMap.FirstOrDefault(x => x.ArgumentToPopulate?.Id == arg.Id);
            if (exp == null)
            {
                var newExp = new WfExpression()
                {
                    ExpressionString = expressionString,
                    ArgumentToPopulate = arg,
                    ExpressionInActivity = activity,
                    IsTemplateString = false
                };

                activity.ExpressionMap.Add(newExp.As<WfExpression>());
            }
            else
            {
                var wrExp = exp.AsWritable<WfExpression>();
                wrExp.ExpressionString = expressionString;
                wrExp.Save();
            }

            return wf;
        }



        public static Workflow AddUpdateEntityExpressionToInputArgument(this Workflow wf, WfActivity activity, string argumentName, EntityRef entityRef)
        {
            var destination = activity.GetInputArgument(argumentName);
            if (destination == null)
                throw new ArgumentException();

            return AddUpdateEntityExpression(wf, activity, destination, entityRef);
        }

        public static Workflow AddUpdateEntityExpression(this Workflow wf, WfActivity activity, ActivityArgument arg, EntityRef entityRef)
        {
            var exp = activity.ExpressionMap.FirstOrDefault(x => x.ArgumentToPopulate?.Id == arg.Id);
            var r = Entity.Get(entityRef).As<Resource>();

            var expName = r.Id.ToString();
            if (!string.IsNullOrEmpty(r.Name))
                expName = r.Name;

            if (exp == null)
            {
                exp = new WfExpression { ArgumentToPopulate = arg, ExpressionInActivity = activity, ExpressionString = string.Format("[{0}]", expName) };
                exp.WfExpressionKnownEntities.Add(new NamedReference { Name = expName, ReferencedEntity = r });
                activity.ExpressionMap.Add(exp);
            }
            else
            {
                var wrExp = exp.AsWritable<WfExpression>();
                wrExp.ExpressionString = string.Format("[{0}]", expName);
                wrExp.WfExpressionKnownEntities.Clear();
                wrExp.WfExpressionKnownEntities.Add(new NamedReference { Name = expName, ReferencedEntity = r });
                wrExp.Save();
            }

            return wf;
        }


        public static ActivityArgument GetInputArgument(this WfActivity activity, string implementationName)
        {
            return activity.GetInputArguments().FirstOrDefault(a => a.Name == implementationName);     // note that his code assumes the first type is an activity type
        }

        public static IEnumerable<ActivityArgument> GetInputArguments(this WfActivity activity)
        {
            // for a workflow the input arguments are on the instance, for an activity they are on the type
            var typeArguments = activity.IsOfType.Select(t => t.As<ActivityType>()).First(t => t != null).InputArguments;

            var entityWithArgsAndExits = activity.As<EntityWithArgsAndExits>();
            if (entityWithArgsAndExits != null)
                return entityWithArgsAndExits.InputArguments.Union(typeArguments);
            else
                return typeArguments; // note that his code assumes the first type is an activity type
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
    }
}
