// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using Autofac;
using EDC.ReadiNow.Metadata.Query.Structured;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;

namespace EDC.ReadiNow.Security.AccessControl
{
    /// <summary>
    /// Autofac module for access control.
    /// </summary>
    public class EntityAccessControlModule: Module
    {
        private const string CachingSystemQueryRepository = "CachingSystemQueryRepository";
        private const string CachingDefaultQueryRepository = "CachingDefaultQueryRepository";
        public const string DefaultQueryRepository = "DefaultQueryRepository";
        public const string SystemQueryRepository = "SystemQueryRepository";

        /// <summary>
        /// Load the registrations.
        /// </summary>
        /// <param name="builder">
        /// The Autofac provided <see cref="ContainerBuilder"/>.
        /// </param>
        protected override void Load(ContainerBuilder builder)
        {
            // While RegisterType<T> is cleaner and easier than Register<T>(lambda), it is more efficient according to 
            // http://docs.autofac.org/en/latest/best-practices/index.html#register-frequently-used-components-with-lambdas.

            // IRuleRepository
            builder.Register(
                c => new RuleRepository( c.Resolve<IEntityRepository>() ) )
                .As<IRuleRepository>( );            

            // CachingSystemQueryRepository
            builder.Register(
                c => new CachingQueryRepository(
                    new SystemAccessRuleQueryRepository(new SystemAccessRuleQueryFactory())))
                .Named<CachingQueryRepository>(CachingSystemQueryRepository)
                .As<CachingQueryRepository>()
                .As<ICacheService>()
                .SingleInstance();

            // CachingQueryRepository
            builder.Register(
                c => new CachingQueryRepository(
                    new QueryRepository(
                        c.Resolve<IReportToQueryConverter>( ),
                        c.Resolve<IRuleRepository>( ),
                        c.ResolveNamed<IEntityRepository>( "Graph" ),
                        auth => true ) ) )
                .Named<CachingQueryRepository>(CachingDefaultQueryRepository)
                .As<CachingQueryRepository>()
                .As<ICacheService>()
                .SingleInstance();

            // IUserRoleRepository (caching)
            builder.Register(
                c => new CachingUserRoleRepository(new UserRoleRepository()))
                .As<CachingUserRoleRepository>()
                .As<IUserRoleRepository>()
                .As<ICacheService>( )
                .SingleInstance();

            // IQueryRepository  (role-checking, caching, default)
            builder.Register(
                c => new RoleCheckingQueryRepository(
                    c.ResolveNamed<CachingQueryRepository>(CachingDefaultQueryRepository), 
                    c.Resolve<CachingUserRoleRepository>()))
                .Named<IQueryRepository>(DefaultQueryRepository)
                .As<IQueryRepository>()
                .As<RoleCheckingQueryRepository>()
                .SingleInstance();

            // IQueryRepository  (role-checking, caching, system)
            builder.Register(
                c => new RoleCheckingQueryRepository(
                    c.ResolveNamed<CachingQueryRepository>(CachingSystemQueryRepository),
                    c.Resolve<CachingUserRoleRepository>()))
                .Named<IQueryRepository>(SystemQueryRepository)
                .As<IQueryRepository>()
                .As<RoleCheckingQueryRepository>()
                .SingleInstance();

            // IEntityTypeRepository
            builder.Register(
                c => new EntityTypeRepository())
                .As<EntityTypeRepository>()
                .As<IEntityTypeRepository>()
                .SingleInstance( );

            // IUserRuleSetProvider
            builder.Register(
                c => new CachingUserRuleSetProvider(
                    new UserRuleSetProvider(
                        c.Resolve<IUserRoleRepository>( ),
                        c.Resolve<IRuleRepository>( ) ) ) )
                .As<IUserRuleSetProvider>( )
                .As<ICacheService>( )
                .SingleInstance( );

            // IEntityMemberRequestFactory
            builder.Register(
                c => new CachingEntityMemberRequestFactory( new EntityMemberRequestFactory( ) ) )
                .As<CachingEntityMemberRequestFactory>( )
                .As<IEntityMemberRequestFactory>( )
                .As<ICacheService>( )
                .SingleInstance( );

            // IEntityAccessControlChecker
            LoadEntityAccessControlChecker( builder );

            // IEntityAccessControlService            
            builder.Register(
                c => new EntityAccessControlService(
                    c.Resolve<IEntityAccessControlChecker>() ))
                .As<IEntityAccessControlService>()
                .As<EntityAccessControlService>()
                .SingleInstance();

            // ITypeAccessReasonService          
            builder.Register(
                c => new TypeAccessReasonService(
                    c.ResolveNamed<IQueryRepository>( DefaultQueryRepository ),
                    c.Resolve<IEntityRepository>( ) )
                )
                .As<ITypeAccessReasonService>( )
                .As<TypeAccessReasonService>( )
                .SingleInstance( );
        }

