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
using EDC.Common;
using ReadiNow.Expressions.Tree.Nodes;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;

namespace ReadiNow.Expressions.Compiler
{
    /// <summary>
    /// Main API for interracting with the expression engine.
    /// </summary>
    public class ExpressionEngine : IExpressionCompiler, IExpressionParseTreeCompiler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExpressionEngine(IEntityRepository entityRepository, IScriptNameResolver scriptNameResolver)
        {
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            if (scriptNameResolver == null)
                throw new ArgumentNullException("scriptNameResolver");

            EntityRepository = entityRepository;
            ScriptNameResolver = scriptNameResolver;
        }

        /// <summary>
        /// Repository used to load information about schema entities.
        /// </summary>
        private IEntityRepository EntityRepository { get; set; }

        /// <summary>
        /// Service used to resolve identifiers.
        /// </summary>
        private IScriptNameResolver ScriptNameResolver { get; set; }


        /// <summary>
        /// Convert a string into an expression tree and perform static evaluation.
        /// </summary>
        /// <param name="script">The script to parse.</param>
        /// <param name="settings">Additional settings that control parsing. In particular, contains the type of the root context object, if any.</param>
        /// <returns>An expression tree.</returns>
        public IExpression Compile(string script, BuilderSettings settings)
        {
            using ( EntryPointContext.AppendEntryPoint( "ExprCompile" ) )
            using ( new SecurityBypassContext( ) )
            using ( Profiler.Measure("ExpressionEngine.Compile") )
            {
                // Parse the expression
                ParseTree parseTree = ExpressionGrammar.ParseMacro(script);
                ParseTreeNode parseRoot = parseTree.Root;

                // Compile the parse tree
                IExpression result = CompileParseTreeNode(parseRoot, settings);
                return result;
            }
        }


        /// <summary>
        /// Compile an expression from a parse tree node.
        /// </summary>
        /// <param name="parseTreeNode">An Irony parse tree node.</param>
        /// <param name="settings">Compilation settings.</param>
        /// <returns>An expression tree.</returns>
        public IExpression CompileParseTreeNode(ParseTreeNode parseTreeNode, BuilderSettings settings)
        {
            if (parseTreeNode == null)
                throw new ArgumentNullException("parseTreeNode");

            // Build the expression
            var builder = new StaticBuilder(EntityRepository, ScriptNameResolver);
            builder.Settings = settings;
            Expression result = builder.CompileTree(parseTreeNode);
            return result;
        }


        /// <summary>
        /// Creates a structured query expression.
        /// </summary>
        /// <remarks>
        /// This method does not add the new expression to the target structured query, however it will add new tree nodes to the structured query as they are requred.
        /// </remarks>
        public ScalarExpression CreateQueryEngineExpression( IExpression expression, QueryBuilderSettings settings )
        {
            if ( expression == null )
                throw new ArgumentNullException( "expression" );

            Expression expressionToCreate = expression as Expression;
            if ( expressionToCreate == null )
                throw new ArgumentException( "Expected instance of type Expression.", "expression" );

            using ( EntryPointContext.AppendEntryPoint( "ExprQuery" ) )
            using ( new SecurityBypassContext( ) )
            using ( Profiler.Measure( "ExpressionEngine.CreateQueryEngineExpression" ) )
            {
                if (settings == null)
                    settings = new QueryBuilderSettings();

                var queryContext = new QueryBuilderContext
                {
                    Settings = settings
                };

                // Add nodes to the query tree, if necessary.
                expressionToCreate.ListRoot.BuildQueryNode(queryContext, true);

                // Build up the expression itself
                ScalarExpression result = expressionToCreate.Root.BuildQuery(queryContext);

                // Special handing for bools
                //if (expressionToCreate.ResultType.Type == DataType.Bool)
                //{
                //    if (settings.ConvertBoolsToString)
                //        result = CastFromBool.CastToString(result);
                //}

                // Set result type
                // TODO: return entity type
                // TODO: return for any expression type
                var calcExpression = result as CalculationExpression;
                if (calcExpression != null)
                {
                    calcExpression.DisplayType = ExprTypeHelper.ToDatabaseType( expressionToCreate.ResultType );
                }

                return result;
            }
        }

        /// <summary>
        /// Determine entities that the expression itself depend on. (The compiled expression, not the calculated results).
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public CalculationDependencies GetCalculationDependencies(IExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Expression expr = expression as Expression;
            if (expr == null)
                throw new ArgumentException("Expected instance of type Expression.", "expression");

            // Get all nodes
            IEnumerable<ExpressionNode> exprNodes;
            exprNodes = Delegates.WalkGraph(expr.Root, exprNode => exprNode.Arguments);

            HashSet<long> identifiedEntities = new HashSet<long>();
            HashSet<long> fields = new HashSet<long>();
            HashSet<long> relationships = new HashSet<long>();

            foreach (ExpressionNode node in exprNodes)
            {
                node.RegisterDependencies(identifiedEntities, fields, relationships);
            }

            // TODO: When we update to .Net 4.5.1, take advantage of HashSet supporting IReadOnlyCollection
            CalculationDependencies res = new CalculationDependencies(identifiedEntities.ToList(), fields.ToList(), relationships.ToList());
            return res;
        }

        /// <summary>
        /// Prewarm the expression engine.
        /// </summary>
        public void Prewarm()
        {
            // Fill the grammar object pool with at least one item
            ExpressionGrammar.Prewarm();

            // Pre load expression language database
            var language = LanguageManager.Instance;
            if (language == null)
                throw new InvalidOperationException("Expected language manager");
        }



    }

}
