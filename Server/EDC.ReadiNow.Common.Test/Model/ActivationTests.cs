// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Autofac;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
    public class ActivationTests
    {
        [Test]
        public void Autofac_EntityRepository( )
        {
            IEntityRepository instance = Factory.EntityRepository;
            Assert.That( instance, Is.InstanceOf<EntityRepository>( ) );
        }

        [Test]
        public void Autofac_IEntityRepository( )
        {
            IEntityRepository instance = Factory.Current.Resolve<IEntityRepository>( );
            Assert.That( instance, Is.InstanceOf<EntityRepository>( ) );
        }

        [Test]
        public void Autofac_IEntitySaver( )
        {
            IEntitySaver instance = Factory.Current.Resolve<IEntitySaver>( );
            Assert.That( instance, Is.InstanceOf<EntitySaver>( ) );
        }

        [Test]
        public void Autofac_IEntityResolverProvider( )
        {
            IEntityResolverProvider instance = Factory.Current.Resolve<IEntityResolverProvider>( );
            Assert.That( instance, Is.InstanceOf<EntityResolverProvider>( ) );
        }

        [Test]
        public void Autofac_IEntityDefaultsDecoratorProvider( )
        {
            IEntityDefaultsDecoratorProvider instance = Factory.Current.Resolve<IEntityDefaultsDecoratorProvider>( );
            Assert.That( instance, Is.InstanceOf<CachingEntityDefaultsDecoratorProvider>( ) );

            CachingEntityDefaultsDecoratorProvider cache = ( CachingEntityDefaultsDecoratorProvider ) instance;
            Assert.That( cache.InnerProvider, Is.InstanceOf<EntityDefaultsDecoratorProvider>( ) );
        }
    }
}