        /// <summary>
        /// Load the registrations for the EntityAccessControlChecker.
        /// </summary>
        /// <param name="builder">
        /// The Autofac provided <see cref="ContainerBuilder"/>.
        /// </param>
        private static void LoadEntityAccessControlChecker( ContainerBuilder builder )
        {
            // Safety layer
            // This layer adds parameter checking for debug builds only
            Func<IEntityAccessControlChecker, IEntityAccessControlChecker> safetyLayer;
#if DEBUG
            safetyLayer = layer => new SafetyEntityAccessControlChecker( layer );
#else
            safetyLayer = layer => layer;
#endif

            // General structure:
            //                                         (per ruleset)                       (per ruleset)
            //                                     /-> caching -> securesFlag -> safety -> caching -> checker (with non-current-user query filter)
            // safety -> logging -> counter -> union
            //                                     \-> caching -> securesFlag -> safety -> caching -> checker (with current-user query filter)
            //                                         (per uesr)                          (per user)

            // Inner-PerRuleSet cache
            builder.Register(
                c => new CachingPerRuleSetEntityAccessControlChecker(
                    new IntersectingEntityAccessControlChecker(
                        new List<IEntityAccessControlChecker>
                        {
                            new SystemEntityAccessControlChecker(
                                c.Resolve<CachingUserRoleRepository>(),
                                c.ResolveNamed<CachingQueryRepository>(CachingSystemQueryRepository),
                                c.Resolve<EntityTypeRepository>()),
                            new EntityAccessControlChecker(
                                c.Resolve<CachingUserRoleRepository>(),
                                new FilteringQueryRepository(
                                    c.ResolveNamed<CachingQueryRepository>(CachingDefaultQueryRepository),
                                    QueryFilter.DoesNotReferToCurrentUser),
                                c.Resolve<EntityTypeRepository>())
                        }),
                    c.Resolve<IUserRuleSetProvider>(),
                    "Security cache (inner access rule only, per ruleset)"))
                .Named<CachingPerRuleSetEntityAccessControlChecker>("Inner-PerRuleSet")
                .As<ICacheService>()
                .SingleInstance();

            // Inner-PerUser cache
            builder.Register(
                c => new CachingEntityAccessControlChecker(
                    new IntersectingEntityAccessControlChecker(
                        new List<IEntityAccessControlChecker>
                        {
                            new SystemEntityAccessControlChecker(
                                c.Resolve<CachingUserRoleRepository>(),
                                c.ResolveNamed<CachingQueryRepository>(CachingSystemQueryRepository),
                                c.Resolve<EntityTypeRepository>()),
                            new EntityAccessControlChecker(
                                c.Resolve<CachingUserRoleRepository>(),
                                new FilteringQueryRepository(
                                    c.ResolveNamed<CachingQueryRepository>(CachingDefaultQueryRepository),
                                    QueryFilter.RefersToCurrentUser),
                                c.Resolve<EntityTypeRepository>())
                        }),
                    "Security cache (inner access rule only, per user)"))
                .Named<CachingEntityAccessControlChecker>("Inner-PerUser")
                .As<ICacheService>()
                .SingleInstance();

            // Outer-PerRuleSet cache
            builder.Register(
                c => new CachingPerRuleSetEntityAccessControlChecker(
                    new SecuresFlagEntityAccessControlChecker(
                        safetyLayer(
                            c.ResolveNamed<CachingPerRuleSetEntityAccessControlChecker>( "Inner-PerRuleSet" ) ),
                        c.Resolve<IEntityMemberRequestFactory>( ),
                        c.Resolve<IEntityTypeRepository>( )
                    ),
                    c.Resolve<IUserRuleSetProvider>( ),
                    "Security cache (outer, per ruleset)" ) )
                .Named<CachingPerRuleSetEntityAccessControlChecker>( "Outer-PerRuleSet" )
                .As<CachingPerRuleSetEntityAccessControlChecker>( )
                .As<ICacheService>( )
                .SingleInstance( );

            // Outer-PerUser cache
            builder.Register(
                c => new CachingEntityAccessControlChecker(
                    new SecuresFlagEntityAccessControlChecker(
                        safetyLayer(
                            c.ResolveNamed<CachingEntityAccessControlChecker>( "Inner-PerUser" ) ),
                        c.Resolve<IEntityMemberRequestFactory>( ),
                        c.Resolve<IEntityTypeRepository>( )
                    ),
                    "Security cache (outer, per user)" ) )
                .Named<CachingEntityAccessControlChecker>( "Outer-PerUser" )
                .As<CachingEntityAccessControlChecker>( )
                .As<ICacheService>( )
                .SingleInstance( );

            // Root
            builder.Register(
                c => safetyLayer(
                    new LoggingEntityAccessControlChecker(
                        new CounterEntityAccessControlChecker(
                            new UnionEntityAccessControlChecker(
                                c.ResolveNamed<CachingPerRuleSetEntityAccessControlChecker>( "Outer-PerRuleSet" ),
                                c.ResolveNamed<CachingEntityAccessControlChecker>( "Outer-PerUser" ),
                                true // 'CanCreate' only goes to first branch.
                                ) ) ) ) )
                .As<IEntityAccessControlChecker>( );
        }
    }
}
