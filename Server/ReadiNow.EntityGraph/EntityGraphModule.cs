// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using Autofac;
using ReadiNow.EntityGraph.GraphModel;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using ReadiNow.EntityGraph.Parser;

namespace ReadiNow.EntityGraph
{
    /// <summary>
    /// Autofac dependency injection module for query engine.
    /// </summary>
    public class EntityGraphModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // GraphEntityRepository
            // Note: we use property injection, because we have circular dependencies.  GraphRepo->AccessControl->QueryEngine->GraphRepo.
            builder
                .Register(
                    c => new IdResolvingEntityRepository(
                        new GraphEntityRepository( ) ) )
                .OnActivated(
                    evt => {
                        IdResolvingEntityRepository idRepos = evt.Instance;
                        GraphEntityRepository graphRepos = (GraphEntityRepository)idRepos.Inner;
                        graphRepos.EntityAccessControlService = evt.Context.Resolve<IEntityAccessControlService>();
                    })
                .Keyed<IEntityRepository>( "Graph" );

            // RequestParser
            builder
                .RegisterType<RequestParser>()
                .As<IRequestParser>();
                
        }
    }
}
