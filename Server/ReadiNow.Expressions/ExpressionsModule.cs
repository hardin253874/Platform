// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Autofac;
using Autofac.Extras.AttributeMetadata;

using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Evaluation;
using ReadiNow.Expressions.CalculatedFields;
using ReadiNow.Expressions.NameResolver;
using EDC.ReadiNow.Cache;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Core;

namespace ReadiNow.QueryEngine
{
    /// <summary>
    /// Autofac dependency injection module for expression engine.
    /// </summary>
    public class ExpressionsModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // ExpressionEngine
            builder.RegisterType<ExpressionEngine>( )
                .As<IExpressionCompiler>()
                .As<IExpressionParseTreeCompiler>();

            // ExpressionRunner
            builder.RegisterType<ExpressionRunner>()
                .As<IExpressionRunner>();

            // ScriptNameResolver
            // note: the CachingQueryRunner is unnecessary here because we are already caching, and it can cause invalidation issues
            builder.Register(
                ctx => new ScriptNameResolver(
                    ctx.ResolveNamed<IEntityRepository>("Graph"),
                    ctx.ResolveKeyed<IQueryRunner>(Factory.NonCachedKey)))
                .As<ScriptNameResolver>()
                .Keyed<IScriptNameResolver>(Factory.NonCachedKey);

            // CachingScriptNameResolver
            builder.RegisterType<CachingScriptNameResolver>()
                .WithAttributeFilter()
                .As<IScriptNameResolver>()
                .As<ICacheService>()
                .SingleInstance();

            // CalculatedFieldMetadataProvider
            builder.RegisterType<CalculatedFieldMetadataProvider>();

            // CachingCalculatedFieldMetadataProvider 
            builder.Register(
                ctx => new CachingCalculatedFieldMetadataProvider(
                    ctx.Resolve<CalculatedFieldMetadataProvider>() ) )
                .As<ICalculatedFieldMetadataProvider>()
                .As<ICacheService>()
                .SingleInstance();

            // CalculatedFieldProvider
            builder.RegisterType<CalculatedFieldProvider>()
                .As<ICalculatedFieldProvider>();
        }
    }
}
