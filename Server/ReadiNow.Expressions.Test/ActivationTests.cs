// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.CalculatedFields;
using ReadiNow.Expressions.NameResolver;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Test
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void ExpressionCompiler_Instance( )
        {
            IExpressionCompiler instance = Factory.ExpressionCompiler;
            Assert.That( instance, Is.Not.Null );
        }

        [Test]
        public void ExpressionRunner_Instance( )
        {
            IExpressionRunner instance = Factory.ExpressionRunner;
            Assert.That( instance, Is.Not.Null );
        }

        [Test]
        public void CalculatedFieldProvider_Instance()
        {
            ICalculatedFieldProvider instance = Factory.CalculatedFieldProvider;
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void CalculatedFieldMetadataProvider_Instance()
        {
            ICalculatedFieldMetadataProvider instance = Factory.CalculatedFieldMetadataProvider;
            Assert.That(instance, Is.InstanceOf<CachingCalculatedFieldMetadataProvider>());

            CachingCalculatedFieldMetadataProvider provider = instance as CachingCalculatedFieldMetadataProvider;

            Assert.That(provider.InnerProvider, Is.InstanceOf<CalculatedFieldMetadataProvider>());
        }

        [Test]
        public void ScriptNameResolver_Instance()
        {
            // Check caching resolver
            IScriptNameResolver instance = Factory.ScriptNameResolver;
            Assert.That(instance, Is.InstanceOf<CachingScriptNameResolver>());

            // Check inner resolver
            CachingScriptNameResolver cachingResolver = instance as CachingScriptNameResolver;
            IScriptNameResolver innerProvider = cachingResolver.InnerProvider;
            Assert.That(innerProvider, Is.InstanceOf<ScriptNameResolver>());

            // Check inner has graph entity repository
            ScriptNameResolver resolver = innerProvider as ScriptNameResolver;
            Type graphRepositoryType = Factory.GraphEntityRepository.GetType();
            Assert.That(resolver.EntityRepository, Is.InstanceOf(graphRepositoryType));

            // Check inner has query runner (and not the caching one)
            Type nonCachingQueryRunnerType = Factory.Current.ResolveKeyed<IQueryRunner>(Factory.NonCachedKey).GetType();
            Assert.That(resolver.QueryRunner, Is.InstanceOf(nonCachingQueryRunnerType));
        }
    }
}
