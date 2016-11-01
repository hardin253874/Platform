// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using Autofac.Extras.AttributeMetadata;
using System;

using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Core;
using ICacheService = EDC.ReadiNow.Cache.ICacheService;
using ReadiNow.Connector.Payload;
using ReadiNow.Connector.Service;
using ReadiNow.Connector.Interfaces;
using EDC.ReadiNow.Model;
using ReadiNow.Connector.EndpointTypes;

namespace ReadiNow.Connector
{
    /// <summary>
    /// Autofac dependency injection module for data connector.
    /// </summary>
    public class ConnectorModule : Module
    {
        /// <summary>
        /// Perform any registrations
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load( ContainerBuilder builder )
        {
            // JSON Reader
            builder.RegisterType<JilDynamicObjectReaderService>( )
                .As<IDynamicObjectReaderService>( );

            // Main service
            builder.RegisterType<ConnectorService>( );

            // IConnectorService stack
            builder.Register(
                context => new ExceptionServiceLayer(
                    new ApiKeySecurity(
                        context.Resolve<ConnectorService>( ),
                        context.Resolve<IEndpointResolver>( ),
                        context.Resolve<IEntityRepository>( )
                    ) ) )
                .As<IConnectorService>( );

            // JSON Reader
            builder.RegisterType<EndpointResolver>( )
                .As<IEndpointResolver>( );

            // Reader-entity adapter factory
            builder.RegisterType<ReaderToEntityAdapterProvider>( )
                .As<IReaderToEntityAdapterProvider>( );

            // Resource Uri generator
            builder.RegisterType<ResourceUriGenerator>( )
                .As<IResourceUriGenerator>( );

            // Resource resolver provider implementation
            builder.RegisterType<ResourceResolverProvider>( )
                .As<IResourceResolverProvider>( );

            // Resource endpoint implementation
            builder.RegisterType<ResourceEndpoint>( )
                .As<ResourceEndpoint>( );

            // Spreadsheet endpoint implementation
            builder.RegisterType<SpreadsheetEndpoint>( )
                .As<SpreadsheetEndpoint>( );

        }
    }
}
