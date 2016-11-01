// Copyright 2011-2016 Global Software Innovation Pty Ltd
extern alias EdcReadinowCommon;
using EntityType = EdcReadinowCommon::EDC.ReadiNow.Model.EntityType;

using System;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Parser;
using EDC.ReadiNow.Model;
using ReadiNow.DocGen.DataSources;
using Irony.Parsing;

namespace ReadiNow.DocGen.MacroParser
{
    /// <summary>
    /// Various methods for converting parse tree nodes.
    /// </summary>
    class ParseTreeHelper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="externalServices">Provider for resolving script names.</param>
        internal ParseTreeHelper(ExternalServices externalServices )
        {
            if (externalServices == null)
                throw new ArgumentNullException("externalServices");

            ExternalServices = externalServices;
        }

        /// <summary>
        /// DI provider for resolving script names.
        /// </summary>
        internal ExternalServices ExternalServices { get; private set; }

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        /// <param name="expressionParseNode">The parse node of the expression.</param>
        /// <param name="context">Context information.</param>
        /// <param name="requiredResultType">Required result type.</param>
        /// <returns>An expression.</returns>
        internal IExpression ParseExpression(ParseTreeNode expressionParseNode, ReaderContext context, ExprType requiredResultType)
        {
            BuilderSettings settings = new BuilderSettings
            {
                ExpectedResultType = requiredResultType,
                RootContextType = new ExprType
                {
                    EntityType = context.CurrentEntityType,
                    Type = DataType.Entity
                },
                ScriptHost = ScriptHostType.Evaluate
            };
            
            IExpression expression = ExternalServices.ExpressionCompiler.CompileParseTreeNode(expressionParseNode, settings);
            return expression;
        }

        /// <summary>
        /// Converts an entity identifier parse node to its string.
        /// </summary>
        /// <param name="entityNode">The list source parse tree node.</param>
        /// <returns>The name referred to in the entity.</returns>
        internal string ConvertEntity(ParseTreeNode entityNode)
        {
            if (entityNode == null)
                throw new ArgumentNullException("entityNode");
            if (entityNode.Term.Name != Terms.Identifier)
                throw new ArgumentException("Expected Identifier", "entityNode");

            return entityNode.Token.ValueString;
        }


		/// <summary>
		/// Converts a list source parse node to the actual DataSource object.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="listSourceNode">The list source parse tree node.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">listSourceNode</exception>
		/// <exception cref="System.ArgumentException">Expected ListSource;listSourceNode</exception>
		/// <exception cref="System.InvalidOperationException"></exception>
        internal DataSource ConvertListSource(ReaderContext context, ParseTreeNode listSourceNode)
        {
            if (listSourceNode == null)
                throw new ArgumentNullException("listSourceNode");
            if (listSourceNode.Term.Name != DocTerms.ListSource)
                throw new ArgumentException("Expected ListSource", "listSourceNode");

            string sourceKeyword = listSourceNode.ChildNodes[0].Term.Name;

            switch (sourceKeyword)
            {
                case Terms.Expression:
                    IExpression expr = ParseExpression(listSourceNode.ChildNodes[0], context, new ExprType { Type = DataType.Entity, IsList = true });
                    return new ExpressionSource { Expression = expr };

                case Keywords.Load:
                    var typeArg2 = GetEntityArg<EntityType>(listSourceNode, 1, EntityType.EntityType_Type);
                    var instArg = GetEntityArg<IEntity>(listSourceNode, 2, typeArg2);
                    var instSource = new LoadInstanceSource { Instance = instArg, EntityType = typeArg2 };
                    return instSource;

                case Keywords.TestData:
                    return new TestSource();

                default:
                    throw new InvalidOperationException(sourceKeyword + " is not a recognised source keyword.");
            }

        }


        /// <summary>
        /// Gets the entity arg.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="pos">The child node index containing the argument.</param>
        /// <returns>The entity. Or the first one, if there are multiple matches. Or null if there was no match.</returns>
        internal string GetIdentifierArg(ParseTreeNode parentNode, int pos)
        {
            if (parentNode.ChildNodes == null || parentNode.ChildNodes.Count <= pos)
                throw new ArgumentOutOfRangeException("pos");

            // Check the code name
            string codeName = ConvertEntity(parentNode.ChildNodes[pos]);
            if (string.IsNullOrEmpty(codeName))
                throw new Exception("Name was empty.");

            return codeName;
        }


        /// <summary>
        /// Gets the entity arg.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="pos">The child node index containing the argument.</param>
        /// <param name="type">The type of entity expected.</param>
        /// <returns>The entity. Or the first one, if there are multiple matches. Or null if there was no match.</returns>
        internal T GetEntityArg<T>(ParseTreeNode parentNode, int pos, EntityType type) where T : class, IEntity
        {
            // Get the identifier name
            ParseTreeNode nameNode = parentNode.ChildNodes[pos];
            string codeName = GetIdentifierArg(parentNode, pos);

            // Get the entity
            IEntity entity;
            if (type.Alias == "core:type")
            {
                long typeEntityId = ExternalServices.ScriptNameResolver.GetTypeByName(codeName);
                if (typeEntityId == 0)
                    throw ParseExceptionHelper.New(string.Format("The name '{0}' could not be matched.", codeName), nameNode);                
                entity = ExternalServices.EntityRepository.Get(typeEntityId);
            }
            else
            {
                entity = ExternalServices.ScriptNameResolver.GetInstance(codeName, type.Id);
                if (entity == null)
                    throw ParseExceptionHelper.New(string.Format("The name '{0}' could not be matched.", codeName), nameNode);
            }                


            // Cast to expected type
            T castedEntity = entity.Cast<T>();
            return castedEntity;
        }


        /// <summary>
        /// Returns true if the node refers to an Entity node.
        /// </summary>
        internal bool IsEntity(ParseTreeNode node)
        {
            return node.Term is IdentifierTerminal;
        }



    }
}
