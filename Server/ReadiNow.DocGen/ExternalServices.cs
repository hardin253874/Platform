// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;
using System;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// A convenient container for resolved services, so we don't need to specify all at each constructor.
    /// </summary>
    public class ExternalServices
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entityRepository">Entity repository.</param>
        /// <param name="expressionCompiler">The expression compiler.</param>
        /// <param name="scriptNameResolver">Script name resolver.</param>
        /// <param name="expressionRunner">Expression runner.</param>
        /// <exception cref="System.ArgumentNullException">
        /// entityRepository
        /// or
        /// expressionCompiler
        /// or
        /// expressionRunner
        /// or
        /// scriptNameResolver
        /// </exception>
        public ExternalServices(IEntityRepository entityRepository, IExpressionParseTreeCompiler expressionCompiler, IScriptNameResolver scriptNameResolver, IExpressionRunner expressionRunner)
        {
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            if (expressionCompiler == null)
                throw new ArgumentNullException("expressionCompiler");
            if (expressionRunner == null)
                throw new ArgumentNullException("expressionRunner");
            if (scriptNameResolver == null)
                throw new ArgumentNullException("scriptNameResolver");

            EntityRepository = entityRepository;
            ExpressionCompiler = expressionCompiler;
            ExpressionRunner = expressionRunner;
            ScriptNameResolver = scriptNameResolver;
        }

        /// <summary>
        /// Entity repository.
        /// </summary>
        public IEntityRepository EntityRepository { get; private set; }

        /// <summary>
        /// Expression compiler.
        /// </summary>
        public IExpressionParseTreeCompiler ExpressionCompiler { get; private set; }

        /// <summary>
        /// Expression runner.
        /// </summary>
        public IExpressionRunner ExpressionRunner { get; private set; }

        /// <summary>
        /// Script name resolver.
        /// </summary>
        public IScriptNameResolver ScriptNameResolver { get; private set; }
    }
}
