// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EDC.Database;
using EDC.Database.Types;
using ReadiNow.Expressions.Parser;
using ReadiNow.Expressions.Tree;
using ReadiNow.Expressions.Tree.Nodes;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using Irony.Parsing;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Core;
using ReadiNow.Expressions.NameResolver;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Performs all static evaluation of the parse tree.
    /// This includes:
    /// - verifying types
    /// - performing implicit casts
    /// - resolving argument names
    /// - validating entity names, etc
    /// </summary>
    public class StaticBuilder
    {
        /// <summary>
        /// Various settings passed in from the caller into the expression engine.
        /// </summary>
        public BuilderSettings Settings { get; set; }

        internal CompileContext Context { get; set; }

        /// <summary>
        /// Repository used to load information about schema entities.
        /// </summary>
        private IEntityRepository EntityRepository { get; set; }

        /// <summary>
        /// Service used to resolve identifiers.
        /// </summary>
        private IScriptNameResolver ScriptNameResolver { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StaticBuilder(IEntityRepository entityRepository, IScriptNameResolver scriptNameResolver)
        {
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            if (scriptNameResolver == null)
                throw new ArgumentNullException("scriptNameResolver");

            EntityRepository = entityRepository;
            ScriptNameResolver = scriptNameResolver;
        }

        /// <summary>
        /// Main entry point for the static builder.
        /// </summary>
        /// <param name="parseExpressionNode">The top expression parse node. Note that the 'expression' node only appears at the top of an expression tree.</param>
        /// <returns>An expression object, which captures the expression tree.</returns>
        public Expression CompileTree(ParseTreeNode parseExpressionNode)
        {
            // Validate starting conditions
            if (Settings == null)
                throw new InvalidOperationException("Settings property must be called prior to calling CompileTree.");
            if (parseExpressionNode.Term.Name != Terms.Expression)
                throw new ArgumentException("Expected expression node.");

            // Set up context
            Context = new CompileContext
            {
                Settings = Settings,
                Variables = new VariableBag(),
                Parameters = new ParameterBag(Settings)
            };

            // Set up root source
            EntityNode rootSource;
            if (Settings.RootContextType == null)
            {
                Settings.RootContextType = new ExprType(DataType.None);
            }
            if (Settings.RootContextType.Type != DataType.None)
            {
                rootSource = new GetRootContextEntityNode
                {
                    ResultType = Settings.RootContextType
                };
            }
            else
            {
                rootSource = new SingleRowSourceNode();
            }
            Context.ContextExpression = rootSource;
            Context.ParentNodeExpression = rootSource.ChildContainer;

            // Perform the static analysis
            ParseTreeNode rootParseNode = parseExpressionNode.ChildNodes[0];
            ExpressionNode rootExprNode = Compile_Expression(rootParseNode);

            // Perform final cast
            if (Settings.ExpectedResultType != null)
            {
                var assignable = ExprTypeCastHelper.IsAssignable(rootExprNode.ResultType, Settings.ExpectedResultType, CastType.Implicit);
                if (assignable == Assignable.No)
                    throw new ParseException(string.Format("Result was of type {0} but needed to be {1}.", rootExprNode.ResultType, Settings.ExpectedResultType));
                if (assignable == Assignable.RequiresCast)
                    rootExprNode = ExprTypeCastHelper.Cast( rootExprNode, Settings.ExpectedResultType, Context );
            }

            // Assign default decimal places
            rootExprNode.ResultType.DecimalPlaces = Context.GetDecimalPlaces(rootExprNode.ResultType);

            // Package results
            var result = new Expression
            {
                Root = rootExprNode,
                ListRoot = rootSource
            };
            return result;
        }


        /// <summary>
        /// Process an expression or sub expression of whatever type.
        /// Main re-entry point for recursion.
        /// </summary>
        /// <param name="node">The current parse node. Something under expressionImpl, but not expression itself.</param>
        /// <param name="allowPartialParameter">If true, a partial parameter may be returned, otherwise it must be resolved.</param>
        /// <returns>An expression node that represents the static information about this calculation.</returns>
        public ExpressionNode Compile_Expression(ParseTreeNode node, bool allowPartialParameter = false)
        {
            string termName = node.Term.Name;

            ExpressionNode result;

            switch (termName)
            {
                case Terms.SelectExpression:
                    result = Compile_Expression(node.ChildNodes[1]);    // no-op
                    break;

                case Terms.LetClause:
                    result = Compile_LetClause(node);
                    break;

                case Terms.WhereExpression:
                    result = Compile_WhereExpression(node);
                    break;

                case Terms.OrderByExpression:
                    result = Compile_OrderByExpression(node);
                    break;

                case Terms.StringLiteral:
                    result = Compile_StringLiteral(node);
                    break;

                case Terms.DateTimeLiteral:
                    result = Compile_DateTimeLiteral(node);
                    break;

                case Terms.NumberLiteral:
                    result = Compile_NumberLiteral(node);
                    break;

                case Terms.UnaryExpression:
                    result = Compile_UnaryExpression(node);
                    break;

                case Terms.Parameter:
                    result = Compile_Parameter(node);
                    break;

                case Terms.BinaryExpression:
                    result = Compile_BinaryExpression(node);
                    break;

                case Terms.MemberAccess:
                    result = Compile_MemberAccess(node);
                    break;

                case Terms.FunctionExpression:
                    result = Compile_FunctionCall(node);
                    break;

                case Keywords.True:
                case Keywords.False:
                    result = Compile_BoolLiteral(node);
                    break;

                case Keywords.Null:
                    result = Compile_NullLiteral(node);
                    break;

                default:
                    throw new InvalidOperationException(termName);
            }

            if (!allowPartialParameter)
            {
                var resultAsPartial = result as PartialParameterNode;
                if (resultAsPartial != null)
                {
                    result = ResolvePartialParameter(resultAsPartial, node);
                }
            }

            result.OnStaticValidation(Settings, node);

            return result;
        }


        /// <summary>
        /// Process a non-query expression.
        /// </summary>
        public ExpressionNode Compile_LetClause(ParseTreeNode letClause)
        {
            // let x = y appliesToExpression
            if (letClause.ChildNodes.Count != 5)
                throw new InvalidOperationException("Expected 5 child nodes");

            Token identifier = letClause.ChildNodes[1].Token;

            // Parse expression assigned to variable
            ParseTreeNode exprNode = letClause.ChildNodes [ 3 ];

            var prevParentExpr = Context.ParentNodeExpression;
            var variableChildContainer = new ChildContainer( ); // collects things that the variable expression wants to register
            Context.ParentNodeExpression = variableChildContainer;
            ExpressionNode assignedValue = Compile_Expression( exprNode );
            Context.ParentNodeExpression = prevParentExpr;

            // Make variable available as the usage context is evaluated
            Context.PushScope();
            Context.Variables.SetVariable(identifier, assignedValue, variableChildContainer );

            ExpressionNode appliesToExpression = Compile_Expression(letClause.ChildNodes[4]);

            Context.PopScope();

            return appliesToExpression;
        }


        /// <summary>
        /// Process a non-query expression.
        /// </summary>
        public ExpressionNode Compile_WhereExpression(ParseTreeNode whereNode)
        {
            // expr where condition
            if (whereNode.ChildNodes.Count != 3)
                throw new InvalidOperationException("Expected 3 child nodes");

            var result = new WhereNode();

            Context.PushScope();
            Context.ParentNodeExpression = result.ChildContainer;

            // Determine LHS
            ParseTreeNode exprNode = whereNode.ChildNodes[0];
            ExpressionNode applyTo = Compile_Expression(exprNode);
            
            Context.ContextExpression = applyTo.DetermineContextNode();
            Context.ParentNodeExpression = Context.ContextExpression.ChildContainer;

            // Determine RHS
            ExpressionNode condition = Compile_Expression(whereNode.ChildNodes[2]);
            result.Right = ExprTypeCastHelper.Cast(condition, ExprType.Bool, Context);

            Context.PopScope();

            result.Left = applyTo;
            result.ResultType = applyTo.ResultType.Clone();
            Context.ParentNodeExpression.RegisterChild(result);

            return result;
        }


        /// <summary>
        /// Process a non-query expression.
        /// </summary>
        public ExpressionNode Compile_OrderByExpression(ParseTreeNode orderByNode)
        {
            // source 'order' 'by' terms
            if (orderByNode.ChildNodes.Count != 4)
                throw new InvalidOperationException("Expected 4 child nodes");

            if (Settings.ScriptHost == ScriptHostType.Report)
                throw ParseExceptionHelper.New("'order by' is not available in reports.", orderByNode.ChildNodes[1]);

            var termList = orderByNode.ChildNodes[3].ChildNodes;
            List<OrderByTerm> orderTerms = new List<OrderByTerm>();

            var result = new OrderByNode();
            Context.PushScope();
            Context.ParentNodeExpression = result.ChildContainer;

            ParseTreeNode exprNode = orderByNode.ChildNodes[0];
            ExpressionNode applyTo = Compile_Expression(exprNode);

            if (applyTo.ResultType.Type != DataType.Entity)
                throw ParseExceptionHelper.New("'order by' can only be applied to resource lists.", orderByNode.ChildNodes[1]);
            
            Context.PushScope();
            Context.ContextExpression = applyTo.DetermineContextNode();
            
            result.OrderTerms = orderTerms;
            result.Argument = applyTo;
            result.ResultType = applyTo.ResultType.Clone();
            

            foreach (ParseTreeNode orderByTerm in termList)
            {
                bool desc = orderByTerm.ChildNodes.Count > 1 && orderByTerm.ChildNodes[1].Term.Name == "desc";
                var orderExpr = Compile_Expression(orderByTerm.ChildNodes[0]);

                // Cast to string, if an entity. Otherwise treat everything as sortable. Hmm
                if (orderExpr.ResultType.Type == DataType.Entity)
                {
                    orderExpr = ExprTypeCastHelper.Cast( orderExpr, new ExprType( DataType.String ), Context );
                }

                var term = new OrderByTerm
                {
                    Direction = desc ? Direction.Reverse : Direction.Forward,
                    Expression = orderExpr
                };
                orderTerms.Add(term);
            }

            Context.PopScope();
            Context.PopScope();
            Context.ParentNodeExpression.RegisterChild(result);
            
            return result;
        }


        /// <summary>
        /// Process a true/false literal parse node.
        /// </summary>
        private ExpressionNode Compile_BoolLiteral(ParseTreeNode node)
        {
            return new ConstantNode
            {
                Value = Keywords.Equals(node.Term.Name, Keywords.True),
                ResultType = new ExprType { Type = DataType.Bool, Constant = true }
            };
        }


        /// <summary>
        /// Process a null literal parse node.
        /// </summary>
        private ExpressionNode Compile_NullLiteral(ParseTreeNode node)
        {
            return new ConstantNode
            {
                ResultType = new ExprType { Type = DataType.None, Constant = true }
            };
        }


        /// <summary>
        /// Process a string literal parse node.
        /// </summary>
        private ExpressionNode Compile_StringLiteral(ParseTreeNode node)
        {
            string value = (string)node.Token.Value;
            value = value.Replace("\r\n", "\n");
            value = value.Replace("\n", "\r\n");
            return new ConstantNode
            {
                Value = value,
                ResultType = new ExprType { Type = DataType.String, Constant = true }
            };
        }


        /// <summary>
        /// Process a string literal parse node.
        /// </summary>
        private ExpressionNode Compile_DateTimeLiteral(ParseTreeNode node)
        {
            // Supported:
            // date, time, date time (separated by space)
            // YYYY-MM-DD
            // DD/MM/YY
            // DD/MM/YYYY
            // hh:mm
            // hh:mm:ss
            // HH:mm AM/PM
            // HH:mm:ss AM/PM

            string value = (string)node.Token.Value;
            bool isDateTime = value.Contains(':');

            DateTime result;
            bool isValid = DateTime.TryParse(value,
                new CultureInfo("en-AU"),   // this is what we've decided on for ambiguous formats .. but users should use ISO8601  I.e. we accept yyyy-MM-dd or dd/MM/yyyyy, with or without times
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal   // will give us the raw value, with a UTC marker. more reliable, even though we're going to treat it as local
                | DateTimeStyles.NoCurrentDateDefault,  // causes time-only strings to have a date of 1/1/1
                out result);

            if (!isValid)
            {
                throw ParseExceptionHelper.New("Invalid date/time format: " + value, node);
            }

            if (result.Year == 1)
            {
                // Time literal
                return new ConstantNode
                {
                    Value = TimeType.NewTime(result.TimeOfDay),
                    ResultType = new ExprType { Type = DataType.Time, Constant = true }
                };
            }

            // Date literal
            // Note: we're storing the raw time (localtime) even for DateTime (because we don't have timezone info at parse time).
            // It will get converted to UTC at runtime.
            return new ConstantNode
            {
                Value = result,
                ResultType = new ExprType {
                    Type = isDateTime ? DataType.DateTime : DataType.Date,
                    Constant = true }
            };
        }


        /// <summary>
        /// Process a numeric literal parse node.
        /// </summary>
        private ExpressionNode Compile_NumberLiteral(ParseTreeNode node)
        {
            object value = node.Token.Value;

            if (value is int)
            {
                return new ConstantNode
                {
                    Value = value,
                    ResultType = new ExprType { Type = DataType.Int32, Constant = true }
                };
            }
            if (value is double)
            {
                int decimalPlaces = 3;
                try
                {
                    string formatted = node.Token.Text;
                    int dpPos = formatted.IndexOf('.');
                    if (dpPos >= 0)
                    {
                        decimalPlaces = formatted.Length - dpPos - 1;
                    }
                }
                catch { }

                return new ConstantNode
                {
                    Value = (decimal)(double)value, //recast to decimal
                    ResultType = new ExprType
                    {
                        Type = DataType.Decimal,
                        Constant = true,
                        DecimalPlaces = decimalPlaces
                    }
                };
            }
            throw new InvalidOperationException("Cannot handle numeric literal type: " + value.GetType().Name);
        }


        /// <summary>
        /// Process a string literal parse node.
        /// </summary>
        private ExpressionNode Compile_Parameter(ParseTreeNode node)
        {
            if (node.ChildNodes.Count != 2)
                throw new InvalidOperationException();

            var result = Compile_ParameterImpl(node.ChildNodes[1]);
            if (result == null)
                throw ParseExceptionHelper.New(string.Format("Parameter @{0} was not recognised.", node.ChildNodes[1].Token.Value), node.ChildNodes[0]);
            return result;
        }


        /// <summary>
        /// Process a parameter node.
        /// </summary>
        private ExpressionNode Compile_ParameterImpl(ParseTreeNode node)
        {
            string parameterName = node.Token.Value.ToString();
            return Compile_ParameterImpl(parameterName);
        }


        /// <summary>
        /// Process a string literal parse node.
        /// </summary>
        private ExpressionNode Compile_ParameterImpl(string parameterName, bool allowPartial = true)
        {
            if (allowPartial && Context.Parameters.HasPartial(parameterName))
            {
                return new PartialParameterNode { PartialName = parameterName };
            }

            ExprType paramType = ResolveParameter(parameterName);
            if (paramType == null)
                return null;

            if (paramType.Type == DataType.Entity)
            {
                var result = new EntityParameterNode
                {
                    ParameterName = parameterName,
                    ResultType = paramType
                };
                result = Context.ParentNodeExpression.RegisterOrReuseChild(result);
                return result;
            }

            return new ParameterNode
            {
                ParameterName = parameterName,
                ResultType = paramType
            };
        }


        /// <summary>
        /// Resolves a parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public ExprType ResolveParameter(string parameterName)
        {
            if (Settings.StaticParameterResolver == null)
                return null;

            var result = Settings.StaticParameterResolver(parameterName);

            return result;
        }


        /// <summary>
        /// Converts a partial parameter node into a proper parameter node.
        /// </summary>
        private ExpressionNode ResolvePartialParameter(PartialParameterNode partialNode, ParseTreeNode node)
        {
            string parameterName = partialNode.PartialName;
            var result = Compile_ParameterImpl(parameterName, false);
            if (result == null)
                throw ParseExceptionHelper.New(string.Format("Parameter {0} was not recognised.", parameterName), node);
            return result;
        }


        /// <summary>
        /// Converts a partial parameter node into a proper parameter node or a partial name.
        /// </summary>
        private ExpressionNode ResolvePartialParameter(PartialParameterNode partialNode, ParseTreeNode node, ParseTreeNode memberAccessNode, out bool memberConsumed)
        {
            // Get the member name
            if (memberAccessNode.Term.Name != Terms.Identifier)
                throw new Exception("Expected identifier.");
            string memberName = (string)memberAccessNode.Token.Value;

            string partialName = partialNode.PartialName;
            string proposedName = partialName + "." + memberName;

            ExpressionNode result = Compile_ParameterImpl(proposedName);
            memberConsumed = result != null;
            if (!memberConsumed)
                result = ResolvePartialParameter(partialNode, node);
            return result;
        }


        /// <summary>
        /// Process a unary operator parse node.
        /// </summary>
        private ExpressionNode Compile_UnaryExpression(ParseTreeNode node)
        {
            if (node.ChildNodes.Count != 2)
                throw new InvalidOperationException("Expected 2 child nodes");
            
            string operatorToken = node.ChildNodes[0].Token.ValueString.ToLower();

            // Get the argument
            ParseTreeNode argParseNode = node.ChildNodes[1];
            ExpressionNode argCalcNode = Compile_Expression(argParseNode);

            // 'Select' is a nice placebo.
            if (operatorToken == "select")
                return argCalcNode;

            var result = MatchSignature(node, operatorToken, new[] { argCalcNode }, true);
            return result;
        }


        /// <summary>
        /// Process a binary operator parse node.
        /// </summary>
        private ExpressionNode Compile_BinaryExpression(ParseTreeNode node)
        {
            if (node.ChildNodes.Count != 3)
                throw new InvalidOperationException("Expected 3 child nodes");

            var operatorNode = node.ChildNodes[1];
            string operatorToken = operatorNode.Token.ValueString.ToLower();

            // Get the argument
            ParseTreeNode leftParseNode = node.ChildNodes[0];
            ParseTreeNode rightParseNode = node.ChildNodes[2];
            ExpressionNode leftCalcNode = Compile_Expression(leftParseNode);
            ExpressionNode rightCalcNode = Compile_Expression(rightParseNode);

            var result = MatchSignature(operatorNode, operatorToken, new[] { leftCalcNode, rightCalcNode }, true);
            return result;
        }


        ///// <summary>
        ///// Process a binary operator parse node.
        ///// </summary>
        //private ExpressionNode Compile_WhereExpression(ParseTreeNode node)
        //{
        //    if (node.ChildNodes.Count != 3)
        //        throw new InvalidOperationException("Expected 3 child nodes");

        //    // Get the argument
        //    ParseTreeNode leftParseNode = node.ChildNodes[0];
        //    ParseTreeNode rightParseNode = node.ChildNodes[2];
        //    ExpressionNode leftCalcNode = Compile_Expression(leftParseNode);
        //    ExpressionNode rightCalcNode = Compile_Expression(rightParseNode);

        //    var result = MatchSignature("where", new[] { leftCalcNode, rightCalcNode }, true);
        //    return result;
        //}


        /// <summary>
        /// Process a call to a function.
        /// </summary>
        /// <param name="node">A function-expression parse node.</param>
        /// <returns></returns>
        private ExpressionNode Compile_FunctionCall(ParseTreeNode node)
        {
            // 2 children for static access. E.g. b(args)
            // 3 children for member access. E.g. a.b(args)
            // Last child is the argument list. Second last is function name. First is context, if provided.

            // Get parse nodes
            int childCount = node.ChildNodes.Count;
            ParseTreeNode functionNameNode;
            ParseTreeNode argListParseNode;
            ParseTreeNode contextNode = null;
            bool firstArgIsDatePart = false;

            if (childCount == 2)
            {
                functionNameNode = node.ChildNodes[0];
                argListParseNode = node.ChildNodes[1];
            }
            else if (childCount == 3)
            {
                contextNode = node.ChildNodes[0];
                functionNameNode = node.ChildNodes[1];
                argListParseNode = node.ChildNodes[2];
            }
            else
            {
                throw new InvalidOperationException("Expected 2 or 3 child nodes");
            }


            // Determine function
            string functionName = functionNameNode.Token.ValueString.ToLowerInvariant();

            // Special handing for 'convert' function
            if (functionName == "convert")
                return Compile_ConvertFunctionCall(argListParseNode, node);
            if (functionName == "all")
                return Compile_AllFunctionCall(argListParseNode, node);
            if (functionName == "resource")
                return Compile_ResourceFunctionCall(argListParseNode, node);
            if (functionName == "datediff" || functionName == "dateadd" || functionName == "datename")
                firstArgIsDatePart = true;

            // Try and catch entity node binds
            Context.PushScope();
            var container = new ChildContainer();
            Context.ParentNodeExpression = container;

            // Get list of argument, including the context node, if applicable.
            IEnumerable<ParseTreeNode> allArgParseNodes = argListParseNode.ChildNodes;
            if (firstArgIsDatePart)
            {
                allArgParseNodes = allArgParseNodes.Skip(1);
            }
            if (contextNode != null)
            {
                // Preappend context node
                allArgParseNodes = contextNode.ToEnumerable().Concat(allArgParseNodes);
            }

            // Compile arguments context node.
            ExpressionNode[] argumentExpressions = allArgParseNodes.Select(n => Compile_Expression(n)).ToArray();

            // Find a matching function signature
            var result = MatchSignature(functionNameNode, functionName, argumentExpressions, false);

            Context.PopScope();

            if (firstArgIsDatePart && result is IDateTimePartFunction)
            {
                DateTimeParts part = GetDateTimePart(argListParseNode.ChildNodes[0]);
                // Because as it turns out, SQL just treats them the same as 'day' anyway.
                if ((part == DateTimeParts.Weekday || part == DateTimeParts.DayOfYear) && (functionName == "datediff" || functionName == "dateadd"))
                    throw ParseExceptionHelper.New(string.Format("'{0}' is not a valid date/time part for {1}.", part, functionName), node);
                ((IDateTimePartFunction)result).DateTimePart = part;
            }
            if (result is EntityNode)
            {
                Context.ParentNodeExpression.RegisterChild((EntityNode)result);
            }
            if (result is AggregateNode)
            {
                ((AggregateNode)result).ChildContainer = container;                
            }
            else
            {
                foreach (var child in container.ChildEntityNodes)
                {
                    Context.ParentNodeExpression.RegisterChild(child);
                }
            }

            return result;
        }

        /// <summary>
        /// Extract a date/time part.
        /// </summary>
        private DateTimeParts GetDateTimePart(ParseTreeNode node)
        {
            if (node.Term.Name != Terms.MemberAccess)
                throw ParseExceptionHelper.New("Expected a date/time part.", node);
            
            var child = node.ChildNodes[0];
            if (child.Term.Name != Terms.Identifier)
                throw ParseExceptionHelper.New("Expected a date/time part.", node);

            string partName = child.Token.ValueString;
            DateTimeParts result;
            if (!Enum.TryParse(partName, true, out result))
                throw ParseExceptionHelper.New(string.Format("'{0}' was not a recognised date/time part.", partName), node);

            return result;
        }


        /// <summary>
        /// Process the 'all' function.
        /// </summary>
        /// <param name="argumentList">The parse node that holds the list of arguments to the convert function.</param>
        /// <param name="functionNode"></param>
        private ExpressionNode Compile_AllFunctionCall(ParseTreeNode argumentList, ParseTreeNode functionNode)
        {
            if (Settings.ScriptHost == ScriptHostType.Report)
                throw ParseExceptionHelper.New("The 'all' function is not available in reports.", functionNode);

            if (argumentList.ChildNodes.Count != 1)
                throw ParseExceptionHelper.New("Wrong number of values passed to 'all' function.", functionNode);

            // Look up type
            string typeName = GetKeyword(argumentList.ChildNodes[0]);
            if (typeName == null)
                throw ParseExceptionHelper.New("Expected a definition name.", argumentList.ChildNodes[0]);

            long typeId = ScriptNameResolver.GetTypeByName(typeName);
            if (typeId == 0)
                throw ParseExceptionHelper.New("Could not find a definition called '" + typeName + "'.", functionNode.ChildNodes[1]);

            var result = new AllInstancesNode
            {
                Argument = CreateIdentifierLiteral(typeId),
                ResultType = ExprTypeHelper.EntityOfType(ToEntityRef(typeId))
            };
            result.ResultType.IsList = true;
            Context.ParentNodeExpression.RegisterChild(result);
            return result;
        }


        /// <summary>
        /// Process an explicit cast using the convert(targetType, expr) function.
        /// </summary>
        /// <param name="argumentList">The parse node that holds the list of arguments to the convert function.</param>
        /// <param name="functionNode"></param>
        private ExpressionNode Compile_ResourceFunctionCall(ParseTreeNode argumentList, ParseTreeNode functionNode)
        {
            if (Settings.ScriptHost == ScriptHostType.Report)
                throw ParseExceptionHelper.New("The 'resource' function is not available in reports.", functionNode); // TODO: we should support this

            if (argumentList.ChildNodes.Count != 2)
                throw ParseExceptionHelper.New("Wrong number of values passed to 'resource' function.", functionNode);

            string typeName = GetKeyword(argumentList.ChildNodes[0]);
            if (typeName == null)
                throw ParseExceptionHelper.New("Expected a definition name.", argumentList.ChildNodes[0]);

            // Look up type
            long typeId = ScriptNameResolver.GetTypeByName(typeName);
            if (typeId == 0)
                throw ParseExceptionHelper.New("Could not find a definition called '" + typeName + "'.", functionNode.ChildNodes[1]);
            EntityType type = EntityRepository.Get<EntityType>(typeId);

            // Attempt to treat the second argument as a literal
            var nameArg = argumentList.ChildNodes[1];
            string instanceName = GetKeyword(nameArg);

            if (instanceName != null)
            {
               
                // Look up instance
                IEntity instance = ScriptNameResolver.GetInstance(instanceName, type.Id);
                if (instance == null)
                    throw ParseExceptionHelper.New("Could not find a '" + typeName + "' called '" + instanceName + "'.", functionNode.ChildNodes[1].ChildNodes[1]);

                var entityRef = ToEntityRef(instance.Id);
                var result = new ResourceInstanceNode
                {
                    Instance = entityRef,
                    ResultType = ExprTypeHelper.EntityOfType(new EntityRef(typeId))
                };
                Context.ParentNodeExpression.RegisterChild(result);
                return result;
            }
            else // we have a dynamic argument
            {
                ExpressionNode nameExpr = Compile_Expression(nameArg);
                
                var result = new ResourceInstanceDynamicNode
                {
                    EntityType = type,
                    NameExpression =  nameExpr,
                    ResultType = ExprTypeHelper.EntityOfType(new EntityRef(typeId))
                };
                Context.ParentNodeExpression.RegisterChild(result);
                return result;
            }

        }


        /// <summary>
        /// Process an explicit cast using the convert(targetType, expr) function.
        /// </summary>
        /// <param name="argumentList">The parse node that holds the list of arguments to the convert function.</param>
        /// <param name="functionNode"></param>
        private ExpressionNode Compile_ConvertFunctionCall(ParseTreeNode argumentList, ParseTreeNode functionNode)
        {
            if (argumentList.ChildNodes.Count != 2)
                throw ParseExceptionHelper.New("Wrong number of values passed to 'convert' function.", functionNode);

            ParseTreeNode targetNode = argumentList.ChildNodes[0];
            string targetTypeName = GetKeyword(targetNode);
            if (targetTypeName == null)
                throw ParseExceptionHelper.New("Expected a type or definition name.", targetNode);

            string targetTypeNameLower = targetTypeName.ToLowerInvariant();
            ParseTreeNode valueNode = argumentList.ChildNodes[1];

            ExpressionNode valueExpr = Compile_Expression(valueNode);

            // Check for cast to explicit type, such as convert(string, someExpr)
            // (note: the cast strings will appear in the parse tree as a member access.. for now)
            DataType castTo = LanguageManager.Instance.GetCastTypeByName(targetTypeNameLower);
            if (targetTypeNameLower == "string")
                castTo = DataType.String;

            ExprType castToType = null;

            if (castTo != DataType.None)
            {
                // Cast to scalar type
                castToType = new ExprType { Type = castTo };
            }
            else
            {
                // Cast entity type
                long castToEntityType = ScriptNameResolver.GetTypeByName(targetTypeName);
                if (castToEntityType != 0)
                {
                    castToType = ExprTypeHelper.EntityOfType(new EntityRef(castToEntityType));
                    castToType.IsList = valueExpr.ResultType.IsList;
                }
            }

            if (castToType != null)
            {
                Assignable assignable = ExprTypeCastHelper.IsAssignable( valueExpr.ResultType, castToType, CastType.Explicit );

                ExpressionNode result;
                if (assignable == Assignable.Yes)
                    result = valueExpr;
                else if (assignable == Assignable.RequiresCast)
                    result = ExprTypeCastHelper.Cast( valueExpr, castToType, Context );
                else
                    throw ParseExceptionHelper.New(string.Format("Cannot convert from {0} to {1}.", valueExpr.ResultType.Type, castTo), targetNode);

                return result;
            }

            throw ParseExceptionHelper.New(string.Format("Cannot convert to {0}. Unrecognised type or definition name.", targetTypeName), targetNode);
        }


        /// <summary>
        /// Processes lookup to a field or relationship.
        /// </summary>
        /// <param name="node">The member-access parse node. First child is the context (if any), last child is the member to access.</param>
        private ExpressionNode Compile_MemberAccess(ParseTreeNode node)
        {
            if (node.ChildNodes.Count < 1 || node.ChildNodes.Count > 2)
                throw new InvalidOperationException("Expected 1 or 2 child nodes");

            // If 1 child, then this is the member (field/relationship) we are accessing, from the root context object.
            // If 3 children, then the first is the object provides the context, and the last is the accessor.

            bool noContext = node.ChildNodes.Count == 1;
            ParseTreeNode fieldParseNode;
            ExprType contextType;
            EntityNode contextNode; // this is the source expression that will be used to source entity data.
            ChildContainer parentNode;  // this is the target expression that any new relationships will be registered against.

            if (noContext)
            {
                contextType = Context.ContextExpression.ResultType ?? Settings.RootContextType;
                fieldParseNode = node.ChildNodes[0];
                contextNode = Context.ContextExpression;
                parentNode = Context.ParentNodeExpression;
            }
            else
            {
                fieldParseNode = node.ChildNodes[1];
                var childNode = Compile_Expression(node.ChildNodes[0], true);

                // Deal with checking for partial parameter
                if (childNode is PartialParameterNode)
                {
                    bool memberConsumed;
                    var childNodeNew = ResolvePartialParameter(childNode as PartialParameterNode, node, fieldParseNode, out memberConsumed);
                    if (memberConsumed)
                        return childNodeNew;
                    childNode = childNodeNew;
                }

                if (!(childNode is EntityNode))
                    throw ParseExceptionHelper.New("Fields and relationships can only be accessed on resources.", fieldParseNode);
                contextNode = (EntityNode)childNode;
                parentNode = contextNode.GetQueryNode().ChildContainer;
                contextType = contextNode.ResultType;
            }

            // Get the member name
            if (fieldParseNode.Term.Name != Terms.Identifier)
                throw new Exception("Expected identifier.");
            string memberName = (string)fieldParseNode.Token.Value;

            // Check if it is a variable
            VariableInfo variableInfo = Context.Variables.GetVariable(fieldParseNode.Token);
            if ( variableInfo != null)
            {
                return Compile_VariableAccess( variableInfo, parentNode );
            }

            // Check that we have a context to operate on
            if (contextType.EntityType == null)
            {
                // Fall back to checking for parameters of that name.
                var fieldAsParameter = Compile_ParameterImpl(fieldParseNode);
                if (fieldAsParameter != null)
                    return fieldAsParameter;
                throw ParseExceptionHelper.New("Attempted to access a field or relationship, when no starting entity-type has been provided.", fieldParseNode, "No context.");
            }
                

            // These are all internal errors
            if (contextType.Type != DataType.Entity)
                throw new Exception("Expected entity context for member access.");  // for now
            if (contextType.EntityType.Id == 0)
                throw new Exception("Context entity type was not set.");

            // Resolve the member (field/relationship)
            EntityType contextTypeEntity = EntityRepository.Get<EntityType>(contextType.EntityType);

            MemberInfo member;

            if ( Settings.ScriptHostIsApi && memberName.StartsWith( "#" ) )
            {
                // Resolve member by ID. E.g. [#-123] .. use positive number for fields and forward relationship, negative number for reverse relationships.
                long memberId;
                if ( !long.TryParse( memberName.Substring( 1 ), out memberId ) )
                {
                    throw ParseExceptionHelper.New( "Expected an ID number to access member by ID.", fieldParseNode );
                }
                long absMemberId = Math.Abs(memberId);

                IEntity memberEntity = EntityRepository.Get(absMemberId);
                if (memberEntity.Is<Relationship>())
                {
                    member = new MemberInfo { MemberType = MemberType.Relationship, MemberId = absMemberId, Direction = memberId > 0 ? Direction.Forward : Direction.Reverse };
                }
                else if (memberEntity.Is<Field>())
                {
                    member = new MemberInfo { MemberType = MemberType.Field, MemberId = memberId };
                }
                else
                {
                    throw ParseExceptionHelper.New( "Member ID was not a field or relationship.", fieldParseNode );
                }
            }
            else
            {
	            using ( new ScriptNameResolverContext( ) )
	            {
		            // Resolve member by name
		            using ( Profiler.Measure( "Script compile: Resolve member name" ) )
		            {
			            member = ScriptNameResolver.GetMemberOfType( memberName, contextTypeEntity.Id, MemberType.Any );
		            }

		            if ( member == null )
		            {
			            // Could not find member with that name. Fall back to checking for parameters of that name. (Do we want to do this?? It's nice for workflow) SH: Not needed for workflow
			            var fieldAsParameter = Compile_ParameterImpl( fieldParseNode );
			            if ( fieldAsParameter != null )
				            return fieldAsParameter;

			            string message;

			            if ( ScriptNameResolverContext.Reason == NullMemberNameReason.Duplicate )
			            {
							message = ParseExceptionHelper.Reformat( string.Format( "The name '{0}' exists multiple times on '{1}'.", memberName, contextTypeEntity.Name ), fieldParseNode );
						}
			            else
			            {
				            message = ParseExceptionHelper.Reformat( string.Format( "The name '{0}' could not be matched on '{1}'.", memberName, contextTypeEntity.Name ), fieldParseNode );
			            }

			            throw new InvalidMemberParseException( contextTypeEntity.Id, memberName, message );
		            }
	            }
            }

            ExpressionNode resultNode;

            if (member.MemberType == MemberType.Field)
            {
                resultNode = Compile_FieldAccess(contextNode, member, fieldParseNode);
            }
            else
            {
                resultNode = Compile_RelationshipAccess(contextType, contextNode, parentNode, member);
            }
            return resultNode;
        }

        private static ExpressionNode Compile_VariableAccess( VariableInfo variableInfo, ChildContainer parentNode )
        {
            // Attach variable child containers
            ExpressionNode variableExpr = variableInfo.Expression;
            ChildContainer variableChildContainer = variableInfo.ChildContainer;
            foreach ( EntityNode childEntityNode in variableChildContainer.ChildEntityNodes )
            {
                if ( childEntityNode.RootForVariableContainer == null )
                    childEntityNode.RootForVariableContainer = variableChildContainer; 
                parentNode.RegisterOrReuseChild( childEntityNode );
            }

            return variableExpr;
        }

        private ExpressionNode Compile_FieldAccess(EntityNode contextNode, MemberInfo member, ParseTreeNode fieldParseNode)
        {
            // Note: fieldParseNode may be null if called from TransformForOrdering

            // Access a field
            Field field = EntityRepository.Get<Field>(member.MemberId);
            DatabaseType dbType = field.ConvertToDatabaseType();
            DataType dataType = DataTypeHelper.FromDatabaseType(dbType);

            // Prevent references to calculated fields
            if (Settings.ScriptHost != ScriptHostType.Evaluate && Factory.CalculatedFieldMetadataProvider.IsCalculatedField(field.Id))
            {
                throw ParseExceptionHelper.New(
                    string.Format("'{0}' is a calculated field. Calculated field cannot be used in other calculations.",
                    fieldParseNode.Token.Value), fieldParseNode);
            }

            // Set up result expression
            var resultNode = new AccessFieldNode
            {
                ResultType = new ExprType { Type = dataType, Constant = false, IsList = contextNode.ResultType.IsList }
            };
            resultNode.SetArgument(0, contextNode);
            resultNode.SetArgument(1, CreateIdentifierLiteral(field.Id));

            // Extract number of decimal places
            DecimalField decimalField = field.As<DecimalField>();
            if (decimalField != null)
            {
                resultNode.ResultType.DecimalPlaces = decimalField.DecimalPlaces;
            }

            return resultNode;
        }

        private ExpressionNode Compile_RelationshipAccess(ExprType contextType, EntityNode contextNode, ChildContainer parentNode, MemberInfo member)
        {
            // Access a relationship
            Relationship relationship = EntityRepository.Get<Relationship>(member.MemberId);
            EntityType targetEnd = member.Direction == Direction.Forward ? relationship.ToType : relationship.FromType;

            if ( targetEnd == null )
                throw new Exception( string.Format( "Relationship '{0}' is missing from/to type.", relationship.Name ) );

            string cardinality = relationship.Cardinality.Alias;
            bool isList = contextType.IsList
                || cardinality == "core:manyToMany"
                || (member.Direction == Direction.Forward && cardinality == "core:oneToMany")
                || (member.Direction == Direction.Reverse && cardinality == "core:manyToOne");

            // Create a relationship node
            var resultNode = new AccessRelationshipNode
            {
                ResultType = new ExprType
                {
                    Type = DataType.Entity,
                    EntityType = new EntityRef(targetEnd.Id),
                    Constant = false,
                    IsList = isList,
                },
                InputType = contextType,
                Direction = member.Direction
            };
            resultNode.SetArgument(0, contextNode);
            resultNode.SetArgument(1, CreateIdentifierLiteral(relationship.Id));

            resultNode = parentNode.RegisterOrReuseChild(resultNode);
            return resultNode;
        }


        /// <summary>
        /// Creates an expression that represents an entity constant.
        /// </summary>
        /// <param name="entityId">The ID of the entity.</param>
        private ExpressionNode CreateIdentifierLiteral(long entityId)
        {
            var entityRef = ToEntityRef(entityId);

            var node = new ConstantEntityNode
            {
                Instance = entityRef,
                ResultType = new ExprType { Type = DataType.Entity, Constant = true }
            };
            return node;
        }


		/// <summary>
		/// Convert an entity to an entity ref.
		/// </summary>
		/// <param name="entityId">The entity identifier.</param>
		/// <returns></returns>
        private EntityRef ToEntityRef(long entityId)
        {
            var entityRef = new EntityRef(entityId);

            if (Settings.TestMode)
            {
                Resource entity = EntityRepository.Get<Resource>(entityId);
                entityRef.Alias = entity.Alias;
            }

            return entityRef;
        }


        /// <summary>
        /// Access a keyword.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetKeyword(ParseTreeNode node)
        {
            ParseTreeNode identifier;
            if (node.Term.Name == Terms.MemberAccess)
            {
                var memberAccess = node;
                identifier = memberAccess.ChildNodes[0];
            }
            else
            {
                identifier = node;
            }
            if (identifier.Token == null)
            {
                return null;
            }
            return identifier.Token.Value.ToString();
        }


        /// <summary>
        /// Searches the language databse for a function or operator that matches the required token name and input types.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="token">Name of the function, or operator token.</param>
        /// <param name="arguments">Expressions for the arguments.</param>
        /// <param name="isOperator">True if we are looking for an operator, false for a function.</param>
        /// <returns>The resulting expression node for the matched operator.</returns>
        private ExpressionNode MatchSignature(ParseTreeNode node, string token, ExpressionNode[] arguments, bool isOperator)
        {
            string lowerToken = token.ToLowerInvariant();
            Function match = null;
            Function implicitMatch = null;
            int bestPriority = int.MaxValue;
            bool argCountOk = false;
            bool tokenFound = false;

            IEnumerable<Function> functions = LanguageManager.Instance.FunctionsByToken[lowerToken];

            // Find something that will match
            foreach (Function fn in functions)
            {
                tokenFound = true;

                if (fn.ParameterTypes.Length != arguments.Length)
                    continue;
                argCountOk = true;

                // Special case test for conditions
                var newArguments = CheckForComparisonsBetweenEntitiesAndStrings(fn, arguments);
                if (newArguments != null)
                {
                    arguments = newArguments;
                    match = fn;
                    break;
                }

                // Check if parameters are of the correct types
                bool isExact = true;
                bool canCast = true;
                int signaturePriority = 0;

                for (int i=0; i<arguments.Length; i++)
                {
                    LanguageExprType parameterType = fn.ParameterTypes[i];
                    ExprType argumentType = arguments[i].ResultType;
                    
                    if (parameterType.TypeGroup == TypeGroup.Custom)
                        continue;

                    int argCastPriority;
                    Assignable res = ExprTypeCastHelper.IsAssignable( argumentType, parameterType, CastType.Implicit, out argCastPriority );

                    if (res == Assignable.No)
                    {
                        canCast = false;
                        isExact = false;
                        break;
                    }
                    if (res == Assignable.RequiresCast)
                    {                        
                        isExact = false;
                        signaturePriority = Math.Max(signaturePriority, argCastPriority);

                        // exclude AllExceptBool because bool will implicitly cast to string.
                        if (parameterType.TypeGroup == TypeGroup.AllExceptBool && argumentType.Type == DataType.Bool) 
                            canCast = false;
                    }
                }

                if (isExact)
                    match = fn;
                else if (canCast)
                {
                    if (signaturePriority < bestPriority)
                    {
                        bestPriority = signaturePriority;
                        implicitMatch = fn; // todo: operator precedence??
                    }
                }
            }

            if (match == null && implicitMatch == null)
            {
                if (!tokenFound)
                    throw ParseExceptionHelper.New(string.Format("'{0}' is not a recognised function.", token), node);
                if (!argCountOk)
                    throw ParseExceptionHelper.New(string.Format("Wrong number of values passed to '{0}' function.", token), node);
                throw ParseExceptionHelper.New(string.Format("Could not find a match of the correct type for '{0}'.", token), node);
            }

            // Implicit cast
            var argList = arguments.ToList();
            if (match == null)
            {
                match = implicitMatch;
                for (int i=0; i<arguments.Length; i++)
                {
                    ExprType parameterType = implicitMatch.ParameterTypes[i];
                    ExprType argumentType = argList[i].ResultType;
                    Assignable res = ExprTypeCastHelper.IsAssignable( argumentType, parameterType, CastType.Implicit );

                    if (res == Assignable.No)
                    {
                        throw new InvalidOperationException();  // assert false
                    }
                    if (res == Assignable.RequiresCast)
                    {
                        ExpressionNode implicitCast = ExprTypeCastHelper.Cast( arguments [ i ], parameterType, Context);
                        argList[i] = implicitCast;
                    }
                }
            }

            // Verify availability
            // Note: if Settings.ScriptHost is ScriptHostType.Any, then the calculation must work on any/every engine.
            if (match.Availability != ScriptHostType.Any && Settings.ScriptHost != match.Availability)
            {
                throw ParseExceptionHelper.New(string.Format("'{0}' is unavailable in {1}.", token, Settings.ScriptHost == ScriptHostType.Report ? "reports" : "this context"), node);
            }
            if (match.ApiUseOnly && !Settings.ScriptHostIsApi)
            {
                throw ParseExceptionHelper.New(string.Format("The '{0}' function is internal.", token), node);
            }

            // Create an expression
            ExpressionNode calcNode = (ExpressionNode)Activator.CreateInstance(match.Type);
            calcNode.ResultType = match.ResultType.Clone();
            ExprType inputType = null;
            if (match.InputType != null)
                inputType = match.InputType.Clone();

            for (int i = 0; i < argList.Count; i++)
            {
                ExpressionNode argument = argList[i];
                if (match.ParameterTypes[i].TranformForOrdering)
                {
                    argument = TransformForOrdering(argument, ref inputType);
                }
                calcNode.SetArgument(i, argument);
            }
            calcNode.InputType = inputType;

            ExprType resultType = calcNode.ResultType;

            // EntityType hack
            // If the result is an entity, then copy the entity type info from the first parameter that is also an entity. Hmm.
            if (resultType.Type == DataType.Entity && resultType.EntityType == null)
            {
                resultType.EntityType = argList.Select(a => a.ResultType.EntityType).FirstOrDefault(et => et != null);
            }

            // Currency hack
            LanguageExprType langExprType = resultType as LanguageExprType;
            if (resultType.Type == DataType.Decimal && langExprType != null && langExprType.TypeGroup != TypeGroup.None || calcNode is DivideNode)
            {
                if (arguments.Select(a => a.ResultType.Type).Any(t => t == DataType.Currency))
                    resultType.Type = DataType.Currency;
            }

            // Decimal places
            // Copy number of decimal places. (Use whichever argument has the greatest precision)
            if (DataTypeHelper.IsDecimal(resultType.Type))
            {
                resultType.DecimalPlaces = calcNode.OnDetermineDecimalPlaces(Context);
            }

            // TODO... need to know if operators are deterministic
            calcNode.ResultType.Constant = argList.All(p => p.ResultType.Constant);
            calcNode.OnDetermineResultType(Settings);

            calcNode.SetToken(token);

            return calcNode;            
        }

        /// <summary>
        /// Performs transformations on an expression if it may be used for comparisons or ordering.
        /// </summary>
        /// <param name="node">The node that may need to be transformed.</param>
        /// <param name="resultType">An instruction to the fuction/operator about what type of evaluation to perform.</param>
        /// <returns></returns>
        private ExpressionNode TransformForOrdering(ExpressionNode node, ref ExprType resultType)
        {
            ExpressionNode result = node;
            EntityNode entityNode = node as EntityNode;

            if (entityNode != null && node.ResultType.Type == DataType.Entity)
            {
                IEntity entityType = EntityRepository.Get(node.ResultType.EntityType);
                bool isChoice = entityType != null && entityType.Is<EnumType>();
                if (isChoice)
                {
                    // For choice fields, add an indirection to look up the ordering field.
                    var memberInfo = new MemberInfo { MemberId = new EntityRef("core:enumOrder").Id, MemberType = MemberType.Field };
                    result = Compile_FieldAccess(entityNode, memberInfo, null);
                    resultType = result.ResultType;
                }
            }
            return result;
        }

        /// <summary>
        /// Handles the special case of comparing a choice-field to a string, and magically expecting it to work.
        /// </summary>
        /// <remarks>Returns null if not applicable, or the converted arguments</remarks>
        private ExpressionNode[] CheckForComparisonsBetweenEntitiesAndStrings(Function function, ExpressionNode[] arguments)
        {
            LanguageExprType[] parameters = function.ParameterTypes;

            // Ensure signature matches
            if (parameters.Length != 2 || arguments.Length != 2)
                return null;

            if (!parameters[0].TranformForOrdering || !parameters[1].TranformForOrdering)
                return null;

            if (parameters[0].Type != DataType.Entity || parameters[1].Type != DataType.Entity)
                return null;

            bool castRightScenario = arguments[0].ResultType.Type == DataType.Entity && arguments[1].ResultType.Type == DataType.String;
            bool castLeftScenario = arguments[0].ResultType.Type == DataType.String && arguments[1].ResultType.Type == DataType.Entity;

            if (!castLeftScenario && !castRightScenario)
                return null;

            // Determine entity type
            ExpressionNode entityArgument = arguments[castRightScenario ? 0 : 1];
            IEntityRef entityType = entityArgument.ResultType.EntityType;
            if (entityType == null)
                return null;

            IEntity entityTypeEntity = EntityRepository.Get(entityType);

            // Only handle string literals
            ConstantNode stringNode = arguments[castRightScenario ? 1 : 0] as ConstantNode;
            if (stringNode == null)
                return null;
            string enumValueName = stringNode.Value as string;

            // Look up literal value
            IEntity id = Factory.ScriptNameResolver.GetInstance(enumValueName, entityType.Id);

            // Load the enum value
            EnumValue enumValue = EntityRepository.Get(id).As<EnumValue>();
            if (enumValue == null)
                return null;

            // Get the ordering value
            ExpressionNode enumOrderValue = new ConstantNode
            {
                Value = enumValue.EnumOrder,
                ResultType = new ExprType { Type = DataType.Int32, Constant = true }
            };

            // Create a result
            ExpressionNode[] results = new ExpressionNode[2];
            results[0] = castRightScenario ? entityArgument : enumOrderValue;
            results[1] = castLeftScenario ? entityArgument : enumOrderValue;
            return results;
        }

    }

}

